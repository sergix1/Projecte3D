using UnityEngine;
using UnityEngine.AI;

public class ClickToMove : MonoBehaviour
{
    public Transform SpawnBulletPos;

    NavMeshAgent agent;
    Animator animator;
    Camera cam;
    CapsuleCollider bodyCollider;

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
    public LayerMask groundLayer;
    public LayerMask blockingLayer = Physics.DefaultRaycastLayers;
    public bool useNavMeshMovement = false;
    public bool validateColliderPath = false;
    public float manualMoveSpeed = 2.3f;
    public float manualAnimationSpeed = 1f;
    public float manualWalkPlaybackSpeed = 1.1f;
    public float manualCollisionSkin = 0.04f;

    bool canShoot = true;
    bool isShootingAnimation = false;
    bool pendingAttack = false;
    bool useManualMovement = false;
    bool hasManualDestination = false;

    float currentTimeToAA;
    Vector3 pendingProjectileDirection;
    Vector3 manualDestination;
    GameObject rangeMarker;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        bodyCollider = GetComponent<CapsuleCollider>();
        agent.updateRotation = false;
        cam = Camera.main;

        if (useNavMeshMovement)
            EnsureAgentOnNavMesh();
        else
            agent.enabled = false;
    }

    void Update()
    {
        currentTimeToAA += UnityEngine.Time.deltaTime;

        // CLICK DERECHO
        if (Input.GetMouseButtonDown(1))
        {
            if (TryGetMovementHit(out RaycastHit hit))
            {
                CancelAttack();
                TryMoveTo(hit.point);
            }
           
        }

        // CLICK IZQUIERDO
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (currentTimeToAA > 0.5f)
                {
                    Transform closestEnemy = GetClosestEnemyInRange(hit.point);

                    if (closestEnemy != null)
                    {
                        if (TryShoot(closestEnemy, closestEnemy.position))
                            currentTimeToAA = 0;
                    }
                }

                Instantiate(
                    clickPoint2Prefab,
                    hit.point + Vector3.up * 0.03f,
                    Quaternion.Euler(90f, 0f, 0f)
                );
            }
        }
        if (!useManualMovement && CanUseAgent() && !isShootingAnimation && agent.velocity.magnitude > 0.1f)
        {
            Vector3 moveDirection = agent.velocity.normalized;
            moveDirection.y = 0;

            Quaternion lookRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                UnityEngine.Time.deltaTime * 10f
            );
        }

        UpdateManualMovement();

        // DEBUG RANGE
        if (Input.GetKey(KeyCode.C))
            ShowRangeMarker();
        else
            HideRangeMarker();

        // ANIMACION MOVIMIENTO
        if (isShootingAnimation)
        {
            animator.speed = 1f;
            animator.SetFloat("speed", 0);
        }
        else if (useManualMovement)
        {
            if (!hasManualDestination)
            {
                animator.speed = 1f;
                animator.SetFloat("speed", 0);
            }
        }
        else
        {
            animator.speed = 1f;
            float speed = CanUseAgent() ? agent.velocity.magnitude : 0f;

            animator.SetFloat("speed", speed);
        }

    }

    void ResetShoot()
    {
        canShoot = true;
    }

    void ResetShootAnimation()
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
        if (CanUseAgent())
            agent.isStopped = false;

        isShootingAnimation = false;
        pendingAttack = false;
    }

    private bool TryShoot(Transform enemy, Vector3 targetPoint)
    {
        if (!canShoot || isShootingAnimation)
            return false;

        Vector3 flatToEnemy = enemy.position - transform.position;
        flatToEnemy.y = 0;

        if (flatToEnemy.sqrMagnitude > range * range)
            return false;

        if (flatToEnemy.sqrMagnitude <= 0.001f)
            return false;

        transform.rotation =
            Quaternion.LookRotation(flatToEnemy.normalized)
            * Quaternion.Euler(0, RotationOffset, 0);

        animator.speed = 1f;
        SelectEnemy(enemy);

        canShoot = false;
        hasManualDestination = false;

        if (CanUseAgent())
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        isShootingAnimation = true;

        animator.SetTrigger("shoot");
        pendingAttack = true;
        pendingProjectileDirection = (targetPoint - SpawnBulletPos.position).normalized;

        Invoke(nameof(FirePendingAttack), attackWindup);
        Invoke(nameof(ResetShootAnimation), shootCooldown);
        Invoke(nameof(ResetShoot), shootCooldown);

        return true;
    }

    private void CancelAttack()
    {
        if (!isShootingAnimation && !pendingAttack)
            return;

        CancelInvoke(nameof(FirePendingAttack));
        CancelInvoke(nameof(ResetShootAnimation));

        pendingAttack = false;
        isShootingAnimation = false;

        if (CanUseAgent())
            agent.isStopped = false;

        animator.ResetTrigger("shoot");
        animator.speed = 1f;
        animator.SetFloat("speed", CanUseAgent() ? agent.velocity.magnitude : 0f);
    }

    private Transform GetClosestEnemyInRange(Vector3 hitPoint)
    {
        Transform closestEnemy = null;
        float closestDistanceToClick = Mathf.Infinity;

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

    private bool TryGetMovementHit(out RaycastHit hit)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        int movementMask = groundLayer.value != 0 ? groundLayer.value : Physics.DefaultRaycastLayers;
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, movementMask, QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            hit = default;
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit candidate in hits)
        {
            if (candidate.collider.transform.IsChildOf(transform))
                continue;

            hit = candidate;
            return true;
        }

        hit = default;
        return false;
    }

    private void TryMoveTo(Vector3 point)
    {
        if (!useNavMeshMovement)
        {
            MoveManuallyTo(point);
            return;
        }

        if (!EnsureAgentOnNavMesh())
        {
            MoveManuallyTo(point);
            return;
        }

        if (!NavMesh.SamplePosition(point, out NavMeshHit navHit, 2f, agent.areaMask))
        {
            MoveManuallyTo(point);
            return;
        }

        Vector3 destination = navHit.position;
        NavMeshPath path = new NavMeshPath();

        if (!agent.CalculatePath(destination, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            MoveManuallyTo(point);
            return;
        }

        if (validateColliderPath && PathBlockedByCollider(path))
            return;

        agent.isStopped = false;
        if (!agent.SetPath(path))
        {
            MoveManuallyTo(point);
            return;
        }

        Vector3 pos = destination;
        pos.y += 0.03f;

        Instantiate(
            clickPointPrefab,
            pos,
            Quaternion.Euler(90f, 0f, 0f)
        );
    }

    private void MoveManuallyTo(Vector3 point)
    {
        useManualMovement = true;
        hasManualDestination = true;
        manualDestination = point;

        if (agent.enabled)
            agent.enabled = false;

        Vector3 pos = manualDestination;
        pos.y += 0.03f;

        Instantiate(
            clickPointPrefab,
            pos,
            Quaternion.Euler(90f, 0f, 0f)
        );
    }

    private void UpdateManualMovement()
    {
        if (!useManualMovement || !hasManualDestination || isShootingAnimation)
            return;

        Vector3 current = transform.position;
        Vector3 target = manualDestination;
        current.y = 0f;
        target.y = 0f;

        Vector3 toTarget = target - current;
        if (toTarget.sqrMagnitude <= 0.01f)
        {
            hasManualDestination = false;
            animator.SetFloat("speed", 0f);
            return;
        }

        Vector3 direction = toTarget.normalized;
        float speed = manualMoveSpeed > 0f ? manualMoveSpeed : 2.3f;
        float step = speed * UnityEngine.Time.deltaTime;
        Vector3 displacement = direction * Mathf.Min(step, toTarget.magnitude);

        if (!TryGetManualDisplacement(displacement, out Vector3 safeDisplacement))
        {
            hasManualDestination = false;
            animator.speed = 1f;
            animator.SetFloat("speed", 0f);
            return;
        }

        Vector3 nextPosition = transform.position + safeDisplacement;
        nextPosition = ProjectPointToGround(nextPosition);

        transform.position = nextPosition;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            UnityEngine.Time.deltaTime * 10f
        );
        animator.speed = manualWalkPlaybackSpeed > 0f ? manualWalkPlaybackSpeed : 1f;
        animator.SetFloat("speed", manualAnimationSpeed);
    }

    private bool TryGetManualDisplacement(Vector3 displacement, out Vector3 safeDisplacement)
    {
        safeDisplacement = displacement;

        float distance = displacement.magnitude;
        if (distance <= 0.001f)
            return true;

        Vector3 direction = displacement / distance;
        if (!TryGetManualCapsule(out Vector3 capsuleBottom, out Vector3 capsuleTop, out float radius))
            return true;

        int mask = blockingLayer.value != 0 ? blockingLayer.value : Physics.DefaultRaycastLayers;
        RaycastHit[] hits = Physics.CapsuleCastAll(
            capsuleBottom,
            capsuleTop,
            radius,
            direction,
            distance + manualCollisionSkin,
            mask,
            QueryTriggerInteraction.Ignore
        );

        float closestDistance = Mathf.Infinity;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            int hitLayer = 1 << hit.collider.gameObject.layer;
            if ((groundLayer.value & hitLayer) != 0)
                continue;

            if (hit.distance < closestDistance)
                closestDistance = hit.distance;
        }

        if (float.IsInfinity(closestDistance))
            return true;

        float allowedDistance = closestDistance - manualCollisionSkin;
        if (allowedDistance <= 0.001f)
        {
            safeDisplacement = Vector3.zero;
            return false;
        }

        safeDisplacement = direction * Mathf.Min(distance, allowedDistance);
        return true;
    }

    public Vector3 ClampManualDisplacement(Vector3 displacement)
    {
        return TryGetManualDisplacement(displacement, out Vector3 safeDisplacement)
            ? safeDisplacement
            : Vector3.zero;
    }

    private bool TryGetManualCapsule(out Vector3 capsuleBottom, out Vector3 capsuleTop, out float radius)
    {
        if (bodyCollider != null)
        {
            Vector3 center = transform.TransformPoint(bodyCollider.center);
            Vector3 scale = transform.lossyScale;
            radius = bodyCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float height = Mathf.Max(bodyCollider.height * Mathf.Abs(scale.y), radius * 2f);
            float halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);
            capsuleBottom = center + Vector3.down * halfSegment;
            capsuleTop = center + Vector3.up * halfSegment;
            return radius > 0f;
        }

        radius = agent != null ? Mathf.Max(0.05f, agent.radius * 0.9f) : 0.25f;
        float fallbackHeight = agent != null ? Mathf.Max(agent.height, radius * 2f) : 1.8f;
        capsuleBottom = transform.position + Vector3.up * radius;
        capsuleTop = transform.position + Vector3.up * (fallbackHeight - radius);
        return true;
    }

    private Vector3 ProjectPointToGround(Vector3 point)
    {
        int movementMask = groundLayer.value != 0 ? groundLayer.value : Physics.DefaultRaycastLayers;
        Ray ray = new Ray(point + Vector3.up * 10f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 30f, movementMask, QueryTriggerInteraction.Ignore))
        {
            point.y = hit.point.y;
            return point;
        }

        return point;
    }

    private bool EnsureAgentOnNavMesh()
    {
        if (!agent.enabled)
            agent.enabled = true;

        if (agent.isOnNavMesh)
            return true;

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 5f, agent.areaMask))
        {
            agent.enabled = false;
            return false;
        }

        return agent.Warp(navHit.position);
    }

    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }

    public void StopManualMovement()
    {
        hasManualDestination = false;
        useManualMovement = false;
        animator.speed = 1f;
        animator.SetFloat("speed", 0f);
    }

    private bool PathBlockedByCollider(NavMeshPath path)
    {
        if (path.corners.Length < 2)
            return false;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            if (SegmentBlockedByCollider(path.corners[i], path.corners[i + 1]))
                return true;
        }

        return false;
    }

    private bool SegmentBlockedByCollider(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        direction.y = 0f;

        float distance = direction.magnitude;
        if (distance <= 0.05f)
            return false;

        direction /= distance;

        float radius = Mathf.Max(0.05f, agent.radius * 0.9f);
        float height = Mathf.Max(agent.height, radius * 2f);
        Vector3 capsuleBottom = from + Vector3.up * radius;
        Vector3 capsuleTop = from + Vector3.up * (height - radius);

        RaycastHit[] hits = Physics.CapsuleCastAll(
            capsuleBottom,
            capsuleTop,
            radius,
            direction,
            distance,
            blockingLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            int hitLayer = 1 << hit.collider.gameObject.layer;
            if ((groundLayer.value & hitLayer) != 0)
                continue;

            return true;
        }

        return false;
    }

    private void SelectEnemy(Transform selectedEnemy)
    {
        foreach (Transform enemy in enemyes)
        {
            if (enemy.TryGetComponent(out EnemyBase enemyBase))
                enemyBase.selected = enemy == selectedEnemy;
        }
    }

    private void InstantiateProjectile(Vector3 direction)
    {
        Quaternion projectileRotation = Quaternion.LookRotation(direction);

        var projectileObj =
            Instantiate(
                projectile,
                SpawnBulletPos.position,
                projectileRotation,
                projectileTransform
            );

        if (!projectileObj.TryGetComponent(out Projectile projectileComponent))
        {
            Debug.Log("El componente projectile no funciona");
        }
        else
        {
            projectileComponent.SetDirection(direction.normalized);
        }
    }

    private void ShowRangeMarker()
    {
        if (RangePrefab == null)
            return;

        if (rangeMarker == null)
        {
            rangeMarker = Instantiate(RangePrefab);
        }

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
