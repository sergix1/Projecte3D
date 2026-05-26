using UnityEngine;
using UnityEngine.AI;

public static class RuntimeCollisionBuilder
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureSceneMeshColliders()
    {
        MeshFilter[] meshFilters = Object.FindObjectsByType<MeshFilter>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
                continue;

            if (meshFilter.GetComponentInParent<NavMeshAgent>() != null)
                continue;

            if (meshFilter.GetComponentInParent<BackgroundTreeRing>() != null)
                continue;

            if (IsTreeObject(meshFilter.transform))
                continue;

            if (!meshFilter.TryGetComponent(out MeshRenderer renderer) || !renderer.enabled)
                continue;

            if (meshFilter.TryGetComponent(out Collider _))
                continue;

            TryAddMeshCollider(meshFilter);
        }
    }

    private static void TryAddMeshCollider(MeshFilter meshFilter)
    {
        MeshCollider collider = null;

        try
        {
            collider = meshFilter.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = meshFilter.sharedMesh;
            collider.convex = false;
        }
        catch (System.Exception)
        {
            if (collider != null)
                Object.Destroy(collider);
        }
    }

    private static bool IsTreeObject(Transform transform)
    {
        while (transform != null)
        {
            if (transform.name.ToLowerInvariant().Contains("arbol"))
                return true;

            transform = transform.parent;
        }

        return false;
    }
}
