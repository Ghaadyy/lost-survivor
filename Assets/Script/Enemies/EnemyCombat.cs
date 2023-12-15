using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, 0.25f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision!");
        GameObject player = other.gameObject;

        if (player.tag == "Player")
        {
            Debug.Log("collided with player");
            HealthBar healthBar = GameObject.FindObjectOfType<HealthBar>();
            Debug.Log(healthBar);
            healthBar.DecreaseHealth();
        }
    }
}