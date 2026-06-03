using UnityEngine;
using Photon.Pun;

public class Portal_Door : Colliderable
{
    public BoxCollider2D boxOUT;

    protected override void OnCollide(Collider2D coll)
    {
        // Rule 4: CompareTag instead of name check
        if (coll.CompareTag("Player"))
        {
            PhotonView pv = coll.GetComponent<PhotonView>();

            // Rule 4: Physical movement only on the local player (IsMine)
            if (pv != null && pv.IsMine)
            {
                if (boxOUT == null) return;

                Vector3 vector = boxOUT.transform.position;
                vector.z = 0;

                // Offset based on exit portal rotation
                if (boxOUT.gameObject.transform.rotation == Quaternion.Euler(0, 0, 0))
                    vector.y += 0.2f;
                else if (boxOUT.gameObject.transform.rotation == Quaternion.Euler(0, 0, 180))
                    vector.y -= 0.2f;
                else if (boxOUT.gameObject.transform.rotation == Quaternion.Euler(0, 0, -90))
                    vector.x += 0.2f;
                else if (boxOUT.gameObject.transform.rotation == Quaternion.Euler(0, 0, 90))
                    vector.x -= 0.2f;

                // Move the local player. PhotonTransformView will sync this position to other clients.
                coll.transform.position = vector;

                Debug.Log("Portal_Door: Player teleported through Portal_Door.");
            }
        }
    }
}