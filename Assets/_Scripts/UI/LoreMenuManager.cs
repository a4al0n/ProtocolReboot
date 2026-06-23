using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoreMenuManager : MonoBehaviour
{
    [System.Serializable]
    public class LoreEntry
    {
        public string title;
        [TextArea(3, 10)]
        public string content;
    }

    [Header("Lore Database")]
    public LoreEntry[] loreDatabase;

    [Header("UI References")]
    public GameObject lorePanel;
    public Transform buttonContainer;
    public GameObject buttonPrefab;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    [Header("Button Highlight Settings")]
    public Color normalButtonColor = Color.white;
    public Color selectedButtonColor = new Color(0.8f, 0.8f, 1f);

    private List<GameObject> spawnedButtons = new List<GameObject>();
    private int currentSelectedIndex = -1;
    private bool isGenerated = false;

    private void Start()
    {
        if (lorePanel != null)
            lorePanel.SetActive(false);
    }

    public void OpenLoreMenu()
    {
        if (lorePanel != null)
            lorePanel.SetActive(true);

        if (!isGenerated)
        {
            GenerateButtons();
            isGenerated = true;
        }

        if (loreDatabase != null && loreDatabase.Length > 0)
        {
            SelectEntry(0);
        }
    }

    public void CloseLoreMenu()
    {
        if (lorePanel != null)
            lorePanel.SetActive(false);
    }

    private void GenerateButtons()
    {
        if (buttonContainer == null || buttonPrefab == null || loreDatabase == null)
        {
            Debug.LogWarning("LoreMenuManager: Missing references for button generation.");
            return;
        }

        foreach (var btn in spawnedButtons)
        {
            if (btn != null) Destroy(btn);
        }
        spawnedButtons.Clear();

        for (int i = 0; i < loreDatabase.Length; i++)
        {
            int index = i;

            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
            newButton.SetActive(true);

            TextMeshProUGUI buttonLabel = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonLabel != null)
                buttonLabel.text = loreDatabase[i].title;

            Button buttonComponent = newButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => SelectEntry(index));
            }

            spawnedButtons.Add(newButton);
        }
    }

    public void SelectEntry(int index)
    {
        if (loreDatabase == null || index < 0 || index >= loreDatabase.Length)
            return;

        currentSelectedIndex = index;

        if (titleText != null)
            titleText.text = loreDatabase[index].title;

        if (contentText != null)
            contentText.text = loreDatabase[index].content;

        UpdateButtonHighlight();
    }

    private void UpdateButtonHighlight()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] == null) continue;

            Image buttonImage = spawnedButtons[i].GetComponent<Image>();
            if (buttonImage == null) continue;

            buttonImage.color = (i == currentSelectedIndex) ? selectedButtonColor : normalButtonColor;
        }
    }
}