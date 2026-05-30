using UnityEngine;

public class MageProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 4f;
    public int damage = 10;

    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (direction == Vector3.zero)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;

        Debug.Log("Direccion proyectil: " + direction);

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.SendMessage("RestLife", damage, SendMessageOptions.DontRequireReceiver);
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("Enemy") && !other.CompareTag("MageProjectile"))
        {
            Destroy(gameObject);
        }
    }
}