using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMenu : MonoBehaviour
{
    [Header("------Text------")]
    public Text levelTextMenu;
    public Text hitpointText, pesosText, upgradeCostText, xpText;

    [Header("------Sprite/Image------")]  
    public Image characterSprite;                       
    public Image weaponSprite;                         
    private int currentCharacterSelection = 0;        

    [Header("------XP------")]
    public RectTransform xpBar;                        
    public GameObject savingTextObject;

    public void OnArrowClick(bool right)
    {
        if (right)
        {
            currentCharacterSelection++;
            if (currentCharacterSelection == GameManager.instance.playerSprites.Count)
                currentCharacterSelection = 0;

            OnSelectionChanged();
        }
        else
        {
            currentCharacterSelection--;
            if (currentCharacterSelection < 0)
                currentCharacterSelection = GameManager.instance.playerSprites.Count - 1;

            OnSelectionChanged();
        }
    }

    private void OnSelectionChanged()
    {
        characterSprite.sprite = GameManager.instance.playerSprites[currentCharacterSelection];
        GameManager.instance.player.SwapSprite(currentCharacterSelection);
    }

    public void OnUpgradeClick()
    {
        if (GameManager.instance.TryUpgradeWeapon())
            UpdateMenu();
    }

    public void UpdateMenu()
    {
        // Если игрока еще нет (он спавнится через сеть), просто выходим из метода
        if (GameManager.instance.player == null) return;

        weaponSprite.sprite = GameManager.instance.weaponSprites[GameManager.instance.weapon.weaponLevel];

        if (GameManager.instance.weapon.weaponLevel == GameManager.instance.weaponPrices.Count)
            upgradeCostText.text = "MAX";
        else
            upgradeCostText.text = GameManager.instance.weaponPrices[GameManager.instance.weapon.weaponLevel].ToString();

        levelTextMenu.text = GameManager.instance.GetCurrentLevel().ToString();
        hitpointText.text = GameManager.instance.player.hitPoint.ToString() + " /" + GameManager.instance.player.maxHitPoint;
        pesosText.text = GameManager.instance.pesos.ToString();

        int currentLevel = GameManager.instance.GetCurrentLevel();
        if (currentLevel == GameManager.instance.xpTable.Count)
        {
            xpText.text = GameManager.instance.experience.ToString() + " total exprience points";
            xpBar.localScale = Vector3.one;
        }
        else
        {
            int prevLevelXP = GameManager.instance.GetXPToLevel(currentLevel-1);
            int currLevelXP = GameManager.instance.GetXPToLevel(currentLevel);

            int diff = currLevelXP - prevLevelXP;
            int currXpIntoLevel = GameManager.instance.experience - prevLevelXP;

            float completionRatio = (float)currXpIntoLevel / (float)diff;
            xpBar.localScale = new Vector3(completionRatio, 1, 1);
            xpText.text = currXpIntoLevel.ToString() + " / " + diff;
        }
    }

    public void ShowSavingText()
    {
        if (savingTextObject.activeSelf != true)
        {
            savingTextObject.SetActive(true);
            Invoke("HideSavingText", 2);
        }
        else
            return;
    }

    private void HideSavingText()
    {
        savingTextObject.SetActive(false);
    }
}
