using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform lookAt;
    public float boundX = 0.3f;
    public float boundY = 0.15f;

    private Vector3 delts = Vector3.zero;
    private Vector3 destination;

    private void FixedUpdate()
    {
        // 1. Проверяем наличие менеджера и игрока
        if (GameManager.instance == null || GameManager.instance.player == null)
        {
            return;
        }

        // 2. Если lookAt еще не назначен, берем его из GameManager
        if (lookAt == null)
        {
            lookAt = GameManager.instance.player.transform;
        }

        delts = Vector3.zero;

        // Теперь lookAt.position точно не выдаст ошибку
        float deltaX = lookAt.position.x - transform.position.x;
        if (deltaX > boundX || deltaX < -boundX)
        {
            if (transform.position.x < lookAt.position.x)
                delts.x = deltaX - boundX;
            else
                delts.x = deltaX + boundX;
        }

        float deltaY = lookAt.position.y - transform.position.y;
        if (deltaY > boundY || deltaY < -boundY)
        {
            if (transform.position.y < lookAt.position.y)
                delts.y = deltaY - boundY;
            else
                delts.y = deltaY + boundY;
        }

        delts.z = 0f;
        destination = Vector3.Lerp(transform.position, transform.position + delts, 0.2f);
        destination.z = -1f;

        transform.position = destination;
    }
}