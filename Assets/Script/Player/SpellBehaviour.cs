using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBehaviour : MonoBehaviour
{
    private float damage = -30;
    void Start()
    {
        Destroy(gameObject, 10.0f);
    }
    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            FloatingHealthBar healthBar = collision.gameObject.GetComponentInChildren<FloatingHealthBar>();
            healthBar.SetHealthBar(damage);
        }

        Destroy(gameObject);
    }
}
