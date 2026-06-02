using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MageAI : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string ShootTrigger = "shoot";

    public enum MageState
    {
        Idle,
        Attacking,
        Teleporting
    }

    [Header("Disparo")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float attackRange = 14f;
    public float attackCooldown = 2f;
    public float attackDelay = 0.4f;
    public float targetAimHeight = 0.8f;
    public float minShootDistance = 0.03f;
    public int damage = 10;

    [Header("Teleport")]
    public TeleportArea teleportArea;
    public GameObject teleportEffect;
    public float teleportCooldown = 5f;
    public float teleportIfPlayerCloserThan = 6f;
    public float minDistanceFromPlayer = 5f;
    public int teleportAttempts = 20;
    public float teleportRetryCooldown = 1f;
    public float attackDelayAfterTeleport = 0.5f;
    public float teleportEffectLifetime = 1.5f;
    public bool teleportEveryCooldown = true;

    [Header("Visual")]
    public GameObject visualObject;
    public float disappearTime = 0.12f;
    public float appearTime = 0.08f;
    public float minLookDistance = 0.1f;

    private Transform player;
    private Animator animator;
    private NavMeshAgent agent;
    private EnemyBase enemyBase;
    private Rigidbody rb;
    private MageLevitation levitation;

    private MageState currentState = MageState.Idle;

    private float nextAttackTime;
    private float nextTeleportTime;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(PlayerTag);

        if (playerObj != null)
            player = playerObj.transform;

        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyBase = GetComponent<EnemyBase>();
        rb = GetComponent<Rigidbody>();
        levitation = GetComponentInChildren<MageLevitation>();

        if (enemyBase != null)
            enemyBase.updatePathToPlayer = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        StopAgent();
    }

    void Update()
    {
        if (player == null)
            return;

        LookAtPlayer();

        switch (currentState)
        {
            case MageState.Idle:
                UpdateIdleState();
                break;

            case MageState.Attacking:
                break;

            case MageState.Teleporting:
                break;
        }
    }

    private void UpdateIdleState()
    {
        float distanceToPlayer = (transform.position - player.position).sqrMagnitude;
        float attackRangeSqr = attackRange * attackRange;
        float teleportDistanceSqr = teleportIfPlayerCloserThan * teleportIfPlayerCloserThan;

        bool playerTooClose = distanceToPlayer < teleportDistanceSqr;
        bool canTeleportByTime = teleportEveryCooldown && distanceToPlayer <= attackRangeSqr;

        if (Time.time >= nextTeleportTime && (playerTooClose || canTeleportByTime))
        {
            StartCoroutine(Teleport());
            return;
        }

        if (distanceToPlayer <= attackRangeSqr && Time.time >= nextAttackTime)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        currentState = MageState.Attacking;
        nextAttackTime = Time.time + attackCooldown;

        StopAgent();

        if (animator != null)
            animator.SetTrigger(ShootTrigger);

        yield return new WaitForSeconds(attackDelay);

        if (currentState == MageState.Attacking)
            Shoot();

        currentState = MageState.Idle;
    }

    private void Shoot()
    {
        if (firePoint == null || projectilePrefab == null)
            return;

        Vector3 targetPosition = player.position + Vector3.up * targetAimHeight;
        Vector3 direction = targetPosition - firePoint.position;

        if (direction.sqrMagnitude <= minShootDistance * minShootDistance)
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

    private IEnumerator Teleport()
    {
        Vector3 teleportPosition;

        if (!FindTeleportPosition(out teleportPosition))
        {
            nextTeleportTime = Time.time + teleportRetryCooldown;
            yield break;
        }

        currentState = MageState.Teleporting;

        nextTeleportTime = Time.time + teleportCooldown;
        nextAttackTime = Time.time + disappearTime + appearTime + attackDelayAfterTeleport;

        StopAgent();

        SpawnTeleportEffect(transform.position);

        if (visualObject != null)
            visualObject.SetActive(false);

        yield return new WaitForSeconds(disappearTime);

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.Warp(teleportPosition);
        }
        else
        {
            transform.position = teleportPosition;
        }

        StopAgent();
        ResetLevitationBasePosition();

        SpawnTeleportEffect(transform.position);

        yield return new WaitForSeconds(appearTime);

        if (visualObject != null)
            visualObject.SetActive(true);

        LookAtPlayer();

        currentState = MageState.Idle;
    }

    private bool FindTeleportPosition(out Vector3 result)
    {
        if (teleportArea == null)
        {
            result = transform.position;
            return false;
        }

        for (int i = 0; i < teleportAttempts; i++)
        {
            if (!teleportArea.GetRandomPoint(out result))
                continue;

            float distanceToPlayer = (result - player.position).sqrMagnitude;
            float minDistance = minDistanceFromPlayer * minDistanceFromPlayer;

            if (distanceToPlayer >= minDistance)
                return true;
        }

        result = transform.position;
        return false;
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= minLookDistance * minLookDistance)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        if (levitation != null)
            levitation.SetLookRotation(targetRotation);
        else
            transform.rotation = targetRotation;
    }

    private void StopAgent()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void ResetLevitationBasePosition()
    {
        if (levitation != null)
            levitation.ResetBasePosition();
    }

    private void SpawnTeleportEffect(Vector3 position)
    {
        if (teleportEffect == null)
            return;

        GameObject effect = Instantiate(teleportEffect, position, Quaternion.identity);
        Destroy(effect, teleportEffectLifetime);
    }
}
