using UnityEngine;

public sealed class BackgroundTreeRing : MonoBehaviour
{
    private const string GeneratedTreePrefix = "arbol fondo generado";

    [SerializeField] private Mesh treeMesh;
    [SerializeField] private Material treeMaterial;

    private static readonly TreePlacement[] Placements =
    {
        new(-27.8f, -2.45f, -23.5f, 0.62f, 18f),
        new(-22.4f, -2.45f, -27.8f, 0.70f, 61f),
        new(-17.1f, -2.45f, -31.4f, 0.58f, 119f),
        new(-11.6f, -2.45f, -33.2f, 0.76f, 203f),
        new(-6.2f, -2.45f, -34.1f, 0.64f, 284f),
        new(-0.8f, -2.45f, -35.4f, 0.72f, 337f),
        new(4.8f, -2.45f, -34.2f, 0.59f, 42f),
        new(10.2f, -2.45f, -32.7f, 0.74f, 96f),
        new(15.7f, -2.45f, -30.9f, 0.63f, 148f),
        new(21.1f, -2.45f, -27.6f, 0.71f, 231f),
        new(26.4f, -2.45f, -23.9f, 0.56f, 309f),
        new(30.2f, -2.45f, -18.2f, 0.69f, 12f),
        new(31.6f, -2.45f, -12.3f, 0.77f, 74f),
        new(30.4f, -2.45f, -6.2f, 0.61f, 134f),
        new(31.8f, -2.45f, -0.4f, 0.73f, 188f),
        new(29.7f, -2.45f, 5.7f, 0.57f, 258f),
        new(31.2f, -2.45f, 11.9f, 0.68f, 322f),
        new(30.1f, -2.45f, 18.3f, 0.75f, 37f),
        new(28.4f, -2.45f, 24.5f, 0.62f, 103f),
        new(30.8f, -2.45f, 30.9f, 0.70f, 166f),
        new(27.6f, -2.45f, 36.8f, 0.79f, 247f),
        new(22.1f, -2.45f, 39.5f, 0.60f, 301f),
        new(16.4f, -2.45f, 41.8f, 0.72f, 24f),
        new(10.7f, -2.45f, 43.1f, 0.65f, 83f),
        new(5.1f, -2.45f, 42.2f, 0.78f, 156f),
        new(-0.6f, -2.45f, 43.7f, 0.61f, 215f),
        new(-6.3f, -2.45f, 42.6f, 0.69f, 289f),
        new(-11.8f, -2.45f, 41.1f, 0.57f, 351f),
        new(-17.5f, -2.45f, 39.2f, 0.76f, 47f),
        new(-23.2f, -2.45f, 36.4f, 0.64f, 112f),
        new(-28.9f, -2.45f, 32.1f, 0.73f, 174f),
        new(-31.4f, -2.45f, 26.2f, 0.58f, 236f),
        new(-30.2f, -2.45f, 20.3f, 0.71f, 318f),
        new(-32.1f, -2.45f, 14.7f, 0.63f, 5f),
        new(-29.8f, -2.45f, 8.6f, 0.75f, 69f),
        new(-31.7f, -2.45f, 2.4f, 0.60f, 128f),
        new(-30.4f, -2.45f, -3.7f, 0.68f, 196f),
        new(-32.3f, -2.45f, -9.6f, 0.77f, 271f),
        new(-29.5f, -2.45f, -15.4f, 0.62f, 333f),
        new(-24.6f, -2.45f, 34.8f, 0.56f, 33f),
        new(-19.8f, -2.45f, 37.6f, 0.66f, 91f),
        new(-14.9f, -2.45f, 40.4f, 0.74f, 143f),
        new(-9.5f, -2.45f, 38.6f, 0.59f, 207f),
        new(-3.9f, -2.45f, 40.8f, 0.70f, 263f),
        new(1.9f, -2.45f, 39.1f, 0.63f, 326f),
        new(7.4f, -2.45f, 41.3f, 0.76f, 54f),
        new(13.1f, -2.45f, 38.7f, 0.61f, 117f),
        new(18.6f, -2.45f, 36.9f, 0.69f, 181f),
        new(24.3f, -2.45f, 33.4f, 0.58f, 241f),
        new(25.9f, -2.45f, -15.1f, 0.72f, 306f),
        new(27.1f, -2.45f, -8.8f, 0.64f, 21f),
        new(25.4f, -2.45f, -2.6f, 0.78f, 88f),
        new(27.8f, -2.45f, 3.2f, 0.60f, 151f),
        new(26.2f, -2.45f, 9.6f, 0.70f, 222f),
        new(27.5f, -2.45f, 15.8f, 0.57f, 279f),
        new(25.8f, -2.45f, 22.1f, 0.73f, 346f),
        new(27.9f, -2.45f, 28.4f, 0.62f, 64f),
        new(-25.7f, -2.45f, -17.1f, 0.74f, 129f),
        new(-27.2f, -2.45f, -10.9f, 0.59f, 193f),
        new(-25.1f, -2.45f, -4.8f, 0.68f, 252f),
        new(-27.9f, -2.45f, 1.1f, 0.76f, 311f),
        new(-25.6f, -2.45f, 7.5f, 0.61f, 39f),
        new(-27.3f, -2.45f, 13.8f, 0.71f, 104f),
        new(-25.4f, -2.45f, 20.1f, 0.56f, 167f),
        new(-28.1f, -2.45f, 26.6f, 0.69f, 229f),
        new(-20.8f, -2.45f, -24.7f, 0.63f, 294f),
        new(-13.8f, -2.45f, -27.9f, 0.75f, 354f),
        new(-7.1f, -2.45f, -29.8f, 0.58f, 58f),
        new(0.2f, -2.45f, -31.6f, 0.70f, 123f),
        new(7.3f, -2.45f, -30.3f, 0.64f, 187f),
        new(14.4f, -2.45f, -27.4f, 0.77f, 266f),
        new(21.7f, -2.45f, -23.3f, 0.60f, 329f),
    };

