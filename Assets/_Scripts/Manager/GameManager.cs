using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("------ (Параметры) ------")]
    public int pesos;
    public int experience;
    public List<int> weaponPrices;
    public List<int> xpTable;

    [Header("------ (Ресурсы) ------")]
    public List<AnimatorOverrideController> playerOverrides;
    public List<Sprite> playerSprites;
    public List<Sprite> weaponSprites;

    [Header("------ (Ссылки) ------")]
    public Player player;
    public Weapon weapon;
    public UIManager UIManager;
    public SaveManager SaveManager;

    private void Awake()
    {
        // Hardcode AutomaticallySyncScene according to Rule 1
        PhotonNetwork.AutomaticallySyncScene = true;

        // Если экземпляр уже существует — уничтожаем дубликаты
        if (GameManager.instance != null)
        {
            if (player != null) Destroy(player.gameObject);
            if (UIManager != null) Destroy(UIManager.gameObject);
            if (SaveManager != null) Destroy(SaveManager.gameObject);
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Подписка на событие загрузки сцены
        SceneManager.sceneLoaded += LoadState;

        // Делаем объекты постоянными между сценами
        DontDestroyOnLoad(gameObject);
        // Примечание: в мультиплеере Player создаётся через PhotonNetwork.Instantiate,
        // поэтому он уже помечен как DontDestroyOnLoad автоматически Photon'ом.
        // Мы делаем DontDestroyOnLoad только для не-сетевых объектов.
        if (UIManager != null) DontDestroyOnLoad(UIManager.gameObject);
        if (SaveManager != null) DontDestroyOnLoad(SaveManager.gameObject);
    }

    // --- Интерфейс и Текст ---
    public void OnUIChange()
    {
        if (UIManager != null)
            UIManager.UIUpdate();
    }

    public void ShowText(string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
    {
        if (UIManager != null)
            UIManager.ShowText(msg, fontSize, color, position, motion, duration);
    }

    // --- Оружие ---
    public bool TryUpgradeWeapon()
    {
        if (weapon == null || weaponPrices.Count <= weapon.weaponLevel)
            return false;

        if (pesos >= weaponPrices[weapon.weaponLevel])
        {
            pesos -= weaponPrices[weapon.weaponLevel];
            weapon.UpgradeWeapon();
            OnUIChange();
            return true;
        }
        return false;
    }

    // --- Система опыта и уровней ---
    public int GetCurrentLevel()
    {
        int level = 0;
        int currentXP = 0;

        while (experience >= currentXP)
        {
            if (level >= xpTable.Count) break;
            currentXP += xpTable[level];
            level++;
        }
        return level;
    }

    public int GetXPToLevel(int level)
    {
        int xp = 0;
        for (int i = 0; i < level && i < xpTable.Count; i++)
        {
            xp += xpTable[i];
        }
        return xp;
    }

    public void GrantXP(int xp)
    {
        int currentLevel = GetCurrentLevel();
        experience += xp;

        if (currentLevel < GetCurrentLevel())
            OnLevelUp();

        OnUIChange();
    }

    public void OnLevelUp()
    {
        if (player != null)
        {
            ShowText("LEVEL UP!", 30, Color.yellow, player.transform.position, Vector3.up * 30, 2.0f);
            player.OnLevelUp();
        }
        OnUIChange();
    }

    // --- Игровые состояния ---
    public void Respawn()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            // In multiplayer, the master client loads the level, or we just rely on sync scene.
            // If local player dies, we only respawn them locally at the spawn point instead of reloading the scene for everyone!
            // Wait, reloading the scene for everyone in multiplayer is bad when one player dies.
            // Let's just reposition the player at the SpawnPoint and restore their health.
            GameObject spawnPoint = GameObject.Find("SpawnPoint");
            if (spawnPoint != null && player != null)
            {
                player.transform.position = spawnPoint.transform.position;
            }
        }

        if (UIManager != null) UIManager.HideDeathAnimation();
        if (player != null) player.Respawn();
    }

    public void SaveState()
    {
        if (SaveManager != null) SaveManager.SaveGame();
    }

    public void LoadState(Scene s, LoadSceneMode sceneMode)
    {
        // Запускаем корутину: ждём один кадр, чтобы сцена полностью инициализировалась
        StartCoroutine(LoadStateDelayed());
    }

    private IEnumerator LoadStateDelayed()
    {
        // Ждём один кадр для инициализации объектов сцены
        yield return null;

        // Переназначаем ссылку на игрока если прежняя устарела
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            // Ищем локального игрока в сцене
            foreach (var p in Object.FindObjectsByType<Player>(FindObjectsSortMode.None))
            {
                PhotonView pv = p.GetComponent<PhotonView>();
                if (pv == null || pv.IsMine)
                {
                    player = p;
                    weapon = p.GetComponentInChildren<Weapon>();
                    Debug.Log("GameManager: Re-registered local player after scene load.");
                    break;
                }
            }
        }

        if (player != null)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            bool isMine = (pv == null || pv.IsMine);

            if (isMine)
            {
                if (GetCurrentLevel() > 1)
                    player.SetLevel(GetCurrentLevel());

                GameObject spawnPoint = GameObject.Find("SpawnPoint");
                if (spawnPoint != null)
                {
                    player.transform.position = spawnPoint.transform.position;
                    Debug.Log("GameManager: Moved player to SpawnPoint in scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                }
            }
        }

        if (UIManager != null)
            UIManager.UIUpdate();
    }
}