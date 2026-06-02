using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAttack : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string AttackTrigger = "attack";

    [Header("Ataque")]
    public int damage = 10;
    public float attackRange = 1.5f;
    public float attackExtraRange = 0.3f;
    public float attackCooldown = 1.2f;
    public float attackDelay = 0.35f;
    public float attackCheckInterval = 0.12f;

    [Header("Animacion")]
    public bool lockHeightWhileAttacking = true;
    public float minLookDistance = 0.1f;
    public float attackAnimationTime = 0.9f;

    private Transform player;
    private PlayerHealth playerHealth;
    private NavMeshAgent agent;
    private Animator animator;

    private bool attacking;
    private float nextAttackTime;
    private float nextAttackCheckTime;
    private float attackStartY;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(PlayerTag);

        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        nextAttackCheckTime = Time.time + Random.Range(0f, attackCheckInterval);
    }

    void Update()
    {
        if (player == null)
            return;

        if (Time.time < nextAttackCheckTime)
            return;

        nextAttackCheckTime = Time.time + attackCheckInterval;

        if (attacking || Time.time < nextAttackTime)
            return;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= attackRange * attackRange)
        {
            StartCoroutine(Attack());
        }
    }

    private void LateUpdate()
    {
        if (!attacking)
            return;

        StopAgent();

        if (!lockHeightWhileAttacking)
            return;

        Vector3 position = transform.position;
        position.y = attackStartY;
        transform.position = position;
    }
    private void StopAgent()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private IEnumerator Attack()
    {
        attacking = true;
        nextAttackTime = Time.time + attackCooldown;
        attackStartY = transform.position.y;

        StopAgent();

        LookAtPlayer();

        if (animator != null)
            animator.SetTrigger(AttackTrigger);

        yield return new WaitForSeconds(attackDelay);

        if (playerHealth != null)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0f;

            float finalRange = attackRange + attackExtraRange;

            if (directionToPlayer.sqrMagnitude <= finalRange * finalRange)
                playerHealth.TakeDamage(damage);
        }

        float remainingAnimationTime = attackAnimationTime - attackDelay;

        if (remainingAnimationTime > 0f)
            yield return new WaitForSeconds(remainingAnimationTime);

        attacking = false;
    }
    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > minLookDistance * minLookDistance)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}
