using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    private GameObject player;
    private NavMeshAgent navMesh;
    private int currentlife;
    private int maxLife;
    public TextMeshPro textMeshPro;
    public GameObject SelectedObj;
    public bool selected;
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
        maxLife = 100;
        currentlife = maxLife;
        player = GameObject.FindGameObjectWithTag("Player");
        navMesh = GetComponent<NavMeshAgent>();
    }  
    void Update()
    {
        navMesh.SetDestination(player.transform.position);
        if (selected) SelectedObj.SetActive(true);
        else SelectedObj.SetActive(false);
    }
    public void restLife(int dmg)
    {
        currentlife -= dmg;
        if(currentlife<=0)
        {
            Destroy(this.gameObject);
        }
    }
    
    public void OnTriggerEnter(Collider other)
    {
        
        if(other.CompareTag("Projectile"))
        {
            Debug.Log(other.gameObject);
        }
    }
    
}
