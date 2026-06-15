using UnityEngine;
using Photon.Pun;

public class EnemyHitBox : Colliderable
{
    [Header("------ Settings ------")]
    public int damage;
    public float pushForce;

    // Кулдаун между ударами — 1 секунда
    private float _damageCooldown = 1f;
    private float _lastDamageTime = -999f;

    protected override void OnCollide(Collider2D coll)
    {
        // Проверяем кулдаун
        if (Time.time - _lastDamageTime < _damageCooldown) return;

        Player playerComponent = coll.GetComponent<Player>();
        if (playerComponent == null)
            playerComponent = coll.GetComponentInParent<Player>();
        if (playerComponent == null) return;

        PhotonView pv = coll.GetComponent<PhotonView>();
        if (pv == null) pv = coll.GetComponentInParent<PhotonView>();

        bool isLocalPlayer = (pv != null && pv.IsMine) || !PhotonNetwork.IsConnected;
        if (!isLocalPlayer) return;

        _lastDamageTime = Time.time;

        Damag dmg = new Damag
        {
            damageAmount = damage,
            origin = transform.position,
            pushForce = pushForce
        };

        playerComponent.SendMessage("ReceiveDamage", dmg);
    }
}