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
        if (enemys == null || trans == null) return;
        if (_roadOpened) return;

        if (PhotonNetwork.IsMasterClient)
        {
            num = CountAliveEnemies();

            if (num == 0)
            {
                if (photonView != null)
                {
                    _roadOpened = true;
                    photonView.RPC("DisplayRoadRPC", RpcTarget.AllBuffered);
                }
                else
                {
                    _roadOpened = true;
                    DisplayRoadRPC();
                }
            }
        }
    }

    private int CountAliveEnemies()
    {
        int aliveCount = 0;

        foreach (Transform child in enemys.transform)
        {
            Enemy e = child.GetComponent<Enemy>();
            if (e == null) continue;

            if (!e.gameObject.activeInHierarchy) continue;

            SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
            if (sr != null && !sr.enabled) continue;

            aliveCount++;
        }

        return aliveCount;
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