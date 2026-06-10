using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Enemy : Mover
{
    private bool isAlive = true;
    [Header("------ Settings ------")]
    public bool canRespawn = true;
    public float timeToRespawn = 10f;

    [Header("------ XP Value ------")]
    public int xpValue = 1;

    [Header("------ Chasing ------")]
    public float speedMultiple = 0.75f;
    public float triggerLength = 1.0f;
    public float chaseLength = 1.0f;
    public bool chasing;
    public bool collidingWithPlayer;

    private Transform playTransform;
    private Vector3 startingPosition;

    [Header("------ State Sprites ------")]
    public SpriteRenderer enemyStateSprite;
    public List<Sprite> stateSprites;

    public ContactFilter2D filter;
    private BoxCollider2D hitBox;
    private Collider2D[] hits = new Collider2D[10];

    public bool drawTriggerLength;

    protected override void Start()
    {
        base.Start();
        startingPosition = transform.position;

        if (transform.childCount > 0)
            hitBox = transform.GetChild(0).GetComponent<BoxCollider2D>();

        CloseStateSprite();
    }

    protected Transform FindClosestPlayer()
    {
        // Ищем по компоненту Player вместо тега
        Player[] playerComponents = FindObjectsOfType<Player>();
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (Player p in playerComponents)
        {
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }
        return closest;
    }

    protected virtual void Update()
    {
        collidingWithPlayer = false;

        if (drawTriggerLength)
        {
            Debug.DrawLine(transform.position, new Vector3(transform.position.x + triggerLength, transform.position.y, transform.position.z), Color.green);
            Debug.DrawLine(transform.position, new Vector3(transform.position.x - triggerLength, transform.position.y, transform.position.z), Color.green);
            Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y + triggerLength, transform.position.z), Color.green);
            Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - triggerLength, transform.position.z), Color.green);
        }

        if (hitBox != null)
        {
            hitBox.Overlap(filter, hits);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null) continue;

                // Ищем Player по компоненту вместо тега
                if (hits[i].GetComponent<Player>() != null ||
                    hits[i].GetComponentInParent<Player>() != null)
                {
                    collidingWithPlayer = true;
                }
                hits[i] = null;
            }
        }
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            return;

        if (isAlive)
            ChasingTarget();
        else
            pushDirection = Vector3.zero;
    }

    protected virtual void ChasingTarget()
    {
        playTransform = FindClosestPlayer();

        if (playTransform == null && !PhotonNetwork.IsConnected)
        {
            if (GameManager.instance != null && GameManager.instance.player != null)
                playTransform = GameManager.instance.player.transform;
        }

        if (playTransform == null)
        {
            UpdateMotor((startingPosition - transform.position), speedMultiple);
            chasing = false;
            CloseStateSprite();
            return;
        }

        bool isTargetAlive = true;
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Player targetPlayer = playTransform.GetComponent<Player>();
            isTargetAlive = targetPlayer == null || targetPlayer.isAlive;
        }

        if ((Vector3.Distance(playTransform.position, startingPosition) < chaseLength) && isTargetAlive)
        {
            if (Vector3.Distance(playTransform.position, startingPosition) < triggerLength)
                chasing = true;

            if (chasing)
            {
                OpenStateSprite();
                if (!collidingWithPlayer)
                    UpdateMotor((playTransform.position - transform.position).normalized, speedMultiple);
            }
            else
            {
                UpdateMotor((startingPosition - transform.position), speedMultiple);
                CloseStateSprite();
            }
        }
        else
        {
            UpdateMotor((startingPosition - transform.position), speedMultiple);
            chasing = false;
            CloseStateSprite();
        }
    }

    private void OpenStateSprite()
    {
        if (enemyStateSprite == null || stateSprites == null || stateSprites.Count < 2) return;

        enemyStateSprite.enabled = true;
        if ((float)hitPoint / (float)maxHitPoint < 0.5f)
            enemyStateSprite.sprite = stateSprites[1];
        else
            enemyStateSprite.sprite = stateSprites[0];
    }

    private void CloseStateSprite()
    {
        if (enemyStateSprite != null)
            enemyStateSprite.enabled = false;
    }

    [PunRPC]
    public void RPC_NetworkTakeDamage(int damageAmount, Vector3 origin, float pushForce)
    {
        Damag dmg = new Damag
        {
            damageAmount = damageAmount,
            origin = origin,
            pushForce = pushForce
        };
        ReceiveDamage(dmg);
    }

    protected override void Death()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.GrantXP(xpValue);
                GameManager.instance.ShowText("+" + xpValue + " xp", 30, Color.magenta, transform.position, Vector3.up * 40, 1.0f);
            }
        }

        if (canRespawn)
        {
            isAlive = false;
            if (hitBox != null) hitBox.enabled = false;
            CloseStateSprite();

            var col = GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = false;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            StartCoroutine("WaitingForRespawn");
        }
        else
        {
            PhotonView myView = GetComponent<PhotonView>();
            if (myView != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    IEnumerator WaitingForRespawn()
    {
        yield return new WaitForSeconds(timeToRespawn);
        isAlive = true;
        if (hitBox != null) hitBox.enabled = true;

        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = true;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;

        OpenStateSprite();
        hitPoint = maxHitPoint;
        gameObject.transform.position = startingPosition;
    }
}