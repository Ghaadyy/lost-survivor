using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSphereBehaviour : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {
        Destroy(gameObject, 0.25f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with hit sphere");
        GameObject obj = other.gameObject;
        if (obj.tag == "Enemy")
        {
            FloatingHealthBar enemy = obj.GetComponentInChildren<FloatingHealthBar>();
            if (enemy != null)
            {
                enemy.SetHealthBar(-10);
            }
        }
    }
}
