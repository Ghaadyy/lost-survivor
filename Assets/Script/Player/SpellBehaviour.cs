using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBehaviour : MonoBehaviour
{
    [SerializeField][Range(0, 50)] private float Speed = 10.0f;
    private float damage = -30;
    void Start()
    {
        Destroy(gameObject, 10.0f);
    }
    void Update()
    {
        //transform.position = transform.forward * 1 * Time.deltaTime;
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
