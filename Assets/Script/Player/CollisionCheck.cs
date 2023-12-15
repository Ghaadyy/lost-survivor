using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class CollisionCheck : MonoBehaviour
{
    private float max_distance = 1.5f;

    public GameObject HitSphere, HitPt;

    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        HitSphere.GetComponent<MeshRenderer>().enabled = true;
    }

    void Update()
    {
        FireFallingRay();
    }

    void FireFallingRay() //Check if player is falling
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



    //Animation events

    private void InstantiateHitSphere()
    {
        Instantiate(HitSphere, HitPt.transform.position, Quaternion.identity);
    }
}
