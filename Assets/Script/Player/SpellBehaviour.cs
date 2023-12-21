using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpellBehaviour : MonoBehaviour
{
    private float damage = -30;
    private float speed = 40.0f;
    void Start()
    {
        Destroy(gameObject, 10.0f);
    }
    void Update()
    {
        if (GameManager.Instance.GameState == GameState.GamePlay) transform.position += transform.forward * Time.deltaTime * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        try
        {
            if (GameManager.Instance.GameState == GameState.GamePlay)
            {
                if (collision.gameObject.tag == "Enemy")
                {
                    if (collision.gameObject.GetComponentInChildren<BossBehaviour>() != null)
                    {
                        BossHealthBar healthBar = FindObjectOfType<BossHealthBar>();
                        healthBar.SetHealthBar(damage);
                    }
                    else
                    {
                        FloatingHealthBar healthBar = collision.gameObject.GetComponentInChildren<FloatingHealthBar>();
                        healthBar.SetHealthBar(damage);
                    }
                }

                Destroy(gameObject);
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
