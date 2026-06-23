// SceneTranslate.cs
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneTranslate : MonoBehaviourPunCallbacks
{
    [Header("UI Settings")]
    public GameObject loadingPanel;
    public Slider progressSlider;

    [Header("Lore Settings")]
    public string[] loreTexts;
    public TextMeshProUGUI loreTextUI;

    [Header("Press Any Key Settings")]
    public GameObject pressAnyKeyText;
    public float blinkSpeed = 1.5f;

    [Header("Animation Settings")]
    public float sliderSpeed = 2f;

    private bool _isLoading = false;
    private Coroutine _blinkCoroutine;

    public static SceneTranslate Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(false);
    }

    private void Update()
    {
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

    public void ShowLoadingScreen()
    {
        ShowPanel(0f);
        ShowRandomLore();
    }

    private void ShowRandomLore()
    {
        if (loreTextUI == null || loreTexts == null || loreTexts.Length == 0) return;

        string randomText = loreTexts[Random.Range(0, loreTexts.Length)];
        loreTextUI.text = randomText;
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
        ShowRandomLore();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f);
            if (progressSlider != null)
                progressSlider.value = Mathf.MoveTowards(
                    progressSlider.value, target, sliderSpeed * Time.deltaTime);
            yield return null;
        }

        if (progressSlider != null)
            progressSlider.value = 1f;

        yield return StartCoroutine(WaitForPlayerInput());

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        _isLoading = false;
        HidePanel();
    }

    private IEnumerator PerformNetworkLoading(string sceneName)
    {
        _isLoading = true;
        ShowPanel(0f);
        ShowRandomLore();

        PhotonNetwork.LoadLevel(sceneName);

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

        if (progressSlider != null)
            progressSlider.value = 1f;

        yield return StartCoroutine(WaitForPlayerInput());

        _isLoading = false;
        HidePanel();
    }

    private IEnumerator WaitForPlayerInput()
    {
        Debug.Log("SceneTranslate: Waiting for player input now...");

        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.SetActive(true);
            _blinkCoroutine = StartCoroutine(BlinkPressAnyKey());
        }

        yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));

        Debug.Log("SceneTranslate: Input detected, continuing...");

        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(false);
    }

    private IEnumerator BlinkPressAnyKey()
    {
        if (pressAnyKeyText == null) yield break;

        CanvasGroup cg = pressAnyKeyText.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = pressAnyKeyText.AddComponent<CanvasGroup>();

        while (true)
        {
            float t = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) + 1f) / 2f;
            cg.alpha = t;
            yield return null;
        }
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