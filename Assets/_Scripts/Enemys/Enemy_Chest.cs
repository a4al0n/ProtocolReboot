using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Chest : Enemy
{
    public bool _________________;
    public Sprite[] sprites;

    protected override void Update()
    {
        base.Update();
        if (chasing)
            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[1];
        else
            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
    }

    protected override void Death()
    {
        Destroy(gameObject);

        GameManager.instance.GrantXP(xpValue);
        GameManager.instance.ShowText("+" + xpValue + " xp", 30, Color.magenta, transform.position, Vector3.up * 40, 1.0f);
    }
}