    [ContextMenu("Rebuild Background Trees")]
    public void RebuildTrees()
    {
        if (!gameObject.scene.IsValid())
            return;

        ResolveTreeReferences();

        if (treeMesh == null || treeMaterial == null)
            return;

        ClearGeneratedTrees();

        for (int i = 0; i < Placements.Length; i++)
        {
            TreePlacement placement = Placements[i];
            GameObject tree = new GameObject($"{GeneratedTreePrefix} ({i + 1:00})");
            tree.transform.SetParent(transform, false);
            tree.transform.localPosition = placement.Position;
            tree.transform.localRotation = Quaternion.Euler(-90f, 0f, placement.RotationZ);
            tree.transform.localScale = new Vector3(placement.Scale, placement.Scale * 0.478f, placement.Scale * 0.476f);

            MeshFilter meshFilter = tree.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = treeMesh;

            MeshRenderer meshRenderer = tree.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = treeMaterial;
        }
    }

    private void ResolveTreeReferences()
    {
        if (treeMesh != null && treeMaterial != null)
            return;

        MeshFilter[] meshFilters = FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.transform.IsChildOf(transform) || !meshFilter.name.ToLowerInvariant().Contains("arbol"))
                continue;

            if (treeMesh == null)
                treeMesh = meshFilter.sharedMesh;

            if (treeMaterial == null && meshFilter.TryGetComponent(out MeshRenderer meshRenderer))
                treeMaterial = meshRenderer.sharedMaterial;

            if (treeMesh != null && treeMaterial != null)
                return;
        }
    }

    private void ClearGeneratedTrees()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (!child.name.StartsWith(GeneratedTreePrefix))
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private readonly struct TreePlacement
    {
        public TreePlacement(float x, float y, float z, float scale, float rotationZ)
        {
            Position = new Vector3(x, y, z);
            Scale = scale;
            RotationZ = rotationZ;
        }

        public Vector3 Position { get; }
        public float Scale { get; }
        public float RotationZ { get; }
    }
}
