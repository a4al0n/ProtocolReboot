// SceneTranslate.cs
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTranslate : MonoBehaviourPunCallbacks
{
    [Header("UI Settings")]
    public GameObject loadingPanel;
    public Slider progressSlider;

    [Header("Animation Settings")]
    public float sliderSpeed = 2f;

    private bool _isLoading = false;

    // Синглтон — переживает смену сцен
    public static SceneTranslate Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // <-- переживает смену сцены
    }

    private void Start()
    {
        if (loadingPanel == null)
        {
            var ui = GameObject.Find("SCUI");
            if (ui != null) loadingPanel = ui;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (progressSlider == null && loadingPanel != null)
            progressSlider = loadingPanel.GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        // Не-мастер клиент: отслеживаем прогресс сетевой загрузки
        if (!_isLoading
            && PhotonNetwork.IsConnected
            && PhotonNetwork.InRoom
            && PhotonNetwork.LevelLoadingProgress > 0f
            && PhotonNetwork.LevelLoadingProgress < 1f)
        {
            ShowPanel(progressSlider != null ? progressSlider.value : 0f);

            if (progressSlider != null)
                progressSlider.value = Mathf.MoveTowards(
                    progressSlider.value,
                    PhotonNetwork.LevelLoadingProgress,
                    sliderSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Показать панель загрузки немедленно (вызывается из Portal до смены сцены).
    /// </summary>
    public void ShowLoadingScreen()
    {
        ShowPanel(0f);
    }

    public void ChangeToScene(string sceneName)
    {
        if (_isLoading) return;

        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            StartCoroutine(PerformLocalLoading(sceneName));
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(PerformNetworkLoading(sceneName));
        }
    }

    private IEnumerator PerformLocalLoading(string sceneName)
    {
        _isLoading = true;
        ShowPanel(0f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f);
            if (progressSlider != null)
                progressSlider.value = Mathf.MoveTowards(
                    progressSlider.value, target, sliderSpeed * Time.deltaTime);
            yield return null;
        }

        _isLoading = false;
        HidePanel();
    }

    private IEnumerator PerformNetworkLoading(string sceneName)
    {
        _isLoading = true;
        ShowPanel(0f);

        PhotonNetwork.LoadLevel(sceneName);

        // Ждём старта загрузки с таймаутом
        float timeout = 5f;
        while (PhotonNetwork.LevelLoadingProgress <= 0f && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (timeout <= 0f)
            Debug.LogWarning("SceneTranslate: Timeout waiting for Photon level load!");

        while (PhotonNetwork.LevelLoadingProgress < 1f)
        {
            if (progressSlider != null)
                progressSlider.value = Mathf.MoveTowards(
                    progressSlider.value,
                    PhotonNetwork.LevelLoadingProgress,
                    sliderSpeed * Time.deltaTime);
            yield return null;
        }

        _isLoading = false;
        // Панель скроется сама — объект DontDestroyOnLoad, но новая сцена может её переписать
        HidePanel();
    }

    private void ShowPanel(float initialProgress)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (progressSlider != null) progressSlider.value = initialProgress;
    }

    private void HidePanel()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }
}