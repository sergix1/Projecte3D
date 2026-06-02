using UnityEngine;
using UnityEngine.AI;

public class ClickToMove : MonoBehaviour
{
    private const int ClickRaycastBufferSize = 16;
    private const string GroundTag = "Ground";
    private const string ShootTrigger = "shoot";
    private const string SpeedParameter = "speed";
    private const float MoveStopMargin = 0.05f;
    private const float MinShootDirectionSqr = 0.001f;
    private const float ClickPointRotationX = 90f;

    [Header("UI")]
    public AbilityCooldownUI tripleAttackCooldownUI;
    public float tripleAttackCooldown = 5f;

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
    public float navMeshSampleRadius = 2f;
    public float repeatedClickDistance = 2f;
    public float clickPointHeight = 0.03f;
    public float rangeMarkerHeight = 0.04f;
    public float fallbackAimHeight = 0.8f;
    public KeyCode volleyKey = KeyCode.W;
    public KeyCode rangeKey = KeyCode.C;
    public LayerMask groundLayer;
    public LayerMask clickBlockLayer = ~0;
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

    private float timeSinceLastShot;
    private Vector3 pendingProjectileDirection;
    private Vector3 lastMoveDestination;
    private Vector3 lastMoveClickPoint;
    private GameObject rangeMarker;
    private bool canTripleAttack = true;
    private static readonly RaycastHit[] clickHits = new RaycastHit[ClickRaycastBufferSize];

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        cam = Camera.main;
    }

    void Update()
    {
        timeSinceLastShot += Time.deltaTime;

        if (Input.GetMouseButtonDown(1))
            MoveToMousePosition();

        if (Input.GetMouseButtonDown(0))
            TryAttackMousePosition();

        if (Input.GetKeyDown(volleyKey))
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

        SpawnClickPoint(clickPointPrefab, hit.point);

        if (IsRepeatedClick(hit.point))
            return;

        int areaMask = agent.areaMask;

        if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshSampleRadius, areaMask))
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

        return (previousPoint - point).sqrMagnitude < repeatedClickDistance * repeatedClickDistance;
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
        return (currentDestination - point).sqrMagnitude < repeatedClickDistance * repeatedClickDistance;
    }


    private bool IsMovingToDestination()
    {
        return agent != null
            && agent.enabled
            && agent.isOnNavMesh
            && !agent.isStopped
            && (agent.pathPending || (agent.hasPath && agent.remainingDistance > agent.stoppingDistance + MoveStopMargin));
    }

    private void TryAttackMousePosition()
    {
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (timeSinceLastShot >= shootCooldown)
        {
            Transform closestEnemy = GetClosestEnemyInRange(hit.point);
            if (closestEnemy != null && TryShoot(closestEnemy, GetEnemyAimPoint(closestEnemy)))
                timeSinceLastShot = 0f;
        }

        SpawnClickPoint(clickPoint2Prefab, hit.point);
    }

    private bool TryGetGroundHit(out RaycastHit hit)
    {
        hit = default;
        if (cam == null)
            return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        int groundMask = groundLayer.value != 0 ? groundLayer.value : Physics.DefaultRaycastLayers;
        int blockMask = clickBlockLayer.value != 0 ? clickBlockLayer.value : Physics.DefaultRaycastLayers;

        int hitCount = Physics.RaycastNonAlloc(ray, clickHits, Mathf.Infinity, blockMask, QueryTriggerInteraction.Ignore);
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit currentHit = clickHits[i];

            if (currentHit.collider == null || currentHit.collider.transform.IsChildOf(transform))
                continue;

            if (!IsGroundHit(currentHit, groundMask) || currentHit.distance >= closestDistance)
                continue;

            closestDistance = currentHit.distance;
            hit = currentHit;
        }

        return hit.collider != null;
    }

    private bool IsGroundHit(RaycastHit hit, int groundMask)
    {
        int hitLayerMask = 1 << hit.collider.gameObject.layer;
        return (groundMask & hitLayerMask) != 0 || hit.collider.CompareTag(GroundTag);
    }

    private void SpawnClickPoint(GameObject prefab, Vector3 point)
    {
        if (prefab == null)
            return;

        Instantiate(prefab, point + Vector3.up * clickPointHeight, Quaternion.Euler(ClickPointRotationX, 0f, 0f));
    }

    private Transform GetClosestEnemyInRange(Vector3 hitPoint)
    {
        Transform closestEnemy = null;
        float closestDistanceToClick = Mathf.Infinity;

        if (enemyes == null)
            return null;

        foreach (Transform enemy in enemyes)
        {
            if (!enemy.TryGetComponent<EnemyBase>(out _))
                continue;

            Vector3 enemyAimPoint = GetEnemyAimPoint(enemy);
            Vector3 flatToPlayer = enemyAimPoint - transform.position;
            flatToPlayer.y = 0;

            if (flatToPlayer.sqrMagnitude > range * range)
                continue;

            float distanceToClick = Vector3.Distance(enemyAimPoint, hitPoint);
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

        Vector3 flatToEnemy = targetPoint - transform.position;
        flatToEnemy.y = 0;

        if (flatToEnemy.sqrMagnitude > range * range || flatToEnemy.sqrMagnitude <= MinShootDirectionSqr)
            return false;

        StopMovement();
        transform.rotation = Quaternion.LookRotation(flatToEnemy.normalized) * Quaternion.Euler(0, RotationOffset, 0);
        SelectEnemy(enemy);

        canShoot = false;
        isShootingAnimation = true;
        pendingAttack = true;
        pendingProjectileDirection = (targetPoint - SpawnBulletPos.position).normalized;

        if (animator != null)
            animator.SetTrigger(ShootTrigger);

        Invoke(nameof(FirePendingAttack), attackWindup);
        Invoke(nameof(ResetShootAnimation), shootCooldown);
        Invoke(nameof(ResetShoot), shootCooldown);

        return true;
    }

    private Vector3 GetEnemyAimPoint(Transform enemy)
    {
        if (enemy == null)
            return Vector3.zero;

        Collider enemyCollider = enemy.GetComponentInChildren<Collider>();
        if (enemyCollider != null)
            return enemyCollider.bounds.center;

        Renderer enemyRenderer = enemy.GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
            return enemyRenderer.bounds.center;

        return enemy.position + Vector3.up * fallbackAimHeight;
    }

    private void TryVolleyMousePosition()
    {
        if (!canShoot || !canTripleAttack || isShootingAnimation || SpawnBulletPos == null)
            return;
        if (tripleAttackCooldownUI != null && !tripleAttackCooldownUI.CanUse())
            return;
        if (!TryGetGroundHit(out RaycastHit hit))
            return;

        Vector3 direction = hit.point - SpawnBulletPos.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= MinShootDirectionSqr)
            return;

        direction.Normalize();

        StopMovement();

        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, RotationOffset, 0);

        canShoot = false;
        canTripleAttack = false;
        isShootingAnimation = true;

        if (tripleAttackCooldownUI != null)
            tripleAttackCooldownUI.StartCooldown(tripleAttackCooldown);

        if (animator != null)
            animator.SetTrigger(ShootTrigger);

        InstantiateProjectile(Quaternion.Euler(0f, -volleySpreadAngle, 0f) * direction);
        InstantiateProjectile(direction);
        InstantiateProjectile(Quaternion.Euler(0f, volleySpreadAngle, 0f) * direction);

        Invoke(nameof(ResetShootAnimation), shootCooldown);
        Invoke(nameof(ResetShoot), shootCooldown);
        Invoke(nameof(ResetTripleAttack), tripleAttackCooldown);
    }

    private void ResetTripleAttack()
    {
        canTripleAttack = true;
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
            animator.ResetTrigger(ShootTrigger);
            animator.speed = 1f;
        }
    }

    public void StopManualMovement()
    {
        StopMovement();
        if (animator != null)
            animator.SetFloat(SpeedParameter, 0f);
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
        if (Input.GetKey(rangeKey))
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
            animator.SetFloat(SpeedParameter, 0f);
            return;
        }

        bool moving = IsMovingToDestination();

        animator.speed = moving ? manualWalkPlaybackSpeed : 1f;
        animator.SetFloat(SpeedParameter, moving ? manualAnimationSpeed : 0f);
    }

    private void ShowRangeMarker()
    {
        if (RangePrefab == null)
            return;

        if (rangeMarker == null)
            rangeMarker = Instantiate(RangePrefab);

        float diameter = range * 2f;
        rangeMarker.transform.localScale = new Vector3(diameter, diameter, rangeMarkerThickness);
        rangeMarker.transform.position = transform.position + Vector3.up * rangeMarkerHeight;
        rangeMarker.SetActive(true);
    }

    private void HideRangeMarker()
    {
        if (rangeMarker != null)
            rangeMarker.SetActive(false);
    }
}





