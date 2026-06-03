using UnityEngine;
using Photon.Pun;

public class RoadDisplayControl : MonoBehaviourPun
{
    public GameObject enemys;
    public GameObject trans;
    private int num;

    private void Start()
    {
        // Initially, the path is closed
        if (trans != null)
            trans.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Rule 2: Defensive checks
        if (enemys == null || trans == null) return;

        // Rule 4: Host (MasterClient) validates victory condition
        if (PhotonNetwork.IsMasterClient)
        {
            num = enemys.transform.childCount;

            // If no enemies remain, open the path on all clients
            if (num == 0 && !trans.activeSelf)
            {
                photonView.RPC("DisplayRoadRPC", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    public void DisplayRoadRPC()
    {
        if (trans != null)
        {
            trans.gameObject.SetActive(true);
            Debug.Log("RoadDisplayControl: All enemies defeated. Road opened!");
        }
    }
}