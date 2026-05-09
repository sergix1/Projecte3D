
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light pointLight;
    public float minIntensity =4f;
    public float maxIntensity = 8f;

    void Update()
    {
        pointLight.intensity = Random.Range(minIntensity, maxIntensity);
    }
}