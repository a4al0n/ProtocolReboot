// Portal_Door.cs — убираем _teleporting, он больше не нужен
using UnityEngine;
using Photon.Pun;

public class Portal_Door : Colliderable
{
    public BoxCollider2D boxOUT;

    protected override void OnCollide(Collider2D coll)
    {
        Player playerComponent = coll.GetComponent<Player>();
        if (playerComponent == null)
            playerComponent = coll.GetComponentInParent<Player>();
        if (playerComponent == null) return;

        PhotonView pv = playerComponent.GetComponent<PhotonView>();
        if (pv != null && !pv.IsMine) return;

        if (boxOUT == null)
        {
            Debug.LogWarning("[Portal_Door] boxOUT is not assigned!");
            return;
        }

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

        playerComponent.transform.position = vector;
        Debug.Log($"[Portal_Door] Player teleported to {vector}");
    }
}