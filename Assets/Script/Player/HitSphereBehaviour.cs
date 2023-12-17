using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSphereBehaviour : MonoBehaviour
{
    private HumanCombat player;
    void Start()
    {
        player = GameObject.FindObjectOfType<HumanCombat>();
    }
    void Update()
    {
        Destroy(gameObject, 0.25f);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        if (obj.tag == "Enemy")
        {
            FloatingHealthBar enemy = obj.GetComponentInChildren<FloatingHealthBar>();
            if (enemy != null)
            {
                enemy.SetHealthBar(player.GetPlayerDamage() * -1);
            }
        }
    }
}
