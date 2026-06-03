// PortalDebug.cs
using UnityEngine;

public class PortalDebug : MonoBehaviour
{
    private BoxCollider2D _col;

    private void Start()
    {
        _col = GetComponent<BoxCollider2D>();
        Debug.Log($"[PortalDebug] Start — collider: {_col}, size: {_col?.size}, offset: {_col?.offset}");
    }

    private void Update()
    {
        if (_col == null) return;

        // Рисуем зону портала в Scene View
        Vector2 center = (Vector2)transform.position + _col.offset;
        Debug.DrawLine(center + Vector2.left * _col.size.x / 2, center + Vector2.right * _col.size.x / 2, Color.red);
        Debug.DrawLine(center + Vector2.up * _col.size.y / 2, center + Vector2.down * _col.size.y / 2, Color.red);

        // Проверяем ВСЁ что находится в зоне портала
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, _col.size, 0f);
        if (hits.Length > 0)
        {
            foreach (var h in hits)
                Debug.Log($"[PortalDebug] In zone: {h.name} | tag: {h.tag} | layer: {LayerMask.LayerToName(h.gameObject.layer)}");
        }

        // Отдельно ищем игрока по тегу в радиусе 5 единиц
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 5f);
        foreach (var h in nearby)
        {
            if (h.CompareTag("Player"))
                Debug.Log($"[PortalDebug] Player nearby at distance: {Vector2.Distance(transform.position, h.transform.position):F2} | name: {h.name}");
        }
    }
}