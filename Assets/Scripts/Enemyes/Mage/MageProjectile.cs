using UnityEngine;

public class MageProjectile : MonoBehaviour
{
    private const int RaycastBufferSize = 8;
    private const string EnemyTag = "Enemy";
    private const string MageProjectileTag = "MageProjectile";

    public float speed = 8f;
    public float lifeTime = 4f;
    public int damage = 10;

    private Vector3 direction;
    private bool hasImpacted;
    private Collider[] ownColliders;
    private static readonly RaycastHit[] hits = new RaycastHit[RaycastBufferSize];

    private void Awake()
    {
        ownColliders = GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (direction == Vector3.zero || hasImpacted)
            return;

        Vector3 movement = direction * speed * Time.fixedDeltaTime;

        if (movement.sqrMagnitude <= 0f)
            return;

        int hitCount = Physics.RaycastNonAlloc(
            transform.position,
            direction,
            hits,
            movement.magnitude,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );

        if (TryGetFirstImpact(hitCount, out RaycastHit firstHit))
        {
            HandleImpact(firstHit.collider);
            return;
        }

        transform.position += movement;
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanImpact(other))
            return;

        HandleImpact(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!CanImpact(collision.collider))
            return;

        HandleImpact(collision.collider);
    }

    private bool CanImpact(Collider other)
    {
        if (hasImpacted || other == null)
            return false;

        if (IsOwnCollider(other))
            return false;

        if (other.GetComponentInParent<PlayerHealth>() != null)
            return true;

        if (other.CompareTag(EnemyTag) || other.CompareTag(MageProjectileTag))
            return false;

        return !other.isTrigger;
    }

    private bool IsOwnCollider(Collider other)
    {
        if (ownColliders == null)
            return false;

        foreach (Collider ownCollider in ownColliders)
        {
            if (ownCollider == other)
                return true;
        }

        return false;
    }

    private bool TryGetFirstImpact(int hitCount, out RaycastHit firstHit)
    {
        firstHit = default;
        float closestDistance = Mathf.Infinity;
        bool foundHit = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];

            if (!CanImpact(hit.collider))
                continue;

            if (hit.distance >= closestDistance)
                continue;

            closestDistance = hit.distance;
            firstHit = hit;
            foundHit = true;
        }

        return foundHit;
    }

    private void HandleImpact(Collider other)
    {
        hasImpacted = true;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.TakeDamage(damage);

        Destroy(gameObject);
    }
}
