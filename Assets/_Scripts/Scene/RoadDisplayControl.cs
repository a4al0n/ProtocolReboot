using UnityEngine;
using Photon.Pun;

public class RoadDisplayControl : MonoBehaviourPun
{
    public GameObject enemys;
    public GameObject trans;
    private int num;
    private bool _roadOpened = false;

    private void Start()
    {
        if (trans != null)
            trans.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (enemys == null || !enemys || trans == null || !trans) return;
        if (_roadOpened) return;

        if (PhotonNetwork.IsMasterClient)
        {
            num = enemys.transform.childCount;

            if (num == 0)
            {
                // Проверяем есть ли PhotonView перед вызовом RPC
                if (photonView != null)
                {
                    _roadOpened = true;
                    photonView.RPC("DisplayRoadRPC", RpcTarget.AllBuffered);
                }
                else
                {
                    // Fallback без сети
                    _roadOpened = true;
                    DisplayRoadRPC();
                }
            }
        }
    }

    [PunRPC]
    public void DisplayRoadRPC()
    {
        if (trans != null && trans)
        {
            trans.gameObject.SetActive(true);
            Debug.Log("RoadDisplayControl: All enemies defeated. Road opened!");
        }
    }
}