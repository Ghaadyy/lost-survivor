using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSphereBehaviour : MonoBehaviour
{
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
                enemy.SetHealthBar(-10);
            }
        }
    }
}
