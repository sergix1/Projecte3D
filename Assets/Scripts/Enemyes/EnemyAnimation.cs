using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimation : MonoBehaviour
{
    public Animator animator;

    void Update()
    {
        animator.speed = 2f;
    }
}
