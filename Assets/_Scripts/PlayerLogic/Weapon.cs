// Weapon.cs
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Weapon : Colliderable
{
    [Header("------ Damage ------")]
    public int[] damagePoint = { 1, 2, 3, 4, 5, 6, 7 };
    public float[] pushForce = { 2.0f, 2.2f, 2.5f, 3.0f, 3.3f, 3.6f, 4.0f };

    [Header("------ WeaponLevel ------")]
    public int weaponLevel = 0;
    private SpriteRenderer SpriteRenderer;

    [Header("------ Swing ------")]
    public Animator animator;
    private float swingCoolDown = 0.4f;
    private float lastSwing;

    [Header("------ Rage ------")]
    public GameObject flamingSword;
    public GameObject rageState;
    public bool CanRageSkill = false;
    public bool raging = false;
    public float ragingTime = 4f;

    private Player player;
    private PhotonView playerView;

    // Оружие наносит урон только во время свинга
    private bool _isSwinging = false;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        if (rageState != null)
            rageState.SetActive(false);
        InitializePlayerComponents();
    }

    private void InitializePlayerComponents()
    {
        if (player == null)
        {
            player = GetComponentInParent<Player>();
            if (player != null)
                playerView = player.GetComponent<PhotonView>();
        }
    }

    protected override void Update()
    {
        InitializePlayerComponents();

        // base.Update() только для не-оружийных Colliderable (EnemyHitBox, Portal и т.д.)
        // Для оружия коллизии активны только во время свинга
        if (_isSwinging)
            base.Update();

        if (GameManager.instance == null || player == null || !player.isAlive)
            return;

        if (playerView != null && !playerView.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastSwing > swingCoolDown)
            {
                lastSwing = Time.time;
                Swing();

                if (raging)
                    CreateFlamingSword();
                else if (rageState != null)
                    rageState.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !raging)
        {
            if (CanRageSkill)
            {
                raging = true;
                if (rageState != null)
                    rageState.SetActive(true);
                StartCoroutine("WaitingForRestRageSkill");
            }
        }
    }

    private void Swing()
    {
        if (animator != null)
            animator.SetTrigger("Swing");
        StartCoroutine(SwingWindow());
    }

    // Окно урона — 0.3 секунды во время свинга
    private IEnumerator SwingWindow()
    {
        _isSwinging = true;
        yield return new WaitForSeconds(0.3f);
        _isSwinging = false;
    }

    protected override void OnCollide(Collider2D coll)
    {
        if (!_isSwinging) return;
        if (playerView != null && !playerView.IsMine) return;

        if (coll.GetComponent<Player>() != null ||
            coll.GetComponentInParent<Player>() != null) return;

        // Ищем Fighter на объекте или родителе
        Fighter fighter = coll.GetComponent<Fighter>();
        GameObject targetRoot = coll.gameObject;
        if (fighter == null)
        {
            fighter = coll.GetComponentInParent<Fighter>();
            if (fighter != null)
                targetRoot = fighter.gameObject;
        }
        if (fighter == null) return;

        Damag dmg = new Damag
        {
            damageAmount = damagePoint[weaponLevel],
            origin = transform.position,
            pushForce = pushForce[weaponLevel]
        };

        // Берём PhotonView с корневого объекта где находится Fighter и RPC метод
        PhotonView targetPv = targetRoot.GetComponent<PhotonView>();

        if (targetPv != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            targetPv.RPC("RPC_NetworkTakeDamage", RpcTarget.All,
                dmg.damageAmount, dmg.origin, dmg.pushForce);
        }
        else
        {
            fighter.SendMessage("ReceiveDamage", dmg, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void CreateFlamingSword()
    {
        if (flamingSword != null)
            Instantiate(flamingSword);
    }

    public void UpgradeWeapon()
    {
        weaponLevel++;
        if (SpriteRenderer != null && GameManager.instance != null &&
            GameManager.instance.weaponSprites != null &&
            weaponLevel < GameManager.instance.weaponSprites.Count)
            SpriteRenderer.sprite = GameManager.instance.weaponSprites[weaponLevel];

        if (playerView != null && playerView.IsMine)
            playerView.RPC("RPC_SyncWeaponLevel", RpcTarget.OthersBuffered, weaponLevel);
    }

    public void SetWeaponLevel(int level)
    {
        weaponLevel = level;
        if (SpriteRenderer != null && GameManager.instance != null &&
            GameManager.instance.weaponSprites != null &&
            level < GameManager.instance.weaponSprites.Count)
            SpriteRenderer.sprite = GameManager.instance.weaponSprites[weaponLevel];

        if (playerView != null && playerView.IsMine)
            playerView.RPC("RPC_SyncWeaponLevel", RpcTarget.OthersBuffered, weaponLevel);
    }

    IEnumerator WaitingForRestRageSkill()
    {
        yield return new WaitForSeconds(ragingTime);
        raging = false;
        CanRageSkill = false;
        if (player != null)
            player.rage = 0;
        if (GameManager.instance != null)
            GameManager.instance.OnUIChange();
    }

    public void EnableWeaponCollider()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = true;
    }

    public void DisableWeaponCollider()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = false;
    }
}