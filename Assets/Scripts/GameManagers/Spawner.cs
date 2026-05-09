using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform enemyesTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    float timeToSpawn = 3f;
    float currentTime;
    // Update is called once per frame
    void Update()
    {
        currentTime += UnityEngine.Time.deltaTime;
        if (currentTime > timeToSpawn)
        {
            Invoke("SpawnEnemy", 0);
            currentTime = 0;
        }
    }
    void SpawnEnemy()
    {
        var z=Random.Range(-14,13 );
        var x = 3;
        Instantiate(enemyPrefab,new Vector3(x,-1.82f,z),Quaternion.identity,enemyesTransform);
    }
}
