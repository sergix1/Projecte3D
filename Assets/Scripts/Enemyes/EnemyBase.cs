using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent navMesh;
    [Header("Barra de Vida")]
    public Image healthFill;
    public GameObject healthBar;

    private int currentLife;
    [Header("Drops")]
    public GameObject healthPotionPrefab;
    [Range(0f, 1f)]
    public float healthPotionDropChance = 0.25f;
    public GameObject soul;
    public int qsoul = 1;
    public Transform dropSpawnPoint;
    public float dropHeight = 0.75f;

    [Header("Vida")]
    public int maxLife = 100;

    [Header("UI")]
    public TextMeshPro textMeshPro;
    public GameObject SelectedObj;
    public bool selected;

    private bool lastSelectedState;

    [Header("NavMesh")]
    public bool updatePathToPlayer = true;
    public float pathUpdateInterval = 0.7f;
    public float minDistanceToUpdatePath = 2.5f;
    public float chaseRange = 35f;

    private float nextPathUpdate;

    [Header("Movimiento / Ataque")]
    public float stopDistance = 1.4f;

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
        UpdateHealthBar();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        navMesh = GetComponent<NavMeshAgent>();

        nextPathUpdate = Time.time + Random.Range(0f, pathUpdateInterval);

        if (SelectedObj != null)
        {
            SelectedObj.SetActive(selected);
            lastSelectedState = selected;
        }

    }

    void Update()
    {
        UpdateSelectionVisual();
        UpdatePath();
    }

    private void UpdatePath()
    {
        if (!updatePathToPlayer)
            return;

        if (player == null || navMesh == null || !navMesh.enabled || !navMesh.isOnNavMesh)
            return;

        if (Time.time < nextPathUpdate)
            return;

        nextPathUpdate = Time.time + pathUpdateInterval;

        Vector3 playerPosition = player.position;
        float sqrDistanceToPlayer = (transform.position - playerPosition).sqrMagnitude;

        if (sqrDistanceToPlayer > chaseRange * chaseRange)
        {
            if (!navMesh.isStopped)
                navMesh.isStopped = true;

            return;
        }

        if (navMesh.isStopped)
            navMesh.isStopped = false;

        if (navMesh.hasPath && (navMesh.destination - playerPosition).sqrMagnitude < minDistanceToUpdatePath * minDistanceToUpdatePath)
            return;

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
    private void UpdateHealthBar()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentLife / maxLife;

        if (healthBar != null)
            healthBar.SetActive(true); ;
    }
    public void RestLife(int dmg)
    {
        currentLife -= dmg;
        UpdateHealthBar();

        if (currentLife <= 0)
        {
            Die();
        }
    }

    private Vector3 GetDropPosition()
    {
        if (dropSpawnPoint != null)
            return dropSpawnPoint.position + Vector3.up * dropHeight;

        Collider enemyCollider = GetComponent<Collider>();

        if (enemyCollider != null)
        {
            Vector3 position = enemyCollider.bounds.center;
            position.y = enemyCollider.bounds.min.y + dropHeight;
            return position;
        }

        return transform.position + Vector3.up * dropHeight;
    }

    private void Die()
    {
        if (soul != null && qsoul > 0)
        {
            GameObject soulInstance = Instantiate(soul, GetDropPosition(), Quaternion.identity);

            SoulPickup soulPickup = soulInstance.GetComponent<SoulPickup>();

            if (soulPickup != null)
            {
                soulPickup.SetSoulAmount(qsoul);
            }
        }

        if (healthPotionPrefab != null && Random.value <= healthPotionDropChance)
        {
            Instantiate(healthPotionPrefab, GetDropPosition(), Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
