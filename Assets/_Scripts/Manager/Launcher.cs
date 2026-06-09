using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    [Header("Player Settings")]
    public string playerPrefabName = "Player";
    public string spawnPointTag = "SpawnPoint";

    private void Awake()
    {
        instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Launcher: Already in room, spawning player...");
            SpawnPlayer();
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Launcher: Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Launcher: Already connected, joining lobby...");
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Launcher: Connected to Master! Joining Lobby...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Launcher: Joined Lobby. Joining or creating Room1...");
        PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Launcher: Joined Room! Spawning player...");
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        // Проверяем — есть ли уже локальный игрок в сцене
        foreach (var player in FindObjectsOfType<Player>())
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                Debug.Log("Launcher: Local player already exists, moving to SpawnPoint...");
                MovePlayerToSpawn(player.gameObject);
                return;
            }
        }

        // Игрока нет — создаём
        Vector3 spawnPos = GetSpawnPosition();
        Debug.Log("Launcher: Instantiating player at " + spawnPos);
        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
    }

    private void MovePlayerToSpawn(GameObject player)
    {
        Vector3 spawnPos = GetSpawnPosition();
        player.transform.position = spawnPos;
    }

    private Vector3 GetSpawnPosition()
    {
        GameObject spawnPoint = GameObject.FindWithTag(spawnPointTag);
        if (spawnPoint != null)
            return spawnPoint.transform.position;

        Debug.LogWarning("Launcher: SpawnPoint not found, using Vector3.zero");
        return Vector3.zero;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Launcher: Disconnected — " + cause);
        if (cause == DisconnectCause.DisconnectByClientLogic) return;
        Debug.Log("Launcher: Reconnecting...");
        PhotonNetwork.ConnectUsingSettings();
    }
}