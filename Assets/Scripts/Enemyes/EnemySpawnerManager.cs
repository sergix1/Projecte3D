using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject magePrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] zombieSpawnPoints;
    [SerializeField] private Transform[] mageSpawnPoints;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Limits")]
    [SerializeField] private int maxZombies = 20    ;
    [SerializeField] private int maxMages = 2;

    [Header("Respawn")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float minDistanceFromPlayer = 8f;
    [SerializeField] private float navMeshSearchRadius = 2f;

    private readonly List<GameObject> currentZombies = new List<GameObject>();
    private readonly List<GameObject> currentMages = new List<GameObject>();

    public Transform instanceZombies;

    private float timer;

    private void Start()
    {
        SpawnUntilLimit();
    }

    private void Update()
    {
        CleanLists();

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnUntilLimit();
        }
    }

    private void SpawnUntilLimit()
    {
        if (currentZombies.Count < maxZombies)
        {
            TrySpawnEnemy(zombiePrefab, zombieSpawnPoints, currentZombies);
        }

        if (currentMages.Count < maxMages)
        {
            TrySpawnEnemy(magePrefab, mageSpawnPoints, currentMages);
        }
    }

    private void TrySpawnEnemy(GameObject prefab, Transform[] spawnPoints, List<GameObject> list)
    {
        if (prefab == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        Transform spawnPoint = GetValidSpawnPoint(spawnPoints);

        if (spawnPoint == null)
            return;

        Vector3 spawnPosition = spawnPoint.position;

        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            spawnPosition = hit.position;
        }

        GameObject enemy = Instantiate(prefab, spawnPosition, spawnPoint.rotation,instanceZombies);

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.Warp(spawnPosition);
        }

        list.Add(enemy);
    }

    private Transform GetValidSpawnPoint(Transform[] spawnPoints)
    {
        for (int i = 0; i < 10; i++)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            if (player == null)
                return randomPoint;

            float distanceToPlayer = Vector3.Distance(randomPoint.position, player.position);

            if (distanceToPlayer >= minDistanceFromPlayer)
                return randomPoint;
        }

        return null;
    }

    private void CleanLists()
    {
        currentZombies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
        currentMages.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
    }
}