using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HumanBehaviour : MonoBehaviour
{
    private readonly float run_speed = 5.0f;
    private readonly float walk_speed = 2.0f;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {   
        RunFB(KeyCode.W, "RunF", 1); // Run Forward
        RunFB(KeyCode.S, "RunB", -1); // Run Backward
        RunSideWays(KeyCode.W, KeyCode.A, "RunL"); // Run Forward Left
        RunSideWays(KeyCode.W, KeyCode.D, "RunR"); // Run Forward Right
        
        GoSideways(KeyCode.A, "GoLeft", -1); // Go Left
        GoSideways(KeyCode.D, "GoRight", 1); // Go Right
    }

    void RunFB(KeyCode key, string name, int direction)
    {
        if (Input.GetKey(key))
        {
            transform.position += transform.forward * direction * run_speed * Time.deltaTime;
            animator.SetBool(name, true);

        }

        if (Input.GetKeyUp(key))
        {
            animator.SetBool(name, false);
        }
    }
    void RunSideWays(KeyCode key1, KeyCode key2, string name)
    {
        if (Input.GetKey(key1) && Input.GetKey(key2))
        {
            animator.SetBool(name, true);

        }
        else
        {
            animator.SetBool(name, false);
        }
    }

    void GoSideways(KeyCode key, string name, int direction)
    {
        if (Input.GetKey(key))
        {
            transform.position += transform.right * direction * walk_speed * Time.deltaTime;
            animator.SetBool(name, true);
        }

        if (Input.GetKeyUp(key))
        {
            animator.SetBool(name, false);
        }
    }
}
