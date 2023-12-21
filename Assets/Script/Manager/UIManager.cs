using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button StartButton;
    public Button QuitButton;
    public void ButtonStart_Click()
    {
        GameManager.RenderMenu(false);
        GameManager.RenderUI(true);
        GameManager.RenderBossesHealthBar(true, 0);
        GameManager.RenderBossesHealthBar(true, 1);
        GameManager.Instance.GameState = GameState.GamePlay;

        Debug.Log("Game start");
    }

    public void ButtonQuitGame_Click()
    {
        Application.Quit();
    }

    void Start()
    {

    }

    void Update()
    {
        
    }
}
