using UnityEngine;
using System.Linq;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField][Range(1, 30)] private float sightRange = 20;

    [SerializeField][Range(1, 30)] private float attackRange = 10;

    [SerializeField] private float speed = 4;

    private Animator animator;
    private FloatingHealthBar healthBar;

    private bool isAttacking = false;

    private Vector3 destination;
    private bool walkPointSet = false;
    public float patrolRange = 10;

    public GameObject Ragdoll;

    public LayerMask whatIsGround, whatIsObstruction;

    private void Awake()
    {
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {

    }

    private void Patrol()
    {
        animator.SetBool("walk", true);

        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, 0.5f * speed * Time.deltaTime);
            transform.LookAt(destination);
        }

        // Arrived to destination
        if (transform.position == destination)
        {
            animator.SetBool("walk", false);
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-patrolRange, patrolRange);
        float randomX = Random.Range(-patrolRange, patrolRange);

        destination = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ);

        if (Physics.Raycast(destination, -transform.up, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    void Die()
    {
        gameObject.SetActive(false);
        // Ragdoll.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Die();
        }

        CheckAttackRange();
        CheckSightRange();
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
            Patrol();
        }
    }

    void CheckAttackRange()
    {
        Collider[] attackable = Physics.OverlapSphere(transform.position, attackRange);

        if (attackable.Any(col => col.gameObject.tag == "Player"))
        {
            foreach (Collider col in attackable)
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

    void Attack()
    {
        animator.SetBool("attack", true);
        isAttacking = true;
    }

    void StopAttack()
    {
        animator.SetBool("attack", false);
        isAttacking = false;
    }
}