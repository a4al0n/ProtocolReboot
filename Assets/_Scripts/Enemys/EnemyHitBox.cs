using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyHitBox : Collectable
{
    [Header("------ Settings ------")]
    public int damage;
    public float pushForce;

    protected override void OnCollide(Collider2D coll)
    {
        // Rule 4: CompareTag instead of name check
        if (coll.CompareTag("Player"))
        {
            PhotonView pv = coll.GetComponent<PhotonView>();
            // Rule 2 & 4: Only apply damage to the player if it is the local player (IsMine)
            if (pv != null && pv.IsMine)
            {
                Damag dmg = new Damag
                {
                    damageAmount = damage,
                    origin = transform.position,
                    pushForce = pushForce
                };

                coll.SendMessage("ReceiveDamage", dmg);
            }
        }
    }
}