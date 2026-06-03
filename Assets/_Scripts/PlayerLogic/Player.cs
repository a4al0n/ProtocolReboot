using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player : Mover
{
    public bool isAlive = true;

    [Header("------ (Rage System) ------")]
    public float rage = 0;
    public float maxRage = 50;

    private float lastXScale = 1f;

    // Ссылка на сетевой вид
    private PhotonView view;

    protected override void Start()
    {
        base.Start();
        view = GetComponent<PhotonView>();

        if (view == null)
        {
            Debug.LogError("Player: PhotonView component is missing on the Player prefab!");
            return;
        }

        // Выполняем только на локальном персонаже
        if (view.IsMine)
        {
            // 1. Регистрируем себя в GameManager
            if (GameManager.instance != null)
            {
                GameManager.instance.player = this;

                // 2. И только на локальном вызываем загрузку данных
                if (GameManager.instance.SaveManager != null)
                {
                    GameManager.instance.SaveManager.LoadGame();
                    Debug.Log("Player: Local player data loaded successfully!");
                }

                // Получаем оружие на локальном игроке
                GameManager.instance.weapon = GetComponentInChildren<Weapon>();
                if (GameManager.instance.weapon != null)
                {
                    // If we have loaded data, ensure weapon level is synced to other clients immediately
                    GameManager.instance.weapon.SetWeaponLevel(GameManager.instance.weapon.weaponLevel);
                }
            }

            // 3. Привязываем Cinemachine камеру
            var vcam = Object.FindFirstObjectByType<Cinemachine.CinemachineVirtualCamera>();
            if (vcam != null) vcam.Follow = transform;
        }
    }

    private void FixedUpdate()
    {
        // В мультиплеере управляем только своим персонажем
        if (PhotonNetwork.IsConnected && view != null && !view.IsMine)
        {
            return;
        }

        if (isAlive)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            // Поворот спрайта оружия (только для локального игрока)
            if (GameManager.instance != null && GameManager.instance.weapon != null && GameManager.instance.weapon.animator != null)
            {
                bool sameDir = Mathf.Approximately(transform.localScale.x, lastXScale);
                GameManager.instance.weapon.animator.SetBool("SameDirection", sameDir);
            }
            lastXScale = transform.localScale.x;

            UpdateMotor(new Vector3(x, y, 0), 1);
        }
        else
        {
            pushDirection = Vector3.zero;
        }
    }

    // --- Смена скинов ---
    public void SwapSprite(int SkinID)
    {
        if (GameManager.instance != null && GameManager.instance.playerOverrides != null && SkinID < GameManager.instance.playerOverrides.Count)
        {
            if (anim != null)
            {
                // Меняем скин локально
                anim.runtimeAnimatorController = GameManager.instance.playerOverrides[SkinID];

                // Синхронизируем скин для других игроков
                if (view != null && view.IsMine)
                {
                    view.RPC("RPC_SyncSkin", RpcTarget.OthersBuffered, SkinID);
                }
            }
        }
    }

    [PunRPC]
    private void RPC_SyncSkin(int id)
    {
        if (anim != null && GameManager.instance != null && GameManager.instance.playerOverrides != null && id < GameManager.instance.playerOverrides.Count)
        {
            anim.runtimeAnimatorController = GameManager.instance.playerOverrides[id];
        }
    }

    [PunRPC]
    private void RPC_SyncWeaponLevel(int level)
    {
        Weapon w = GetComponentInChildren<Weapon>();
        if (w != null)
        {
            w.SetWeaponLevel(level);
        }
    }

    // --- Система уровней ---
    public void OnLevelUp()
    {
        maxHitPoint += 10;
        hitPoint = maxHitPoint;
        if (GameManager.instance != null)
            GameManager.instance.OnUIChange();
    }

    public void SetLevel(int level)
    {
        for (int i = 0; i < level; i++)
            OnLevelUp();
    }

    // --- Получение урона ---
    protected override void ReceiveDamage(Damag dmg)
    {
        if (!isAlive) return;

        // В мультиплеере урон игроку обрабатывается только на его владельце (IsMine)
        if (PhotonNetwork.IsConnected && view != null && !view.IsMine) return;

        if (Time.time - lastImmune > ImmuneTime)
        {
            lastImmune = Time.time;
            hitPoint -= dmg.damageAmount;
            pushDirection = (transform.position - dmg.origin).normalized * dmg.pushForce;

            if (GameManager.instance != null && GameManager.instance.weapon != null && !GameManager.instance.weapon.raging)
                OnRageChange(dmg.damageAmount);
        }

        if (hitPoint <= 0)
        {
            hitPoint = 0;
            Death();
        }

        if (GameManager.instance != null)
            GameManager.instance.OnUIChange();
    }

    public void OnRageChange(float alter)
    {
        rage = Mathf.Clamp(rage + alter, 0, maxRage);

        if (rage == maxRage && GameManager.instance != null && GameManager.instance.weapon != null)
            GameManager.instance.weapon.CanRageSkill = true;
    }

    public void Heal(int healingAmount)
    {
        if (hitPoint >= maxHitPoint) return;

        hitPoint = Mathf.Min(hitPoint + healingAmount, maxHitPoint);
        if (GameManager.instance != null)
        {
            GameManager.instance.ShowText("+" + healingAmount + "hp", 25, Color.green, transform.position, Vector3.up * 30, 1.0f);
            GameManager.instance.OnUIChange();
        }
    }

    protected override void Death()
    {
        isAlive = false;

        // Поворачиваем персонажа при смерти
        transform.localEulerAngles = new Vector3(0, 0, 90);

        if (view != null && view.IsMine)
        {
            if (GameManager.instance != null && GameManager.instance.UIManager != null)
                GameManager.instance.UIManager.ShowDeathAnimation();
            StartCoroutine(WaitingForRespawn());
        }
    }

    public void Respawn()
    {
        hitPoint = maxHitPoint;
        isAlive = true;
        transform.localEulerAngles = Vector3.zero;
    }

    private IEnumerator WaitingForRespawn()
    {
        yield return new WaitForSeconds(6);
        if (GameManager.instance != null)
        {
            GameManager.instance.Respawn();
            GameManager.instance.OnUIChange();
        }
    }
}