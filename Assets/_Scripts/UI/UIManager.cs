using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("------包含面板------")]
    public CharacterMenu characterMenu;             
    public CharacterHUD characterHUD;             
    public FloatingTextManager floatingTextManager; 

    [Header("------特殊状态机------")]
    public Animator deathMenuAnim;                

    
    private void Start()
    {
        deathMenuAnim.gameObject.SetActive(false);
        UIUpdate();
    }

    public void UIUpdate()
    {
        characterMenu.UpdateMenu();
        characterHUD.UpdateHUD();
    }

    public void ShowText(string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
    {
        floatingTextManager.Show(msg, fontSize, color, position, motion, duration);
    }

    public void HideDeathAnimation()
    {
        deathMenuAnim.SetTrigger("Hide");
        deathMenuAnim.gameObject.SetActive(false);
    }

    public void ShowDeathAnimation()
    {
        deathMenuAnim.gameObject.SetActive(true);
        deathMenuAnim.SetTrigger("Show");
    }

    public void QuitGame()
    {   
        Application.Quit();
    }
}
