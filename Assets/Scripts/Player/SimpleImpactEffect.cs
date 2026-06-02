using UnityEngine;

public class SimpleImpactEffect : MonoBehaviour
{
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

    [Header("Color")]
    public Color effectColor = Color.red;

    [Header("Particles")]
    public Material particleMaterial;
    public float particleLifetime = 0.25f;
    public float particleSize = 0.12f;
    public float particleSpeed = 3f;
    public short particleAmount = 14;
    public float particleRadius = 0.08f;

    [Header("Light")]
    public bool useLight = true;
    public float lightIntensity = 2f;
    public float lightRange = 1.5f;

    private ParticleSystem particles;
    private ParticleSystemRenderer particleRenderer;
    private Light impactLight;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        CreateParticles();
        CreateLight();
    }

    private void Start()
    {
        ApplyColor();
        particles.Play();
    }

    public void SetColor(Color newColor)
    {
        effectColor = newColor;
        ApplyColor();
    }

    private void CreateParticles()
    {
        particles = gameObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startSize = particleSize;
        main.loop = false;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, particleAmount) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = particleRadius;

        SetParticleMaterial();
    }

    private void CreateLight()
    {
        if (!useLight)
            return;

        impactLight = gameObject.AddComponent<Light>();
        impactLight.range = lightRange;
        impactLight.intensity = lightIntensity;
    }

    private void ApplyColor()
    {
        if (particles != null)
        {
            ParticleSystem.MainModule main = particles.main;
            main.startColor = effectColor;
        }

        if (particleRenderer != null)
        {
            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            propertyBlock.SetColor(BaseColorProperty, effectColor);
            propertyBlock.SetColor(ColorProperty, effectColor);
            propertyBlock.SetColor(EmissionColorProperty, effectColor);
            particleRenderer.SetPropertyBlock(propertyBlock);
        }

        if (impactLight != null)
            impactLight.color = effectColor;
    }

    private void SetParticleMaterial()
    {
        particleRenderer = GetComponent<ParticleSystemRenderer>();

        if (particleRenderer == null)
            return;

        if (particleMaterial != null)
            particleRenderer.sharedMaterial = particleMaterial;
    }
}
