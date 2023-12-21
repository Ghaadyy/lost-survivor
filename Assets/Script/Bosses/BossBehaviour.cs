using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossBehaviour : MonoBehaviour
{
    private Animator animator;
    private BossHealthBar bossHealthBar;
    public GameObject Ragdoll;

    private int AttackTimeout = 3;
    private float AttackTimer = 0;
    private int AttackRange = 20;
    private bool canAttack = true;
    private bool isAttacking = false;

    private bool isDead = false;

    private int sightRange = 40;

    private int speed = 5;

    public int BossNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        bossHealthBar = GameObject.FindObjectsOfType<BossHealthBar>()[BossNumber];
    }

    void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
        bossHealthBar.gameObject.SetActive(false);
        if (Ragdoll != null)
        {
            Ragdoll.SetActive(true);
            Ragdoll.transform.parent = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GameState == GameState.GamePlay)
        {
            if (bossHealthBar.CheckIfDead())
                Die();

            if (!isDead) ShowHealthBar();
            CheckAttackRange();
            CheckSightRange();
        }
    }

    void ShowHealthBar()
    {
        Collider[] sight = Physics.OverlapSphere(transform.position, sightRange);

        var playerInRange = sight.Any(col => col.gameObject.tag == "Player");

        bossHealthBar.gameObject.SetActive(playerInRange);
    }

    void CheckAttackRange()
    {
        Collider[] attackRange = Physics.OverlapSphere(transform.position, AttackRange);

        if (attackRange.Any(col => col.gameObject.tag == "Player"))
        {
            foreach (Collider col in attackRange)
            {
                if (col.gameObject.tag == "Player")
                {
                    AttackPlayer();
                    isAttacking = true;
                }
            }
        }
        else
        {
            isAttacking = false;
        }
    }

    void AttackPlayer()
    {
        var isSkill = Random.Range(0, 2) == 0;

        if (canAttack)
        {
            if (isSkill)
                animator.SetTrigger("skill");
            else
                animator.SetTrigger("attack");
            canAttack = false;
        }
        else
        {
            if (AttackTimer < AttackTimeout)
                AttackTimer += Time.deltaTime;
            else
            {
                AttackTimer = 0;
                canAttack = true;
            }
        }
    }

    void CheckSightRange()
    {
        if (isAttacking)
        {
            animator.SetBool("walk", false);
            return;
        }

        Collider[] sight = Physics.OverlapSphere(transform.position, sightRange);

        if (sight.Any(col => col.gameObject.tag == "Player"))
        {
            foreach (Collider col in sight)
            {
                if (col.gameObject.tag == "Player")
                {
                    ChasePlayer(col.gameObject.transform.position);
                }
            }
        }
        else
        {
            animator.SetBool("walk", false);
        }
    }

    void ChasePlayer(Vector3 playerPosition)
    {
        if (transform.position == playerPosition)
        {
            animator.SetBool("walk", false);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, playerPosition, speed * Time.deltaTime);
            transform.LookAt(playerPosition);
            animator.SetBool("walk", true);
        }
    }
}
