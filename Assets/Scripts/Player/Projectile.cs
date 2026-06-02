using UnityEngine;

public class Projectile : MonoBehaviour
{
    private const int RaycastBufferSize = 8;
    private const string PlayerTag = "Player";
    private const string ProjectileTag = "Projectile";

    public float speed = 20f;
    public float lifeTime = 20f;
    public GameObject impactEffectPrefab;
    public float impactEffectLifetime = 1f;
    public Color enemyImpactColor = Color.red;
    public Color wallImpactColor = Color.gray;
    public float fallbackImpactOffset = 0.2f;

    private Vector3 direction;
    private float damage = 30f;
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

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetDirection(Vector3 direction)
    {
        this.direction = direction.normalized;
    }

    private void FixedUpdate()
    {
        Vector3 movement = direction * speed * UnityEngine.Time.fixedDeltaTime;

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
            HandleImpact(firstHit.collider, firstHit.point);
            return;
        }

        transform.position += movement;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanImpact(other))
            return;

        HandleImpact(other, GetImpactPoint(other));
    }

  /*  private void OnCollisionEnter(Collision collision)
    {
        if (!CanImpact(collision.collider))
            return;

        Vector3 impactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : GetImpactPoint(collision.collider);

        HandleImpact(collision.collider, impactPoint);
    }*/

    private bool CanImpact(Collider other)
    {
        if (hasImpacted || other == null)
            return false;

        if (IsOwnCollider(other))
            return false;

        if (other.CompareTag(PlayerTag) || other.CompareTag(ProjectileTag))
            return false;

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
            return true;

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

    private void HandleImpact(Collider other, Vector3 impactPoint)
    {
        hasImpacted = true;

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        bool hitEnemy = enemy != null;

        if (hitEnemy)
            enemy.RestLife((int)damage);

        SpawnImpactEffect(impactPoint, hitEnemy);
        Destroy(gameObject);
    }

    private Vector3 GetImpactPoint(Collider other)
    {
        Vector3 point = other.ClosestPoint(transform.position);

        if (point == transform.position)
            point = transform.position - direction.normalized * fallbackImpactOffset;

        return point;
    }

    private void SpawnImpactEffect(Vector3 position, bool hitEnemy)
    {
        if (impactEffectPrefab == null)
            return;

        GameObject effect = Instantiate(impactEffectPrefab, position, Quaternion.identity);
        SimpleImpactEffect simpleImpactEffect = effect.GetComponent<SimpleImpactEffect>();

        if (simpleImpactEffect != null)
            simpleImpactEffect.SetColor(hitEnemy ? enemyImpactColor : wallImpactColor);

        Destroy(effect, impactEffectLifetime);
    }
}
