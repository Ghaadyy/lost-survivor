using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class CollisionCheck : MonoBehaviour
{
    private float max_distance = 0.2f;

    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
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
