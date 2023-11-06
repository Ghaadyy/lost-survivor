using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyBehaviour : MonoBehaviour
{
    private float range = 10;
    private float attackRange = 4;
    private float speed = 4;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        Collider[] attackCol = Physics.OverlapSphere(transform.position, attackRange);

        if (attackCol.Any(col => col.gameObject.tag == "Player"))
        {
            foreach (Collider col in attackCol)
            {
                if (col.gameObject.tag == "Player")
                {
                    Attack();
                }
            }
        }
        else
        {
            StopAttack();
        }

        if (colliders.Any(col => col.gameObject.tag == "Player"))
        {
            foreach (Collider col in colliders)
            {
                if (col.gameObject.tag == "Player")
                {
                    transform.position = Vector3.MoveTowards(transform.position, col.gameObject.transform.position, speed * Time.deltaTime);
                    transform.LookAt(col.transform);
                }
            }
        }
    }

    void Attack()
    {
        animator.SetBool("attack", true);
    }

    void StopAttack()
    {
        animator.SetBool("attack", false);
    }
}
