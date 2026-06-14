using UnityEngine;

public class HealingFountain : Colliderable
{
    public int healingAmount = 1;
    public int healingTotal = 10;
    private float healCoolDown = 0.5f;
    private float lastHeal;

    protected override void OnCollide(Collider2D coll)
    {
        // Ищем Player по компоненту вместо имени
        Player playerComponent = coll.GetComponent<Player>();
        if (playerComponent == null)
            playerComponent = coll.GetComponentInParent<Player>();
        if (playerComponent == null) return;

        if (!playerComponent.isAlive) return;

        if (Time.time - lastHeal > healCoolDown && healingTotal > 0)
        {
            lastHeal = Time.time;
            healingTotal--;
            playerComponent.Heal(healingAmount);
        }
    }
}