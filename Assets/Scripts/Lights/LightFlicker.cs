
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light pointLight;
    public float minIntensity = 4f;
    public float maxIntensity = 8f;
    public float updatesPerSecond = 8f;

    private float nextUpdateTime;

    private void Awake()
    {
        if (pointLight == null)
            pointLight = GetComponent<Light>();

        float updateInterval = 1f / Mathf.Max(1f, updatesPerSecond);
        nextUpdateTime = Time.time + Random.Range(0f, updateInterval);
    }

    private void Update()
    {
        if (pointLight == null || Time.time < nextUpdateTime)
            return;

        nextUpdateTime = Time.time + 1f / Mathf.Max(1f, updatesPerSecond);
        pointLight.intensity = Random.Range(minIntensity, maxIntensity);
    }
}
