using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Crate : Fighter
{
    private void Start()
    {
        ImmuneTime = 0.5f;
    }

    protected override void ReceiveDamage(Damag dmg)
    {
        if (Time.time - lastImmune > ImmuneTime)
        {
            lastImmune = Time.time;
            hitPoint -= dmg.damageAmount;
            pushDirection = (transform.position - dmg.origin).normalized * dmg.pushForce;
        }

        if (hitPoint <= 0)
        {
            hitPoint = 0;
            Death();
        }
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
        PhotonView myView = GetComponent<PhotonView>();
        if (myView != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}