using UnityEngine;
using Photon.Pun;

public class Portal_Door : Colliderable
{
    public BoxCollider2D boxOUT;

    private bool _teleporting = false;

    protected override void OnCollide(Collider2D coll)
    {
        if (_teleporting) return;

        Debug.Log($"[Portal_Door] {gameObject.name} hit: {coll.name} tag:{coll.tag}");

        // Ищем Player на объекте или родителе
        Player playerComponent = coll.GetComponent<Player>();
        if (playerComponent == null)
            playerComponent = coll.GetComponentInParent<Player>();

        if (playerComponent == null)
        {
            Debug.Log($"[Portal_Door] No Player on {coll.name}, skipping");
            return;
        }

        // Проверяем что это локальный игрок
        PhotonView pv = playerComponent.GetComponent<PhotonView>();
        if (pv != null && !pv.IsMine)
        {
            Debug.Log("[Portal_Door] Not local player, skipping");
            return;
        }

        if (boxOUT == null)
        {
            Debug.LogWarning("[Portal_Door] boxOUT is not assigned!");
            return;
        }

        _teleporting = true;

        Vector3 vector = boxOUT.transform.position;
        vector.z = 0;

        Quaternion rot = boxOUT.gameObject.transform.rotation;
        if (rot == Quaternion.Euler(0, 0, 0))
            vector.y += 0.2f;
        else if (rot == Quaternion.Euler(0, 0, 180))
            vector.y -= 0.2f;
        else if (rot == Quaternion.Euler(0, 0, -90))
            vector.x += 0.2f;
        else if (rot == Quaternion.Euler(0, 0, 90))
            vector.x -= 0.2f;

        // Телепортируем через transform игрока (не коллайдера)
        playerComponent.transform.position = vector;
        Debug.Log($"[Portal_Door] Player teleported to {vector}");

        // Сбрасываем флаг через секунду
        Invoke(nameof(ResetTeleport), 1f);
    }

    private void ResetTeleport()
    {
        _teleporting = false;
    }
}