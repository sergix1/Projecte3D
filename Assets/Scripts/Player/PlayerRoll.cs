using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerRoll : MonoBehaviour
{
    [SerializeField] private float rollDistance = 2.8f;
    [SerializeField] private float rollDuration = 0.2f;
    [SerializeField] private float rollCooldown = 3f;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private ClickToMove clickToMove;

    [Header("UI")]
    [SerializeField] private AbilityCooldownUI rollCooldownUI;

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
            animator.SetTrigger("Roll");

        float timer = 0f;
        float speed = rollDistance / rollDuration;
        bool useAgent = agent != null && agent.enabled && agent.isOnNavMesh;

        while (timer < rollDuration)
        {
            Vector3 movement = direction * speed * Time.deltaTime;

            if (useAgent)
            {
                Vector3 nextPosition = transform.position + movement;

                if (NavMesh.Raycast(transform.position, nextPosition, out _, agent.areaMask))
                    break;

                agent.Move(movement);
            }
            else
            {
                transform.position += movement;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }
}