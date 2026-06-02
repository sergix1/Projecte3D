using UnityEngine;
using UnityEngine.AI;

public class TeleportArea : MonoBehaviour
{
    public int maxAttempts = 30;
    public float navMeshSearchDistance = 3f;

    private BoxCollider box;

    void Awake()
    {
        box = GetComponent<BoxCollider>();

        if (box != null)
            box.isTrigger = true;
    }

    public bool GetRandomPoint(out Vector3 point)
    {
        if (box == null)
            box = GetComponent<BoxCollider>();

        if (box == null)
        {
            point = transform.position;
            return false;
        }

        Bounds bounds = box.bounds;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, navMeshSearchDistance, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }

        point = transform.position;
        return false;
    }

  
}
