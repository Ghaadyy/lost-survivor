using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSphereBehaviour : MonoBehaviour
{
    public float damage;
    // Use this for initialization
    void Start()
    {
        damage = -20.0f;
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
        Debug.Log("COLLIDED WITH BOSS");
        GameObject player = other.gameObject;

        if (player.tag == "Player")
        {
            HealthBar healthBar = GameObject.FindObjectOfType<HealthBar>();
            healthBar.SetHealth(damage);
        }
    }
}