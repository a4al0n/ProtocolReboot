using UnityEngine;
using Photon.Pun;

public class Chest : Collectable
{
    public Sprite emptyChest;
    public int pesosAmount = 5;

    // Локальный флаг — каждый игрок имеет свой
    private bool _collectedLocally = false;

    protected override void OnCollide(Collider2D coll)
    {
        if (_collectedLocally) return;

        // Ищем локального игрока
        Player playerComponent = coll.GetComponent<Player>();
        if (playerComponent == null)
            playerComponent = coll.GetComponentInParent<Player>();
        if (playerComponent == null) return;

        PhotonView pv = coll.GetComponent<PhotonView>();
        if (pv == null) pv = coll.GetComponentInParent<PhotonView>();

        bool isLocalPlayer = (pv != null && pv.IsMine) || !PhotonNetwork.IsConnected;
        if (!isLocalPlayer) return;

        _collectedLocally = true;
        OnCollect(0);
    }

    protected override void OnCollect(int actorNumber)
    {
        // Меняем спрайт только локально
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && emptyChest != null)
            sr.sprite = emptyChest;

        if (GameManager.instance != null)
        {
            GameManager.instance.pesos += pesosAmount;
            GameManager.instance.ShowText("+" + pesosAmount + " pesos", 25, Color.yellow,
                transform.position, Vector3.up * 20, 1.5f);
            GameManager.instance.OnUIChange();
        }
    }
}