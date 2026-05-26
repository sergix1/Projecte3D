using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    private GameObject player;
    private NavMeshAgent navMesh;
    private int currentlife;
    public int maxLife=100;
    public TextMeshPro textMeshPro;
    public GameObject SelectedObj;
    public bool selected;
    public GameObject soul;
    public int qsoul = 1;
    public float pathUpdateInterval = 0.25f;
    private float nextPathUpdate;
   // public bool SetSelected(bool )
    public int GetCurrentLife()
    {
        return currentlife;
    }
    public int MaxLife()
    {
        return maxLife;
    }

    void Start()
    {
        selected = false;
        currentlife = maxLife;
        player = GameObject.FindGameObjectWithTag("Player");
        navMesh = GetComponent<NavMeshAgent>();
    }  
    void Update()
    {
        if (player != null && navMesh != null && UnityEngine.Time.time >= nextPathUpdate)
        {
            nextPathUpdate = UnityEngine.Time.time + pathUpdateInterval;
            navMesh.SetDestination(player.transform.position);
        }

        if (selected) SelectedObj.SetActive(true);
        else SelectedObj.SetActive(false);
    }
    public void restLife(int dmg)
    {
        currentlife -= dmg;
        if(currentlife<=0)
        {
            GameObject soulInstance = Instantiate(soul, transform.position, Quaternion.identity);
            SoulPickup soulPickup = soulInstance.GetComponent<SoulPickup>();
            if (soulPickup != null)
            {
                soulPickup.SetSoulAmount(Mathf.Max(1, qsoul));
            }
            Destroy(this.gameObject);
        }
    }
}
