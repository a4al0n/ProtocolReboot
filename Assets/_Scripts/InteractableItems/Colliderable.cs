using UnityEngine;

public class Colliderable : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Collider2D[] hits = new Collider2D[10];

    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Update()
    {
        if (boxCollider == null)
        {
            Debug.LogError($"[Colliderable] {gameObject.name}: boxCollider is NULL!");
            return;
        }

        int count = Physics2D.OverlapBox(
            (Vector2)transform.position + boxCollider.offset,
            boxCollider.size,
            0f,
            new ContactFilter2D().NoFilter(),
            hits
        );

        Debug.Log($"[Colliderable] {gameObject.name}: found {count} objects");
        for (int i = 0; i < count; i++)
        {
            if (hits[i] == null) continue;
            Debug.Log($"[Colliderable] hit: {hits[i].name} tag:{hits[i].tag}");
            if (hits[i].gameObject == gameObject) continue;
            OnCollide(hits[i]);
            hits[i] = null;
        }
    }

    protected virtual void OnCollide(Collider2D coll)
    {
        Debug.Log("Collide: " + coll.name);
    }
}