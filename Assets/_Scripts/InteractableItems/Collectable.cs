using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Collectable : Colliderable
{
    protected bool collected = false;

    protected override void OnCollide(Collider2D coll)
    {
        // Rule 4: CompareTag instead of name check
        if (coll.CompareTag("Player"))
        {
            PhotonView pv = coll.GetComponent<PhotonView>();
            // Only local player initiates collection
            if (pv != null && pv.IsMine)
            {
                PhotonView myView = GetComponent<PhotonView>();
                if (myView != null)
                {
                    myView.RPC("CollectRPC", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
                }
                else
                {
                    CollectRPC(0); // offline fallback
                }
            }
        }
    }

    [PunRPC]
    protected virtual void CollectRPC(int actorNumber)
    {
        if (!collected)
        {
            OnCollect(actorNumber);
        }
    }

    protected virtual void OnCollect(int actorNumber)
    {
        collected = true;
    }
}