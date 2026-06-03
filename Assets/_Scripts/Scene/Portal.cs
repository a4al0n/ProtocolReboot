using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IOnEventCallback
{
    public const byte SCENE_CHANGE_EVENT = 42;
    public string sceneName;

    private static bool _sceneChangeInProgress = false;
    private BoxCollider2D _col;

    private void Start()
    {
        _col = GetComponent<BoxCollider2D>();
        if (_col != null)
        {
            _col.isTrigger = true;
            _col.enabled = true;
        }
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void OnDisable()
    {
        _sceneChangeInProgress = false;
    }

    private void Update()
    {
        if (_sceneChangeInProgress) return;
        if (_col == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + _col.offset,
            _col.size, 0f
        );

        foreach (var hit in hits)
        {
            // Ищем Player — тег "fighter" или "Player", и проверяем что это не оружие
            // Оружие — дочерний объект, у него нет PhotonView
            // Ищем PhotonView на самом объекте И на родителе
            PhotonView pv = hit.GetComponent<PhotonView>();
            if (pv == null) pv = hit.GetComponentInParent<PhotonView>();

            // Пропускаем если нет PhotonView (это оружие или другой объект)
            if (pv == null && PhotonNetwork.IsConnected) continue;

            // Проверяем что это именно игрок а не враг
            Player playerComponent = hit.GetComponent<Player>();
            if (playerComponent == null)
                playerComponent = hit.GetComponentInParent<Player>();
            if (playerComponent == null) continue; // не игрок — пропускаем

            // Проверяем что это локальный игрок
            bool isLocalPlayer = (pv != null && pv.IsMine) || !PhotonNetwork.IsConnected;
            if (!isLocalPlayer) continue;

            Debug.Log("Portal: Player detected! Scene: " + sceneName);
            _sceneChangeInProgress = true;

            if (GameManager.instance != null)
                GameManager.instance.SaveState();

            if (SceneTranslate.Instance != null)
                SceneTranslate.Instance.ShowLoadingScreen();

            if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
            {
                ChangeSceneTo(sceneName);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                ChangeSceneTo(sceneName);
            }
            else
            {
                byte[] sceneBytes = System.Text.Encoding.UTF8.GetBytes(sceneName);
                RaiseEventOptions opts = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                };
                PhotonNetwork.RaiseEvent(
                    SCENE_CHANGE_EVENT, sceneBytes, opts, SendOptions.SendReliable);
                Debug.Log("Portal: Sent SCENE_CHANGE_EVENT → MasterClient");
            }

            break;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != SCENE_CHANGE_EVENT) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (_sceneChangeInProgress) return;

        byte[] sceneBytes = (byte[])photonEvent.CustomData;
        string scene = System.Text.Encoding.UTF8.GetString(sceneBytes);
        if (scene != sceneName) return;

        _sceneChangeInProgress = true;
        Debug.Log("Portal: MasterClient received SCENE_CHANGE_EVENT, scene: " + scene);
        ChangeSceneTo(scene);
    }

    private void ChangeSceneTo(string targetScene)
    {
        if (SceneTranslate.Instance != null)
        {
            SceneTranslate.Instance.ChangeToScene(targetScene);
        }
        else
        {
            Debug.LogWarning("Portal: SceneTranslate.Instance not found, falling back.");
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                PhotonNetwork.LoadLevel(targetScene);
            else
                SceneManager.LoadScene(targetScene);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}