using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class PlayerHealth : MonoBehaviour
{
    private const string ShootTrigger = "shoot";
    private const string RollTrigger = "Roll";
    private const string SpeedParameter = "speed";
    private const string DeathTrigger = "death";

    public int maxLife = 100;
    public int currentLife;

    [Header("UI")]
    public Image healthFill;
    public TMP_Text healthText;

    [Header("Game Over")]
    public GameOverManager gameOverManager;
    public float deathAnimationTime = 1.5f;

    private bool isDead;
    private Animator animator;
    private NavMeshAgent agent;

    void Start()
    {
        currentLife = maxLife;

        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        UpdateUI();
    }

    public void RestLife(int damage)
    {
        if (isDead)
            return;

        currentLife -= damage;

        if (currentLife < 0)
            currentLife = 0;

        UpdateUI();

        if (currentLife <= 0)
            StartCoroutine(Die());
    }

    public void TakeDamage(int damage)
    {
        RestLife(damage);
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentLife += amount;

        if (currentLife > maxLife)
            currentLife = maxLife;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentLife / maxLife;

        if (healthText != null)
            healthText.text = currentLife + " / " + maxLife;
    }

    private IEnumerator Die()
    {
        if (isDead)
            yield break;

        isDead = true;

        // Parar movimiento
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        // Desactivar controles
        ClickToMove clickToMove = GetComponent<ClickToMove>();
        if (clickToMove != null)
            clickToMove.enabled = false;

        PlayerRoll playerRoll = GetComponent<PlayerRoll>();
        if (playerRoll != null)
            playerRoll.enabled = false;

        // Lanzar animación muerte
        if (animator != null)
        {
            animator.ResetTrigger(ShootTrigger);
            animator.ResetTrigger(RollTrigger);
            animator.SetFloat(SpeedParameter, 0f);
            animator.applyRootMotion = true;
            animator.SetTrigger(DeathTrigger);
        }

        // Esperar a que termine la animación antes del Game Over
        yield return new WaitForSeconds(deathAnimationTime);

        if (gameOverManager != null)
            gameOverManager.ShowGameOver();
    }
}