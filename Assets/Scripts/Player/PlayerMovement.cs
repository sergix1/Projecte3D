using UnityEngine;
using UnityEngine.AI;

public class ClickToMove : MonoBehaviour
{
    public Transform SpawnBulletPos;

    private NavMeshAgent agent;
    private Animator animator;
    private Camera cam;

    public GameObject clickPointPrefab;
    public GameObject clickPoint2Prefab;
    public GameObject RangePrefab;

    public GameObject projectile;
    public Transform projectileTransform;

    public Transform enemyes;

    public float range = 120;
    public int autosec = 2;

    Vector3 oldPosition;
    float currentTimeToAA;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        cam = Camera.main;

        // IMPORTANTE
        agent.updateRotation = false;
    }

    void Update()
    {
        currentTimeToAA += UnityEngine.Time.deltaTime;

        HandleRightClick();
        HandleLeftClick();
        HandleRotation();
        HandleAnimation();
    }

    void HandleRightClick()
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // CLICK EN ENEMIGO
            if (hit.collider.CompareTag("Enemy"))
            {
                foreach (Transform enemy in enemyes)
                {
                    enemy.GetComponent<EnemyBase>().selected = false;
                }

                hit.collider.GetComponent<EnemyBase>().selected = true;

                Vector3 direction =
                    (hit.collider.transform.position - transform.position).normalized;

                InstantiateProjectile(direction);
            }
            else
            {
                // MOVER
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

    void HandleLeftClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (currentTimeToAA > 0.5f)
            {
                Transform closestEnemy = null;
                float oldDistance = Mathf.Infinity;

                // ENEMIGO MÁS CERCANO AL CLICK
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
                    foreach (Transform enemy in enemyes)
                    {
                        enemy.GetComponent<EnemyBase>().selected = false;
                    }

                    closestEnemy.GetComponent<EnemyBase>().selected = true;

                    Vector3 direction =
                        (closestEnemy.position - transform.position).normalized;

                    InstantiateProjectile(direction);

                    currentTimeToAA = 0;
                }
            }

            Instantiate(
                clickPoint2Prefab,
                hit.point + Vector3.up * 0.03f,
                Quaternion.Euler(90f, 0f, 0f)
            );
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Instantiate(
                RangePrefab,
                transform.position + Vector3.up * 0.03f,
                Quaternion.Euler(90f, 0f, 0f)
            );
        }
    }

    void HandleRotation()
    {
        // ROTACIÓN SUAVE SEGÚN MOVIMIENTO
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion lookRot =
                Quaternion.LookRotation(agent.velocity.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                10f * UnityEngine.Time.deltaTime
            );
        }
    }

    void HandleAnimation()
    {
        if (Vector3.Distance(oldPosition, transform.position) > 0.001f)
            animator.SetFloat("speed", 1);
        else
            animator.SetFloat("speed", 0);

        oldPosition = transform.position;
    }

    private void InstantiateProjectile(Vector3 direction)
    {
        var projectileObj = Instantiate(
            projectile,
            SpawnBulletPos.position,
            Quaternion.identity,
            projectileTransform
        );

        Projectile p = projectileObj.GetComponent<Projectile>();

        if (p == null)
        {
            Debug.Log("El componente Projectile no funciona");
            return;
        }

        p.SetDirection(direction.normalized);
    }
}