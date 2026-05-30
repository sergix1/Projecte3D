using UnityEngine;

public class MageLevitation : MonoBehaviour
{
    public float height = 0.15f;
    public float speed = 2f;
    public float rotationAmount = 2f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float movement = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f * height;

        transform.localPosition = startPosition + new Vector3(0f, movement, 0f);

        float rotation = Mathf.Sin(Time.time * speed) * rotationAmount;
        transform.localRotation = startRotation * Quaternion.Euler(0f, 0f, rotation);
    }
}