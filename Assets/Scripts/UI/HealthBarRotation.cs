using UnityEngine;

public class HealthBarRotation : MonoBehaviour
{
    public Vector3 fixedRotation = new Vector3(0f, 0f, 0f);

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}
