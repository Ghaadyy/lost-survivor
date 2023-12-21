using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class CollisionCheck : MonoBehaviour
{
    private float max_distance = 1.5f;

    public GameObject HitSphere, HitPt;

    private Animator animator;

    private AudioSource[] source;
    private int source_idx = 0;
    void Start()
    {
        animator = GetComponent<Animator>();
        source = GetComponents<AudioSource>();  
    }

    void Update()
    {
        if(GameManager.Instance.GameState == GameState.GamePlay)
        {
            FireFallingRay();
        }
    }

    void FireFallingRay() //Check if player is falling
    {
        Vector3 position = transform.position;
        Vector3 down = -transform.up;
        Ray ray = new Ray(position, down);
        RaycastHit hitData;
        if (Physics.Raycast(ray, out hitData, max_distance))
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
        if(source_idx > 2)
        {
            source_idx = 0;
        }
        source[source_idx].Play();
        source_idx++;
    }
}
