using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHUD : MonoBehaviour
{
    public RectTransform healthBar;     
    public RectTransform xpBar;         
    public RectTransform rageBar;       
    public Image rImage;                

    public Text level;                  

    public void UpdateHUD()
    {
        if (GameManager.instance.player == null) return;
        level.text = GameManager.instance.GetCurrentLevel().ToString();

        float ratio = (float)GameManager.instance.player.hitPoint / (float)GameManager.instance.player.maxHitPoint;
        healthBar.localScale = new Vector3(ratio, 1, 1);

        int currentLevel = GameManager.instance.GetCurrentLevel();
        if (currentLevel == GameManager.instance.xpTable.Count)
        {
            xpBar.localScale = Vector3.one;
        }
        else
        {
            int prevLevelXP = GameManager.instance.GetXPToLevel(currentLevel - 1);
            int currLevelXP = GameManager.instance.GetXPToLevel(currentLevel);

            int diff = currLevelXP - prevLevelXP;
            int currXpIntoLevel = GameManager.instance.experience - prevLevelXP;

            float completionRatio = (float)currXpIntoLevel / (float)diff;
            xpBar.localScale = new Vector3(completionRatio, 1, 1);
        }

        rageBar.localScale = new Vector3(GameManager.instance.player.rage / GameManager.instance.player.maxRage, 1, 1);
        if (GameManager.instance.player.rage == GameManager.instance.player.maxRage)
            rImage.gameObject.SetActive(true);
        else
            rImage.gameObject.SetActive(false);
    }
}
