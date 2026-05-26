using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 20f;
    public GameObject impactEffectPrefab;
    public float impactEffectLifetime = 1f;
    public Color enemyImpactColor = new Color(0.75f, 0.05f, 0.05f, 1f);
    public Color surfaceImpactColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    private Vector3 direction;
    private float damage = 30f;
    private float currentTime;
    private bool hasImpacted;
    private Collider[] ownColliders;

    private void Awake()
    {
        ownColliders = GetComponentsInChildren<Collider>();
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

        RaycastHit[] hits = Physics.RaycastAll(
            transform.position,
            direction,
            movement.magnitude,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );

        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            if (!CanImpact(hit.collider))
                continue;

            HandleImpact(hit.collider, hit.point);
            return;
        }

        transform.position += movement;
    }

    void Update()
    {
        currentTime += UnityEngine.Time.deltaTime;
        if (currentTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanImpact(other))
            return;

        HandleImpact(other, GetImpactPoint(other));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!CanImpact(collision.collider))
            return;

        Vector3 impactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : GetImpactPoint(collision.collider);

        HandleImpact(collision.collider, impactPoint);
    }

    private bool CanImpact(Collider other)
    {
        if (hasImpacted || other == null)
            return false;

        if (IsOwnCollider(other))
            return false;

        if (other.CompareTag("Player") || other.CompareTag("Projectile"))
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

    private void HandleImpact(Collider other, Vector3 impactPoint)
    {
        hasImpacted = true;

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        bool hitEnemy = enemy != null;

        if (hitEnemy)
            enemy.restLife((int)damage);

        SpawnImpactEffect(impactPoint, hitEnemy);
        Destroy(gameObject);
    }

    private Vector3 GetImpactPoint(Collider other)
    {
        Vector3 point = other.ClosestPoint(transform.position);

        if (point == transform.position)
            point = transform.position - direction.normalized * 0.2f;

        return point;
    }

    private void SpawnImpactEffect(Vector3 position, bool hitEnemy)
    {
        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, position, Quaternion.identity);
            Destroy(effect, impactEffectLifetime);
            return;
        }

        Color impactColor = hitEnemy ? enemyImpactColor : surfaceImpactColor;
        Color glowColor = Color.Lerp(impactColor, Color.white, 0.3f);

        GameObject effectObject = new GameObject("Projectile Impact");
        effectObject.transform.position = position;

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystemRenderer particleRenderer = effectObject.GetComponent<ParticleSystemRenderer>();
        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.12f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.35f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            impactColor,
            Color.Lerp(impactColor, Color.black, 0.35f)
        );
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 18)
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = 0.04f;
        shape.angle = 25f;

        Shader particleShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (particleShader == null)
            particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null)
            particleShader = Shader.Find("Sprites/Default");

        if (particleShader != null && particleRenderer != null)
        {
            Material particleMaterial = new Material(particleShader);
            particleRenderer.material = particleMaterial;
        }

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(glowColor, 0f),
                new GradientColorKey(impactColor, 0.45f),
                new GradientColorKey(Color.black, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        Light flash = effectObject.AddComponent<Light>();
        flash.type = LightType.Point;
        flash.color = glowColor;
        flash.intensity = hitEnemy ? 2.6f : 1.6f;
        flash.range = 1.2f;
        flash.shadows = LightShadows.None;

        particles.Play();
        Destroy(effectObject, impactEffectLifetime);
    }
}
