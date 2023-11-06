using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCombat : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        punch("PunchL", 0); // Punch Left
    }

    void punch(string name, int button)
    {
        if (Input.GetMouseButton(button))
        {
            animator.SetBool(name, true);
        }
        else
        {
            animator.SetBool(name, false);
        }
    }
}
