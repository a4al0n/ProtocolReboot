using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Chest : Collectable
{
    public Sprite emptyChest;       
    public int pesosAmount = 5;    

    protected override void OnCollect(int actorNumber)
    {
        collected = true;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = emptyChest;
        }

        // Only add pesos and show text on the specific client of the player who collected the chest
        bool isCollector = (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.ActorNumber == actorNumber) || 
                           (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom);

        if (isCollector && GameManager.instance != null)
        {
            GameManager.instance.pesos += pesosAmount;
            GameManager.instance.ShowText("+" + pesosAmount + " pesos", 25, Color.yellow, transform.position, Vector3.up * 20, 1.5f);
            GameManager.instance.OnUIChange();
        }
    }
}