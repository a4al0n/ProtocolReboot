using UnityEngine;
using Photon.Pun;

public class EnemyHitBox : Colliderable
{
    [Header("------ Settings ------")]
    public int damage;
    public float pushForce;

    protected override void OnCollide(Collider2D coll)
    {
        // Ищем Player по компоненту вместо тега
        Player playerComponent = coll.GetComponent<Player>();
        if (playerComponent == null)
            playerComponent = coll.GetComponentInParent<Player>();
        if (playerComponent == null) return;

        PhotonView pv = coll.GetComponent<PhotonView>();
        if (pv == null) pv = coll.GetComponentInParent<PhotonView>();

        // Только локальный игрок получает урон
        bool isLocalPlayer = (pv != null && pv.IsMine) || !PhotonNetwork.IsConnected;
        if (!isLocalPlayer) return;

        Damag dmg = new Damag
        {
            damageAmount = damage,
            origin = transform.position,
            pushForce = pushForce
        };

        playerComponent.SendMessage("ReceiveDamage", dmg);
    }
}