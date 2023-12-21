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
        GameManager.Instance.GameState = GameState.GamePlay;

        Debug.Log("Game start");
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
