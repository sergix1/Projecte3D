using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerRoll : MonoBehaviour
{
    private const string RollTrigger = "Roll";

    [SerializeField] private float rollDistance = 2.8f;
    [SerializeField] private float rollDuration = 0.2f;
    [SerializeField] private float rollCooldown = 3f;
    [SerializeField] private KeyCode rollKey = KeyCode.Space;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private ClickToMove clickToMove;

    [Header("UI")]
    [SerializeField] private AbilityCooldownUI rollCooldownUI;

    private Camera mainCamera;
    private bool canRoll = true;

    private void Start()
    {
        mainCamera = Camera.main;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (clickToMove == null)
            clickToMove = GetComponent<ClickToMove>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(rollKey) && canRoll)
        {
            if (rollCooldownUI != null && !rollCooldownUI.CanUse())
                return;

            Vector3 direction = GetDirectionToMouse();

            if (direction != Vector3.zero)
                StartCoroutine(Roll(direction));
        }
    }

    private Vector3 GetDirectionToMouse()
    {
        if (mainCamera == null)
            return Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return Vector3.zero;

        Vector3 direction = hit.point - transform.position;
        direction.y = 0f;

        return direction.normalized;
    }

    private IEnumerator Roll(Vector3 direction)
    {
        canRoll = false;

        if (rollCooldownUI != null)
            rollCooldownUI.StartCooldown(rollCooldown);

        clickToMove?.StopManualMovement();

        transform.rotation = Quaternion.LookRotation(direction);

        if (animator != null)
            animator.SetTrigger(RollTrigger);

        float timer = 0f;
        float speed = rollDistance / rollDuration;
        bool useAgent = agent != null && agent.enabled && agent.isOnNavMesh;
        Vector3 targetPosition = GetRollTarget(direction, useAgent);

        while (timer < rollDuration)
        {
            Vector3 remainingMovement = targetPosition - transform.position;
            remainingMovement.y = 0f;

            if (remainingMovement.sqrMagnitude <= 0.0001f)
                break;

            float stepDistance = Mathf.Min(speed * Time.deltaTime, remainingMovement.magnitude);
            Vector3 movement = remainingMovement.normalized * stepDistance;

            if (useAgent)
                agent.Move(movement);
            else
                transform.position += movement;

            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    private Vector3 GetRollTarget(Vector3 direction, bool useAgent)
    {
        Vector3 targetPosition = transform.position + direction * rollDistance;

        if (!useAgent)
            return targetPosition;

        if (NavMesh.Raycast(transform.position, targetPosition, out NavMeshHit hit, agent.areaMask))
            return hit.position;

        return targetPosition;
    }
}
