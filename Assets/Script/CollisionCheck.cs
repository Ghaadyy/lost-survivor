using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class CollisionCheck : MonoBehaviour
{
    private float max_distance = 1.5f;

    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        FireFallingRay(); //Check if player is falling
    }

    void FireFallingRay()
    {
        Vector3 position = transform.position;
        Vector3 down = -transform.up;
        Ray ray = new Ray(position, down);
        RaycastHit hitData;
        if(Physics.Raycast(ray, out hitData, max_distance))
        {
            animator.SetBool("Falling", false);
        }
        else
        {
            animator.SetBool("Falling", true);
        }
    }
}
