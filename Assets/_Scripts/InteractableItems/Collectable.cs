using UnityEngine;
using Photon.Pun;

public class Collectable : Colliderable
{
    protected bool collected = false;

    protected override void OnCollide(Collider2D coll)
    {
        if (collected) return;

        // Ищем Player на объекте или родителе вместо проверки тега
        Player playerComponent = coll.GetComponent<Player>();
        if (playerComponent == null)
            playerComponent = coll.GetComponentInParent<Player>();
        if (playerComponent == null) return;

        PhotonView pv = coll.GetComponent<PhotonView>();
        if (pv == null) pv = coll.GetComponentInParent<PhotonView>();

        // Только локальный игрок инициирует сбор
        bool isLocalPlayer = (pv != null && pv.IsMine) || !PhotonNetwork.IsConnected;
        if (!isLocalPlayer) return;

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

    [PunRPC]
    protected virtual void CollectRPC(int actorNumber)
    {
        if (!collected)
            OnCollect(actorNumber);
    }

    protected virtual void OnCollect(int actorNumber)
    {
        collected = true;
    }
}