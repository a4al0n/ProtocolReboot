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
        // Если уже в комнате (перешли через портал) — просто спавним игрока
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Launcher: Already in room, spawning player...");
            SpawnPlayer();
            return;
        }

        // Первый запуск — подключаемся
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Launcher: Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            // Подключены но не в комнате — заходим в лобби
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
        // Ищем SpawnPoint в сцене
        Vector3 spawnPos = Vector3.zero;
        GameObject spawnPoint = GameObject.FindWithTag(spawnPointTag);
        if (spawnPoint != null)
            spawnPos = spawnPoint.transform.position;
        else
            Debug.LogWarning("Launcher: SpawnPoint not found, spawning at Vector3.zero");

        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
    }
}