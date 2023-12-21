using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    MainMenu,
    GamePlay,
    GameOver,
    GamePause
}
public class GameManager : MonoBehaviour
{

    public static GameManager Instance = null;
    public GameState GameState { get; set; } = GameState.MainMenu;

    private GameObject[] go;
    private GameObject menu;
    private GameObject boss;

    public static void RenderUI(bool render)
    {
        if(render)
        {
            foreach (GameObject _go in Instance.go)
            {
                if(!_go.activeSelf) _go.SetActive(true);
            }
            MouseRotation.LockCursor();
        }
        else
        {
            foreach (GameObject _go in Instance.go)
            {
                if (_go.activeSelf) _go.SetActive(false);
            }        
            MouseRotation.UnlockCursor();
        }
    }

    public static void RenderBossesHealthBar(bool render)
    {
        if (render)
        {
            if (!Instance.boss.activeSelf) Instance.boss.SetActive(true);
            MouseRotation.LockCursor();
        }
        else
        {
            if (Instance.boss.activeSelf) Instance.boss.SetActive(false);
            MouseRotation.UnlockCursor();
        }
    }

    public static void RenderMenu(bool render)
    {
        if (render)
        {
            if(!Instance.menu.activeSelf) Instance.menu.SetActive(true);
            MouseRotation.UnlockCursor();
        }
        else
        {
            if (Instance.menu.activeSelf) Instance.menu.SetActive(false);
            MouseRotation.LockCursor();
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        go = GameObject.FindGameObjectsWithTag("UIContainer");
        menu = GameObject.FindGameObjectWithTag("Menu");
        boss = GameObject.FindGameObjectsWithTag("BossHealthBar")[0];
    }
    void Start()
    {
       RenderMenu(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(Instance.GameState == GameState.GamePause)
            {
                RenderMenu(false);
                //RenderBossesHealthBar(true);
                RenderUI(true);
                Instance.GameState = GameState.GamePlay;
            }
            else if(Instance.GameState == GameState.GamePlay)
            {
                //RenderBossesHealthBar(false);
                RenderUI(false);
                RenderMenu(true);
                Instance.GameState = GameState.GamePause;
            }
        }
    }
}
