using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Animator animator;
    public float animationSpeed = 1f;

    private float lastAnimationSpeed = -1f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (animator == null || Mathf.Approximately(lastAnimationSpeed, animationSpeed))
            return;

        animator.speed = animationSpeed;
        lastAnimationSpeed = animationSpeed;
    }
}
