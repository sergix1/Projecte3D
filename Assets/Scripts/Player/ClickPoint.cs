using UnityEngine;



public class GroundClickMarker : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public float lifeTime = 0.3f;
    public float startScale = 0.6f;

    private float timer;
    private Renderer markerRenderer;
    private Material markerMaterial;
    private int colorPropertyId;
    private bool hasColorProperty;
    private Color startColor;

    public void SetStartScale(float scale)
    {
        startScale = scale;
        transform.localScale = Vector3.one * startScale;
    }

    void Start()
    {
        transform.localScale = Vector3.one * startScale;

        markerRenderer = GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            markerMaterial = markerRenderer.material;
            colorPropertyId = GetColorPropertyId(markerMaterial);
            hasColorProperty = colorPropertyId != 0;

            startColor = hasColorProperty ? markerMaterial.GetColor(colorPropertyId) : Color.white;
            startColor.a = 0.9f;

            if (hasColorProperty)
                markerMaterial.SetColor(colorPropertyId, startColor);
        }
    }

    void Update()
    {
        timer += UnityEngine.Time.deltaTime;
        float t = timer / lifeTime;


        float scale = Mathf.Lerp(startScale, 0f, Mathf.SmoothStep(0, 1, t));
        transform.localScale = new Vector3(scale, scale, scale);

        if (hasColorProperty && markerMaterial != null)
        {
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, t);
            markerMaterial.SetColor(colorPropertyId, color);
        }

        if (t >= 1f)
            Destroy(gameObject);
    }

    private int GetColorPropertyId(Material material)
    {
        if (material == null)
            return 0;

        if (material.HasProperty(ColorId))
            return ColorId;

        if (material.HasProperty(BaseColorId))
            return BaseColorId;

        return 0;
    }
}
