// Colliderable.cs
using UnityEngine;

public class Colliderable : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Collider2D[] hits = new Collider2D[10];
    private Collider2D[] previousHits = new Collider2D[10];

    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Update()
    {
        if (boxCollider == null) return;

        int count = Physics2D.OverlapBox(
            (Vector2)transform.position + boxCollider.offset,
            boxCollider.size,
            0f,
            new ContactFilter2D().NoFilter(),
            hits
        );

        for (int i = 0; i < count; i++)
        {
            if (hits[i] == null) continue;
            // Пропускаем себя и всех родителей/детей
            if (hits[i].gameObject == gameObject) continue;
            if (hits[i].transform.IsChildOf(transform)) continue;
            if (transform.IsChildOf(hits[i].transform)) continue;

            if (!WasInPreviousHits(hits[i]))
                OnCollide(hits[i]);
        }

        System.Array.Copy(hits, previousHits, hits.Length);
        System.Array.Clear(hits, 0, hits.Length);
    }

    private bool WasInPreviousHits(Collider2D coll)
    {
        for (int i = 0; i < previousHits.Length; i++)
            if (previousHits[i] == coll) return true;
        return false;
    }

    protected virtual void OnCollide(Collider2D coll)
    {
        Debug.Log("Collide: " + coll.name);
    }
}