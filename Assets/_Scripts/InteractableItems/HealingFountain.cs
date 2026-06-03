using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingFountain : Colliderable
{
    public int healingAmount = 1;   
    public int healingTotal = 10;      
    private float healCoolDown = 0.5f;  
    private float lastHeal;

    protected override void OnCollide(Collider2D coll)
    {
        if (coll.name == "Player" && GameManager.instance.player.isAlive)
        {
            //Debug.Log("HealingFountain.coll = " + coll.name);
            if (Time.time - lastHeal > healCoolDown && healingTotal > 0)
            {
                Debug.Log(coll.name);
                
                lastHeal = Time.time;
                healingTotal--;

                GameManager.instance.player.Heal(healingAmount);
            }
        }
        else
            return;
    }
}
