using UnityEngine;
using UnityEngine.AI;

public class ClickToMove : MonoBehaviour
{
    public Transform SpawnBulletPos;

    public GameObject clickPointPrefab;
    public GameObject clickPoint2Prefab;
    public GameObject RangePrefab;
    public GameObject projectile;

    public Transform projectileTransform;
    public Transform enemyes;

    public float range = 10f;
    public float rangeMarkerThickness = 0.05f;
    public float RotationOffset = 90f;
    public float shootCooldown = 0.5f;
    public float attackWindup = 0.2f;
    public float volleySpreadAngle = 12f;
    public LayerMask groundLayer;
    public float manualAnimationSpeed = 1f;
    public float manualWalkPlaybackSpeed = 1.1f;

    private NavMeshAgent agent;
    private Animator animator;
    private Camera cam;

    private bool canShoot = true;
    private bool isShootingAnimation;
    private bool pendingAttack;
    private bool hasMoveDestination;
    private bool hasMoveClickPoint;

    private float currentTimeToAA;
    private Vector3 pendingProjectileDirection;
    private Vector3 lastMoveDestination;
    private Vector3 lastMoveClickPoint;
    private GameObject rangeMarker;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        cam = Camera.main;
    }

    void Update()
    {
        currentTimeToAA += Time.deltaTime;

        if (Input.GetMouseButtonDown(1))
            MoveToMousePosition();

        if (Input.GetMouseButtonDown(0))
            TryAttackMousePosition();

        if (Input.GetKeyDown(KeyCode.W))
            TryVolleyMousePosition();

        UpdateRangeMarker();
        UpdateMoveAnimation();
    }

    private void MoveToMousePosition()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (!TryGetGroundHit(out RaycastHit hit))
            return;

        // Esto se muestra SIEMPRE, aunque el click sea repetido
        SpawnClickPoint(clickPointPrefab, hit.point);

        if (IsRepeatedClick(hit.point))
            return;

        int areaMask = agent.areaMask;

        if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, areaMask))
            return;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(navHit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            return;

        if (IsSameDestination(navHit.position))
            return;

        CancelAttack();

        agent.isStopped = false;
        agent.SetDestination(navHit.position);

        lastMoveDestination = navHit.position;
        lastMoveClickPoint = hit.point;
        hasMoveDestination = true;
        hasMoveClickPoint = true;
    }

    private bool IsRepeatedClick(Vector3 point)
    {
        if (!hasMoveClickPoint || !IsMovingToDestination())
            return false;

        Vector3 previousPoint = lastMoveClickPoint;
        previousPoint.y = 0f;
        point.y = 0f;

        return (previousPoint - point).sqrMagnitude < 4f;
    }

    private bool IsSameDestination(Vector3 point)
    {
        if (!IsMovingToDestination())
            return false;

        if (!hasMoveDestination && agent.hasPath)
        {
            lastMoveDestination = agent.destination;
            hasMoveDestination = true;
        }

        if (!hasMoveDestination)
            return false;

        Vector3 currentDestination = lastMoveDestination;
        currentDestination.y = 0f;
        point.y = 0f;
        return (currentDestination - point).sqrMagnitude < 4f;
    }


    private bool IsMovingToDestination()
    {
        return agent != null
            && agent.enabled
            && agent.isOnNavMesh
            && !agent.isStopped
            && (agent.pathPending || (agent.hasPath && agent.remainingDistance > agent.stoppingDistance + 0.05f));
    }
    private void TryAttackMousePosition()
    {
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (currentTimeToAA > 0.5f)
        {
            Transform closestEnemy = GetClosestEnemyInRange(hit.point);
            if (closestEnemy != null && TryShoot(closestEnemy, closestEnemy.position))
                currentTimeToAA = 0;
        }

        SpawnClickPoint(clickPoint2Prefab, hit.point);
    }

    private bool TryGetGroundHit(out RaycastHit hit)
    {
        hit = default;
        if (cam == null)
            return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        int mask = groundLayer.value != 0 ? groundLayer.value : Physics.DefaultRaycastLayers;
        return Physics.Raycast(ray, out hit, Mathf.Infinity, mask, QueryTriggerInteraction.Ignore);
    }

    private void SpawnClickPoint(GameObject prefab, Vector3 point)
    {
        if (prefab == null)
            return;

        Instantiate(prefab, point + Vector3.up * 0.03f, Quaternion.Euler(90f, 0f, 0f));
    }

    private Transform GetClosestEnemyInRange(Vector3 hitPoint)
    {
        Transform closestEnemy = null;
        float closestDistanceToClick = Mathf.Infinity;

        if (enemyes == null)
            return null;

        foreach (Transform enemy in enemyes)
        {
            if (!enemy.TryGetComponent(out EnemyBase enemyBase))
                continue;

            Vector3 flatToPlayer = enemy.position - transform.position;
            flatToPlayer.y = 0;

            if (flatToPlayer.sqrMagnitude > range * range)
                continue;

            float distanceToClick = Vector3.Distance(enemy.position, hitPoint);
            if (distanceToClick < closestDistanceToClick)
            {
                closestEnemy = enemy;
                closestDistanceToClick = distanceToClick;
            }
        }

        return closestEnemy;
    }

    private bool TryShoot(Transform enemy, Vector3 targetPoint)
    {
        if (!canShoot || isShootingAnimation || SpawnBulletPos == null)
            return false;

        Vector3 flatToEnemy = enemy.position - transform.position;
        flatToEnemy.y = 0;

        if (flatToEnemy.sqrMagnitude > range * range || flatToEnemy.sqrMagnitude <= 0.001f)
            return false;

        StopMovement();
        transform.rotation = Quaternion.LookRotation(flatToEnemy.normalized) * Quaternion.Euler(0, RotationOffset, 0);
        SelectEnemy(enemy);

        canShoot = false;
        isShootingAnimation = true;
        pendingAttack = true;
        pendingProjectileDirection = (targetPoint - SpawnBulletPos.position).normalized;

        if (animator != null)
            animator.SetTrigger("shoot");

        Invoke(nameof(FirePendingAttack), attackWindup);
        Invoke(nameof(ResetShootAnimation), shootCooldown);
        Invoke(nameof(ResetShoot), shootCooldown);

        return true;
    }

    private void TryVolleyMousePosition()
    {
        if (!canShoot || isShootingAnimation || SpawnBulletPos == null)
            return;

        if (!TryGetGroundHit(out RaycastHit hit))
            return;

        Vector3 direction = hit.point - SpawnBulletPos.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        StopMovement();
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, RotationOffset, 0);

        canShoot = false;
        isShootingAnimation = true;

        if (animator != null)
            animator.SetTrigger("shoot");

        InstantiateProjectile(Quaternion.Euler(0f, -volleySpreadAngle, 0f) * direction);
        InstantiateProjectile(direction);
        InstantiateProjectile(Quaternion.Euler(0f, volleySpreadAngle, 0f) * direction);

        Invoke(nameof(ResetShootAnimation), shootCooldown);
        Invoke(nameof(ResetShoot), shootCooldown);
    }

    private void ResetShoot()
    {
        canShoot = true;
    }

    private void ResetShootAnimation()
    {
        EndAttack();
    }

    public void FirePendingAttack()
    {
        if (!pendingAttack)
            return;

        pendingAttack = false;
        InstantiateProjectile(pendingProjectileDirection);
    }

    public void EndAttack()
    {
        isShootingAnimation = false;
        pendingAttack = false;
    }

    private void CancelAttack()
    {
        if (!isShootingAnimation && !pendingAttack)
            return;

        CancelInvoke(nameof(FirePendingAttack));
        CancelInvoke(nameof(ResetShootAnimation));

        isShootingAnimation = false;
        pendingAttack = false;

        if (animator != null)
        {
            animator.ResetTrigger("shoot");
            animator.speed = 1f;
        }
    }

    public void StopManualMovement()
    {
        StopMovement();
        if (animator != null)
            animator.SetFloat("speed", 0f);
    }

    private void StopMovement()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void SelectEnemy(Transform selectedEnemy)
    {
        if (enemyes == null)
            return;

        foreach (Transform enemy in enemyes)
        {
            if (enemy.TryGetComponent(out EnemyBase enemyBase))
                enemyBase.selected = enemy == selectedEnemy;
        }
    }

    private void InstantiateProjectile(Vector3 direction)
    {
        if (projectile == null || SpawnBulletPos == null)
            return;

        Quaternion projectileRotation = Quaternion.LookRotation(direction);
        GameObject projectileObj = Instantiate(projectile, SpawnBulletPos.position, projectileRotation, projectileTransform);

        if (projectileObj.TryGetComponent(out Projectile projectileComponent))
            projectileComponent.SetDirection(direction.normalized);
    }

    private void UpdateRangeMarker()
    {
        if (Input.GetKey(KeyCode.C))
            ShowRangeMarker();
        else
            HideRangeMarker();
    }

    private void UpdateMoveAnimation()
    {
        if (animator == null)
            return;

        if (isShootingAnimation)
        {
            animator.speed = 1f;
            animator.SetFloat("speed", 0f);
            return;
        }

        bool moving = IsMovingToDestination();

        animator.speed = moving ? manualWalkPlaybackSpeed : 1f;
        animator.SetFloat("speed", moving ? manualAnimationSpeed : 0f);
    }

    private void ShowRangeMarker()
    {
        if (RangePrefab == null)
            return;

        if (rangeMarker == null)
            rangeMarker = Instantiate(RangePrefab);

        float diameter = range * 2f;
        rangeMarker.transform.localScale = new Vector3(diameter, diameter, rangeMarkerThickness);
        rangeMarker.transform.position = transform.position + Vector3.up * 0.04f;
        rangeMarker.SetActive(true);
    }

    private void HideRangeMarker()
    {
        if (rangeMarker != null)
            rangeMarker.SetActive(false);
    }
}





