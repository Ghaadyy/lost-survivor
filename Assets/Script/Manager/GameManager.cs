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
    private GameObject[] boss; //Get health bar of bosses
    private BossBehaviour[] bossBehaviours; //Get bosses behaviour to check if boss is dead
    private GameObject gameOverCanvas;

    public static void RenderUI(bool render)
    {
        if (render)
        {
            foreach (GameObject _go in Instance.go)
            {
                if (!_go.activeSelf) _go.SetActive(true);
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

    public static void RenderBossesHealthBar(bool render, int idx)
    {
        bool isDead = bool.Parse(PlayerPrefs.GetString("Boss" + idx, "false"));

        if (isDead)
        {
            if (Instance.boss[idx].activeSelf) Instance.boss[idx].SetActive(false);
        }
        else
        {
            if (render)
            {
                if (!Instance.boss[idx].activeSelf) Instance.boss[idx].SetActive(true);
                MouseRotation.LockCursor();
            }
            else
            {
                if (Instance.boss[idx].activeSelf) Instance.boss[idx].SetActive(false);
                MouseRotation.UnlockCursor();
            }
        }
    }

    public static void RenderMenu(bool render)
    {
        if (render)
        {
            if (!Instance.menu.activeSelf) Instance.menu.SetActive(true);
            MouseRotation.UnlockCursor();
        }
        else
        {
            if (Instance.menu.activeSelf) Instance.menu.SetActive(false);
            MouseRotation.LockCursor();
        }
    }

    public static void RenderGameOverCanvas(bool render)
    {
        if (render)
        {
            if (!Instance.gameOverCanvas.activeSelf) Instance.gameOverCanvas.SetActive(true);
            MouseRotation.UnlockCursor();
        }
        else
        {
            if (Instance.gameOverCanvas.activeSelf) Instance.gameOverCanvas.SetActive(false);
        }
    }

    public static bool EndGame()
    {
        bool areDead = true;
        foreach (BossBehaviour boss in Instance.bossBehaviours)
        {
            if (!boss.CheckIfDead())
            {
                areDead = false;
            }
        }

        return areDead;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        go = GameObject.FindGameObjectsWithTag("UIContainer");
        menu = GameObject.FindGameObjectWithTag("Menu");
        boss = GameObject.FindGameObjectsWithTag("BossHealthBar");
        bossBehaviours = FindObjectsOfType<BossBehaviour>();
        gameOverCanvas = GameObject.FindGameObjectWithTag("GameOverContainer");
    }
    void Start()
    {
        RenderMenu(true);
        RenderGameOverCanvas(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Instance.GameState == GameState.GamePause)
            {
                RenderMenu(false);
                RenderBossesHealthBar(true, 0);
                RenderBossesHealthBar(true, 1);
                RenderUI(true);
                Instance.GameState = GameState.GamePlay;
            }
            else if (Instance.GameState == GameState.GamePlay)
            {
                RenderBossesHealthBar(false, 0);
                RenderBossesHealthBar(false, 1);
                RenderUI(false);
                RenderMenu(true);
                Instance.GameState = GameState.GamePause;
            }
        }
    }

    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared.");
    }
}
