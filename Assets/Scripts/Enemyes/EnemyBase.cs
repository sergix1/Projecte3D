using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent navMesh;

    private int currentLife;

    [Header("Vida")]
    public int maxLife = 100;

    [Header("UI")]
    public TextMeshPro textMeshPro;
    public GameObject SelectedObj;
    public bool selected;

    private bool lastSelectedState;

    [Header("Soul")]
    public GameObject soul;
    public int qsoul = 1;

    [Header("NavMesh Optimizado")]
    public float pathUpdateInterval = 0.4f;
    public float minDistanceToUpdatePath = 1.5f;
    public float chaseRange = 35f;

    private float nextPathUpdate;
    private Vector3 lastTargetPosition;

    public int GetCurrentLife()
    {
        return currentLife;
    }

    public int MaxLife()
    {
        return maxLife;
    }

    void Start()
    {
        currentLife = maxLife;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        navMesh = GetComponent<NavMeshAgent>();

        // Para que no todos los enemigos calculen ruta en el mismo frame
        nextPathUpdate = Time.time + Random.Range(0f, pathUpdateInterval);

        if (SelectedObj != null)
        {
            SelectedObj.SetActive(selected);
            lastSelectedState = selected;
        }

        lastTargetPosition = Vector3.positiveInfinity;
    }

    void Update()
    {
        UpdateSelectionVisual();
        UpdatePath();
    }

    private void UpdatePath()
    {
        if (player == null || navMesh == null || !navMesh.enabled || !navMesh.isOnNavMesh)
            return;

        if (Time.time < nextPathUpdate)
            return;

        nextPathUpdate = Time.time + pathUpdateInterval;

        Vector3 playerPosition = player.position;

        float sqrDistanceToPlayer = (playerPosition - transform.position).sqrMagnitude;
        float sqrChaseRange = chaseRange * chaseRange;

        // Si está muy lejos, no calcules path
        if (sqrDistanceToPlayer > sqrChaseRange)
        {
            if (!navMesh.isStopped)
                navMesh.isStopped = true;

            return;
        }

        if (navMesh.isStopped)
            navMesh.isStopped = false;

        // Si el player casi no se ha movido, no recalcules ruta
        float sqrMovedDistance = (playerPosition - lastTargetPosition).sqrMagnitude;
        float sqrMinDistance = minDistanceToUpdatePath * minDistanceToUpdatePath;

        if (sqrMovedDistance < sqrMinDistance)
            return;

        lastTargetPosition = playerPosition;
        navMesh.SetDestination(playerPosition);
    }

    private void UpdateSelectionVisual()
    {
        if (SelectedObj == null)
            return;

        if (selected == lastSelectedState)
            return;

        SelectedObj.SetActive(selected);
        lastSelectedState = selected;
    }

    public void RestLife(int dmg)
    {
        currentLife -= dmg;

        if (currentLife <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (soul != null)
        {
            GameObject soulInstance = Instantiate(soul, transform.position, Quaternion.identity);

            SoulPickup soulPickup = soulInstance.GetComponent<SoulPickup>();

            if (soulPickup != null)
            {
                soulPickup.SetSoulAmount(Mathf.Max(1, qsoul));
            }
        }

        Destroy(gameObject);
    }
}