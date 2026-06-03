using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Door : Colliderable
{
    public Sprite doorOpenSprite;

    protected override void OnCollide(Collider2D coll)
    {
        // Rule 4: CompareTag instead of name check
        if (coll.CompareTag("Player"))
        {
            PhotonView playerView = coll.GetComponent<PhotonView>();
            // Only local player initiates door opening
            if (playerView != null && playerView.IsMine)
            {
                PhotonView myView = GetComponent<PhotonView>();
                if (myView != null)
                {
                    myView.RPC("OpenDoorRPC", RpcTarget.AllBuffered);
                }
                else
                {
                    OpenDoorRPC();
                }
            }
        }
    }

    [PunRPC]
    public void OpenDoorRPC()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = doorOpenSprite;
        }
    }
}