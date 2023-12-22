using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGamePoint : MonoBehaviour
{

    public GameObject portalEffect;
    // Start is called before the first frame update
    void Start()
    {
        portalEffect.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("endpoint entered");
        if (GameManager.Instance.GameState == GameState.GamePlay)
        {
            if (other.gameObject.tag == "Player")
            {
                if (GameManager.EndGame())
                {
                    GameManager.Instance.GameState = GameState.GameOver;
                    GameManager.RenderUI(false);
                    GameManager.RenderMenu(false);
                    GameManager.RenderGameOverCanvas(true);
                    GameManager.Instance.ClearPlayerPrefs();
                }
            }
        }
    }
}
