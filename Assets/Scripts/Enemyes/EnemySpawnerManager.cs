using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject zombiePrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] zombieSpawnPoints;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Normal")]
    [SerializeField] private int maxZombies = 20;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int zombiesPerSpawn = 1;

    [Header("Maldición+")]
    public static bool gamePlusActive;

    [SerializeField] private int gamePlusMaxZombies = 35;
    [SerializeField] private float gamePlusSpawnInterval = 2f;
    [SerializeField] private int gamePlusZombiesPerSpawn = 3;

    [Header("Spawn Settings")]
    [SerializeField] private float minDistanceFromPlayer = 8f;
    [SerializeField] private float navMeshSearchRadius = 2f;
    [SerializeField] private int spawnPointAttempts = 10;
    [SerializeField] private float cleanListInterval = 1f;

    [Header("Organización")]
    public Transform instanceZombies;

    private readonly List<GameObject> currentZombies = new List<GameObject>();

    private int activeMaxZombies;
    private float activeSpawnInterval;
    private int activeZombiesPerSpawn;

    private float timer;
    private float nextCleanTime;

    private void Start()
    {
        Time.timeScale = 1f;

        ApplyDifficulty();
        SpawnUntilLimit();
    }

    private void Update()
    {
        if (Time.time >= nextCleanTime)
        {
            nextCleanTime = Time.time + cleanListInterval;
            CleanList();
        }

        timer += Time.deltaTime;

        if (timer >= activeSpawnInterval)
        {
            timer = 0f;
            SpawnUntilLimit();
        }
    }

    private void ApplyDifficulty()
    {
        if (gamePlusActive)
        {
            activeMaxZombies = gamePlusMaxZombies;
            activeSpawnInterval = gamePlusSpawnInterval;
            activeZombiesPerSpawn = gamePlusZombiesPerSpawn;
        }
        else
        {
            activeMaxZombies = maxZombies;
            activeSpawnInterval = spawnInterval;
            activeZombiesPerSpawn = zombiesPerSpawn;
        }
    }

    private void SpawnUntilLimit()
    {
        for (int i = 0; i < activeZombiesPerSpawn; i++)
        {
            if (currentZombies.Count >= activeMaxZombies)
                return;

            TrySpawnZombie();
        }
    }

    private void TrySpawnZombie()
    {
        if (zombiePrefab == null || zombieSpawnPoints == null || zombieSpawnPoints.Length == 0)
            return;

        Transform spawnPoint = GetValidSpawnPoint();

        if (spawnPoint == null)
            return;

        Vector3 spawnPosition = spawnPoint.position;

        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
            spawnPosition = hit.position;

        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, spawnPoint.rotation, instanceZombies);

        NavMeshAgent agent = zombie.GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.Warp(spawnPosition);

        currentZombies.Add(zombie);
    }

    private Transform GetValidSpawnPoint()
    {
        for (int i = 0; i < spawnPointAttempts; i++)
        {
            Transform randomPoint = zombieSpawnPoints[Random.Range(0, zombieSpawnPoints.Length)];

            if (player == null)
                return randomPoint;

            float distanceToPlayer = (randomPoint.position - player.position).sqrMagnitude;
            float minDistance = minDistanceFromPlayer * minDistanceFromPlayer;

            if (distanceToPlayer >= minDistance)
                return randomPoint;
        }

        return null;
    }

    private void CleanList()
    {
        currentZombies.RemoveAll(zombie => zombie == null || !zombie.activeInHierarchy);
    }

    public void StartGamePlus()
    {
        gamePlusActive = true;
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void RestartNormalGame()
    {
        gamePlusActive = false;
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

    }
}