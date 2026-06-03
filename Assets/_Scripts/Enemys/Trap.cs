using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : EnemyHitBox
{
    public float lateToStart = 1f; 
    private Animator animator;     

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        StartCoroutine("StartTrap");
    }

    protected override void OnCollide(Collider2D coll)
    {
        base.OnCollide(coll);
    }

    IEnumerator StartTrap()
    {
        yield return new WaitForSeconds(lateToStart);
        animator.SetTrigger("start");
    }
}
