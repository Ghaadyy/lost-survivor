using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HumanBehaviour : MonoBehaviour
{
    private readonly float walk_speed = 2.0f;
    private readonly float run_speed = 5.0f;
    private readonly float sprint_speed = 6.0f;
    private readonly float roll_speed = 0.5f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {   
        RunFB(KeyCode.W, "RunF", 1); // Run Forward
        RunSideWays(KeyCode.W, KeyCode.A, "RunL"); // Run Forward Left
        RunSideWays(KeyCode.W, KeyCode.D, "RunR"); // Run Forward Right
        
        RunFB(KeyCode.S, "RunB", -1); // Run Backward
        RunSideWays(KeyCode.S, KeyCode.A, "RunBL"); // Run Backward Left
        RunSideWays(KeyCode.S, KeyCode.D, "RunBR"); // Run Backward Right

        GoSideways(KeyCode.A, KeyCode.D, "GoLeft", -1); // Go Left
        GoSideways(KeyCode.D, KeyCode.A, "GoRight", 1); // Go Right

        Sprint(); // Sprint Forward

        RollFB(KeyCode.W, KeyCode.Space, "RollF", 1); // Roll Forward
        RollFB(KeyCode.S, KeyCode.Space, "RollB", -1); // Roll Backward
    }

    void RunFB(KeyCode key, string name, int direction)
    {
        if (Input.GetKey(key))
        {
            if (animator.GetBool("PunchL"))
            {
                transform.position += transform.forward * direction * walk_speed * Time.deltaTime;
            }
            else 
            {
                transform.position += transform.forward * direction * run_speed * Time.deltaTime;
            }
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

    void GoSideways(KeyCode key, KeyCode wrongKey, string name, int direction)
    {
        if (Input.GetKey(key) && !Input.GetKey(wrongKey))
        {
            transform.position += transform.right * direction * walk_speed * Time.deltaTime;
            animator.SetBool(name, true);
        }
        else
        {
            animator.SetBool(name, false);
        }
    }

    void Sprint()
    {
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            transform.position += transform.forward * sprint_speed * Time.deltaTime;
            animator.SetBool("Sprint", true);

        }
        else
        {
            animator.SetBool("Sprint", false);
        }
    }

    void RollFB(KeyCode key1, KeyCode key2, string name, int direction)
    {
        if(Input.GetKey(key1) && Input.GetKey(key2))
        {
            transform.position += transform.forward * roll_speed * direction * Time.deltaTime;
            animator.SetBool(name, true);
        }
        else
        {
            animator.SetBool(name, false);
        }
    }
}
