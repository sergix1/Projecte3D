using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerRoll : MonoBehaviour
{
    [SerializeField] private float rollDistance = 2.8f;
    [SerializeField] private float rollDuration = 0.2f;
    [SerializeField] private float rollCooldown = 1f;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private ClickToMove clickToMove;

    private Camera mainCamera;
    private bool canRoll = true;

    void Start()
    {
        mainCamera = Camera.main;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (clickToMove == null)
            clickToMove = GetComponent<ClickToMove>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canRoll)
        {
            Vector3 direction = GetDirectionToMouse();

            if (direction != Vector3.zero)
            {
                StartCoroutine(Roll(direction));
            }
        }
    }

    private Vector3 GetDirectionToMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = hit.point - transform.position;
            direction.y = 0;

            return direction.normalized;
        }

        return Vector3.zero;
    }

    private IEnumerator Roll(Vector3 direction)
    {
        canRoll = false;
        clickToMove?.StopManualMovement();

        bool canUseAgent = CanUseAgent();
        if (canUseAgent)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        transform.rotation = Quaternion.LookRotation(direction);

        if (animator != null)
            animator.SetTrigger("Roll");

        float timer = 0f;
        float speed = rollDistance / rollDuration;

        while (timer < rollDuration)
        {
            Vector3 displacement = direction * speed * Time.deltaTime;
            if (clickToMove != null)
                displacement = clickToMove.ClampManualDisplacement(displacement);

            if (displacement.sqrMagnitude <= 0.0001f)
                break;

            transform.position += displacement;
            timer += Time.deltaTime;
            yield return null;
        }

        if (canUseAgent)
        {
            agent.Warp(transform.position);
            if (agent.isOnNavMesh)
                agent.isStopped = false;
        }

        if (animator != null)
            animator.speed = 1f;

        yield return new WaitForSeconds(rollCooldown);

        canRoll = true;
    }

    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }
}
