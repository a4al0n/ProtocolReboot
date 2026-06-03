using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FlamingSword : MonoBehaviour
{
    private Vector3 originalSize;    
    private float nowTime;              
    public float lifeTime = 0.5f;       

    public ContactFilter2D filter;
    private BoxCollider2D boxCollider;
    private Collider2D[] hits = new Collider2D[10];

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        // Defensive checks according to Rule 2
        if (GameManager.instance == null || GameManager.instance.player == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = GameManager.instance.player.transform.position;
        pos.y = GameManager.instance.player.transform.position.y - 0.02f;

        originalSize = GetComponent<Transform>().localScale;
        if (GameManager.instance.player.transform.localScale.x > 0)
        {
            transform.localScale = originalSize;
            pos.x += 0.1f;
        }           
        else if (GameManager.instance.player.transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(originalSize.x * -1, originalSize.y, originalSize.y);
            pos.x -= 0.1f;
        }

        nowTime = Time.time;
        gameObject.transform.position = pos;
    }

    private void Update()
    {
        if (Time.time - nowTime > lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (boxCollider == null) return;

        boxCollider.Overlap(filter, hits);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null)
                continue;

            OnCollide(hits[i]);

            hits[i] = null;
        }

        transform.position += Vector3.right * Time.deltaTime * transform.localScale.x * 3f;
    }

    private void OnCollide(Collider2D coll)
    {
        if (coll.CompareTag("Fighter"))
        {
            // Do not hit player
            if (coll.CompareTag("Player"))
                return;

            if (GameManager.instance == null || GameManager.instance.weapon == null)
                return;

            int level = GameManager.instance.weapon.weaponLevel;
            int dmgAmt = 0;
            float pushF = 0f;

            if (level < GameManager.instance.weapon.damagePoint.Length)
                dmgAmt = GameManager.instance.weapon.damagePoint[level] * 2;
            if (level < GameManager.instance.weapon.pushForce.Length)
                pushF = GameManager.instance.weapon.pushForce[level] * 2;

            Damag dmg = new Damag
            {          
                damageAmount = dmgAmt,
                origin = transform.position,
                pushForce = pushF
            };

            // Route damage via RPC if target is network-synchronized
            PhotonView targetPv = coll.GetComponent<PhotonView>();
            if (targetPv != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                targetPv.RPC("RPC_NetworkTakeDamage", RpcTarget.All, dmg.damageAmount, dmg.origin, dmg.pushForce);
            }
            else
            {
                coll.SendMessage("ReceiveDamage", dmg);
            }
        }
    }
}