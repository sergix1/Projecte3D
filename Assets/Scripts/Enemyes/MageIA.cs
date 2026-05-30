using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MageAI : MonoBehaviour
{
    [Header("Disparo")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float attackRange = 14f;
    public float attackCooldown = 2f;
    public float attackDelay = 0.4f;
    public int damage = 10;

    [Header("Teleport")]
    public float teleportCooldown = 4f;
    public float teleportMinDistance = 6f;
    public float teleportMaxDistance = 10f;
    public float teleportIfPlayerCloserThan = 4f;
    public GameObject teleportEffect;

    private Transform player;
    private Animator animator;
    private NavMeshAgent agent;

    private bool attacking;
    private float nextAttackTime;
    private float nextTeleportTime;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        LookAtPlayer();

        if (distance < teleportIfPlayerCloserThan && Time.time >= nextTeleportTime)
        {
            TeleportAroundPlayer();
            return;
        }

        if (distance <= attackRange && Time.time >= nextAttackTime && !attacking)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        attacking = true;
        nextAttackTime = Time.time + attackCooldown;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
            animator.SetTrigger("shoot");

        yield return new WaitForSeconds(attackDelay);

        Shoot();

        attacking = false;
    }

    private void Shoot()
    {
        if (firePoint == null || projectilePrefab == null)
            return;

        Vector3 targetPosition = player.position + Vector3.up * 0.8f;
        Vector3 direction = targetPosition - firePoint.position;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        MageProjectile projectile = projectileObj.GetComponentInChildren<MageProjectile>();

        if (projectile != null)
        {
            projectile.damage = damage;
            projectile.SetDirection(direction);
        }
    }

    private void TeleportAroundPlayer()
    {
        Vector3 teleportPosition;

        if (!FindTeleportPosition(out teleportPosition))
            return;

        if (teleportEffect != null)
            Instantiate(teleportEffect, transform.position, Quaternion.identity);

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.Warp(teleportPosition);
            agent.isStopped = true;
            agent.ResetPath();
        }
        else
        {
            transform.position = teleportPosition;
        }

        if (teleportEffect != null)
            Instantiate(teleportEffect, transform.position, Quaternion.identity);

        nextTeleportTime = Time.time + teleportCooldown;
        nextAttackTime = Time.time + 0.5f;

        LookAtPlayer();
    }

    private bool FindTeleportPosition(out Vector3 result)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(teleportMinDistance, teleportMaxDistance);

            Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y) * distance;
            Vector3 wantedPosition = player.position + offset;

            if (NavMesh.SamplePosition(wantedPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}