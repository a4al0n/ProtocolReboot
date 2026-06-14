using UnityEngine;
using Photon.Pun;

public class RoadDisplayControl : MonoBehaviourPun
{
    public GameObject enemys;
    public GameObject trans;
    private int num;

    private void Start()
    {
        if (trans != null)
            trans.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Проверяем через оператор Unity — он отловит уничтоженные объекты
        if (enemys == null || !enemys || trans == null || !trans) return;

        if (PhotonNetwork.IsMasterClient)
        {
            num = enemys.transform.childCount;

            if (num == 0 && !trans.activeSelf)
            {
                photonView.RPC("DisplayRoadRPC", RpcTarget.AllBuffered);
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