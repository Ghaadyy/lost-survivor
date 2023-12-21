using UnityEngine;
using System.Collections;

public class SkillBehaviour : MonoBehaviour
{
    [SerializeField]
    private int explosionRadius = 20;

    [SerializeField]
    private int explosionForce = 10;

    // Use this for initialization
    void Start()
    {

    }

    void SkillForce()
    {
        addExplosionForce();
    }

    void addExplosionForce()
    {
        // Detect all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in colliders)
        {
            // Check if the object has a rigidbody
            if (col.gameObject.tag != "Enemy" && col.TryGetComponent(out Rigidbody rb))
            {
                // Apply explosion force to the rigidbody
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.0f, ForceMode.Impulse);
            }
        }
    }
}

