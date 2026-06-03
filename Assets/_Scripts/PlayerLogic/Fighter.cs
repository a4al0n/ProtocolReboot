using UnityEngine;

public class Fighter : MonoBehaviour
{
    [Header("Health Settings")]
    public int hitPoint = 10;
    public int maxHitPoint = 10;
    public float pushRecoverySpeed = 0.2f;

    protected float ImmuneTime = 0.2f;
    protected float lastImmune;
    protected Vector3 pushDirection;

    protected virtual void ReceiveDamage(Damag dmg)
    {
        if (Time.time - lastImmune > ImmuneTime)
        {
            lastImmune = Time.time;
            hitPoint -= dmg.damageAmount;
            pushDirection = (transform.position - dmg.origin).normalized * dmg.pushForce;

            if (hitPoint <= 0)
            {
                hitPoint = 0;
                Death();
            }
        }
    }

    protected virtual void Death() { }
}