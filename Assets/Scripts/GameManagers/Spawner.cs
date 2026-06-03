using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform enemyesTransform;

    public float timeToSpawn = 6f;
    float currentTime;

    void Update()
    {
        currentTime += UnityEngine.Time.deltaTime;
        if (currentTime > timeToSpawn)
        {
            SpawnEnemy();
            currentTime = 0;
        }
    }

    void SpawnEnemy()
    {
        var z = Random.Range(-14, 13);
        var x = 3;
        Instantiate(enemyPrefab, new Vector3(x, -1.82f, z), Quaternion.identity, enemyesTransform);
    }
}
