﻿using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    public float damage;
    // Use this for initialization
    void Start()
    {
        damage = -10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GameState == GameState.GamePlay)
        {
            Destroy(gameObject, 0.25f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            if (GameManager.Instance.GameState == GameState.GamePlay)
            {
                Debug.Log("Collision!");
                GameObject player = other.gameObject;

                if (player.tag == "Player")
                {
                    Debug.Log("collided with player");
                    HealthBar healthBar = FindObjectOfType<HealthBar>();
                    Debug.Log(healthBar);
                    healthBar.SetHealth(damage);
                }
            }
        }
        catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}