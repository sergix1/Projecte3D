using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Vector3 direction;
    public float speed;
    private float damage;
    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
    private float currentY;
    public void SetDirection(Vector3 direction)
    {
        this.direction.y = direction.y;
        this.direction = direction;
    }

    void Start()
    {
        damage = 30f;
       // Debug.Log("Direccion : " + direction);
    }
    private void FixedUpdate()
    {
        //NormalWay;
        this.transform.position +=new Vector3( this.direction.x * speed * UnityEngine.Time.fixedDeltaTime,currentY, this.direction.z * speed * UnityEngine.Time.fixedDeltaTime);
    }
    float currentTime;
    // Update is called once per frame
    void Update()
    {
        currentTime += UnityEngine.Time.deltaTime;
        if (currentTime > 20f)
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyBase>().restLife((int)damage);
            Destroy(this.gameObject);
        }
    }

}
