// Colliderable.cs — добавляем защиту от повторного вызова
using UnityEngine;

public class Colliderable : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Collider2D[] hits = new Collider2D[10];
    // Вызываем OnCollide только при ВХОДЕ, не каждый кадр
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
            if (hits[i].gameObject == gameObject) continue;

            // Вызываем только если этого объекта не было в прошлом кадре
            if (!WasInPreviousHits(hits[i]))
                OnCollide(hits[i]);
        }

        // Сохраняем текущие хиты как предыдущие
        System.Array.Copy(hits, previousHits, hits.Length);

        // Очищаем hits для следующего кадра
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