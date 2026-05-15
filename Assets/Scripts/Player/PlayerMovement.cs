using UnityEngine;
using UnityEngine.AI;

public class ClickToMove : MonoBehaviour
{
    public Transform SpawnBulletPos;

    NavMeshAgent agent;
    Animator animator;
    Camera cam;

    public GameObject clickPointPrefab;
    public GameObject clickPoint2Prefab;
    public GameObject RangePrefab;
    public GameObject selectorPrefab;

    public GameObject projectile;

    public Transform projectileTransform;
    public Transform enemyes;

    public float range = 120;
    public int autosec = 2;
    public float RotationOffset = 90f;
    public float shootCooldown = 0.5f;

    bool canShoot = true;
    bool isShootingAnimation = false;

    float currentTimeToAA;

    Vector3 savedDirection;
    bool projectileShot = false;

    void Start()
    {
       
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        cam = Camera.main;
    }

    void Update()
    {
        currentTimeToAA += UnityEngine.Time.deltaTime;

        // CLICK DERECHO
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // DISPARAR A ENEMIGO
                if (hit.collider.gameObject.CompareTag("Enemy"))
                {
                    if (!canShoot)
                        return;

                    Vector3 direction =
                        (hit.point - transform.position).normalized;

                    // ROTACIÓN HACIA ENEMIGO
                    Vector3 lookDirection =
                        hit.point - transform.position;

                    lookDirection.y = 0;

                    transform.rotation =
         Quaternion.LookRotation(lookDirection.normalized)
         * Quaternion.Euler(0, RotationOffset, 0);
                    canShoot = false;

                    isShootingAnimation = true;
                    agent.isStopped = true;
                    animator.SetTrigger("shoot");
                    
                    instantiateProjectile(direction);

                    Invoke(nameof(ResetShoot), shootCooldown);

                    Invoke(
                        nameof(ResetShootAnimation),
                        shootCooldown
                    );

                    foreach (Transform enemy in enemyes)
                    {
                        enemy.GetComponent<EnemyBase>().selected = false;
                    }

                    hit.collider.gameObject
                        .GetComponent<EnemyBase>()
                        .selected = true;
                }
                // MOVERSE
                else
                {
                    if (isShootingAnimation)
                        return;

                    agent.SetDestination(hit.point);

                    Vector3 pos = hit.point;
                    pos.y += 0.03f;

                    Instantiate(
                        clickPointPrefab,
                        pos,
                        Quaternion.Euler(90f, 0f, 0f)
                    );
                }
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
                    Transform closestEnemy = null;

                    float oldDistance = Mathf.Infinity;

                    foreach (Transform enemy in enemyes)
                    {
                        float distance =
                            Vector3.Distance(enemy.position, hit.point);

                        if (distance < oldDistance)
                        {
                            closestEnemy = enemy;
                            oldDistance = distance;
                        }
                    }

                    if (closestEnemy != null)
                    {
                        if (!canShoot)
                            return;

                        foreach (Transform enemy in enemyes)
                        {
                            enemy.GetComponent<EnemyBase>().selected = false;
                        }

                        closestEnemy
                            .GetComponent<EnemyBase>()
                            .selected = true;

                        Vector3 direction =
                            (
                                closestEnemy.position
                                - transform.position
                            ).normalized;

                        // ROTACIÓN HACIA ENEMIGO
                        Vector3 lookDirection =
                            closestEnemy.position
                            - transform.position;

                        lookDirection.y = 0;

                        Quaternion targetRotation =
     Quaternion.LookRotation(
         lookDirection.normalized
     )
     * Quaternion.Euler(0, RotationOffset, 0);

                        transform.rotation = targetRotation;

                      

                        canShoot = false;
                        agent.isStopped = true;
                        isShootingAnimation = true;

                        animator.SetTrigger("shoot");
                        
                        instantiateProjectile(direction);

                        Invoke(nameof(ResetShoot), shootCooldown);

                        Invoke(
                            nameof(ResetShootAnimation),
                            shootCooldown
                        );

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
        if (!isShootingAnimation && agent.velocity.magnitude > 0.1f)
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
        // DEBUG RANGE
        if (Input.GetKeyDown(KeyCode.C))
        {
            Instantiate(
                RangePrefab,
                transform.position + Vector3.up * 0.03f,
                Quaternion.Euler(90f, 0f, 0f)
            );
        }

        // ANIMACIÓN MOVIMIENTO
        if (isShootingAnimation)
        {
            animator.SetFloat("speed", 0);
        }
        else
        {
            float speed = agent.velocity.magnitude;

            animator.SetFloat("speed", speed);
        }

    }

    void ResetShoot()
    {
        canShoot = true;
    }

    void ResetShootAnimation()
    {
        agent.isStopped = false;
        isShootingAnimation = false;
    }

    private void instantiateProjectile(Vector3 direction)
    {
        Quaternion projectileRotation =
    Quaternion.LookRotation(direction);

        var projectileObj =
            Instantiate(
                projectile,
                SpawnBulletPos.position,
                projectileRotation,
                projectileTransform
            );

        if (projectileObj.GetComponent<Projectile>() == null)
        {
            Debug.Log("El componente projectile no funciona");
        }
        else
        {
            projectileObj
                .GetComponent<Projectile>()
                .SetDirection(direction.normalized);
        }
    }
}