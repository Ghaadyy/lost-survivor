using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSphereBehaviour : MonoBehaviour
{
    private HumanCombat player;
    void Start()
    {
        player = FindObjectOfType<HumanCombat>();
    }
    void Update()
    {
        if (GameManager.Instance.GameState == GameState.GamePlay) Destroy(gameObject, 0.25f);
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            if (GameManager.Instance.GameState == GameState.GamePlay)
            {
                GameObject obj = other.gameObject;
                if (obj.tag == "Enemy")
                {
                    FloatingHealthBar enemy = obj.GetComponentInChildren<FloatingHealthBar>();
                    if (enemy != null)
                    {
                        enemy.SetHealthBar(player.GetPlayerDamage() * -1);
                    }

                    BossBehaviour boss = obj.GetComponentInChildren<BossBehaviour>();
                    if (boss != null)
                    {
                        BossHealthBar bossHealthBar = FindObjectOfType<BossHealthBar>();
                        bossHealthBar.SetHealthBar(player.GetPlayerDamage() * -1);
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
