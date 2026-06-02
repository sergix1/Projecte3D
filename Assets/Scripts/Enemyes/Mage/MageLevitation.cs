using System.Collections.Generic;
using UnityEngine;

public class MageLevitation : MonoBehaviour
{
    private const string VisualPivotName = "MageVisualPivot";

    public Transform[] visualRoots;
    public float height = 0.15f;
    public float speed = 2f;

    private Transform visualPivot;
    private Vector3 basePivotLocalPosition;
    private Quaternion visualLocalRotation = Quaternion.identity;
    private float currentYOffset;

    private void Start()
    {
        FindVisualRoots();
        ResetBasePosition();
    }

    private void LateUpdate()
    {
        EnsureVisualPivot();

        if (visualPivot == null)
            return;

        currentYOffset = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f * height;
        visualPivot.localPosition = basePivotLocalPosition + Vector3.up * currentYOffset;
        visualPivot.localRotation = visualLocalRotation;
    }

    public void SetLookRotation(Quaternion worldRotation)
    {
        EnsureVisualPivot();

        visualLocalRotation = Quaternion.Inverse(transform.rotation) * worldRotation;

        if (visualPivot != null)
            visualPivot.localRotation = visualLocalRotation;
    }

    public void ResetBasePosition()
    {
        FindVisualRoots();
        EnsureVisualPivot();

        if (visualPivot == null)
            return;

        basePivotLocalPosition = visualPivot.localPosition - Vector3.up * currentYOffset;
        currentYOffset = 0f;
    }

    private void EnsureVisualPivot()
    {
        if (visualPivot != null)
            return;

        Transform existingPivot = transform.Find(VisualPivotName);

        if (existingPivot != null)
        {
            visualPivot = existingPivot;
        }
        else
        {
            GameObject pivotObject = new GameObject(VisualPivotName);
            visualPivot = pivotObject.transform;
            visualPivot.SetParent(transform, false);
            visualPivot.localPosition = GetVisualCenter();
        }

        basePivotLocalPosition = visualPivot.localPosition;
        MoveVisualsToPivot();
    }

    private void FindVisualRoots()
    {
        if (visualRoots != null && visualRoots.Length > 0)
            return;

        List<Transform> roots = new List<Transform>();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer childRenderer in renderers)
        {
            if (childRenderer == null || childRenderer.transform == transform)
                continue;

            if (!childRenderer.enabled || !childRenderer.gameObject.activeInHierarchy)
                continue;

            if (IsSelectionVisual(childRenderer.transform))
                continue;

            AddRoot(roots, GetTopLevelChild(childRenderer.transform));
        }

        visualRoots = roots.ToArray();
    }

    private Transform GetTopLevelChild(Transform child)
    {
        Transform result = child;

        while (result.parent != null && result.parent != transform)
            result = result.parent;

        return result;
    }

    private void AddRoot(List<Transform> roots, Transform root)
    {
        if (root == null || root == visualPivot || roots.Contains(root))
            return;

        roots.Add(root);
    }

    private Vector3 GetVisualCenter()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

        if (capsuleCollider != null)
            return capsuleCollider.center;

        return Vector3.zero;
    }

    private void MoveVisualsToPivot()
    {
        if (visualRoots == null)
            return;

        foreach (Transform visualRoot in visualRoots)
        {
            if (visualRoot == null || visualRoot == visualPivot || visualRoot.parent == visualPivot)
                continue;

            visualRoot.SetParent(visualPivot, true);
        }

        MageAI mageAI = GetComponent<MageAI>();

        if (mageAI != null && mageAI.firePoint != null && mageAI.firePoint.parent != visualPivot)
            mageAI.firePoint.SetParent(visualPivot, true);
    }

    private bool IsSelectionVisual(Transform child)
    {
        EnemyBase enemyBase = GetComponent<EnemyBase>();

        return enemyBase != null
            && enemyBase.SelectedObj != null
            && child.IsChildOf(enemyBase.SelectedObj.transform);
    }
}
