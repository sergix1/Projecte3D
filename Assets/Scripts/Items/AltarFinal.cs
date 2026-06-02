using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class AltarFinal : MonoBehaviour
{
    [Header("Requisitos")]
    public int requiredRunes = 5;

    [Header("Referencias")]
    public Transform player;
    public ClickToMove clickToMove;
    public PlayerRoll playerRoll;
    public GameObject runePrefab;
    public Transform spawnPoint;
    public ParticleSystem summonParticles;
    public Light altarLight;
    public GameObject victoryPanel;
    public TextMeshProUGUI interactText;

    [Header("Distancias")]
    public float interactDistance = 2.5f;

    [Header("Animación")]
    public float runeFloatHeight = 1.5f;
    public float runeRiseDuration = 1.5f;
    public float runeRotateSpeed = 90f;

    private bool activated;
    private GameObject spawnedRune;
    private NavMeshAgent playerAgent;

    public GameObject gameplayHud;
    public Canvas canvasVida;
    public GameObject runasCanvas;

    void Start()
    {
        if (player != null)
            playerAgent = player.GetComponent<NavMeshAgent>();

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (interactText != null)
            interactText.text = "";
    }

    void Update()
    {
        if (activated || player == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool playerInRange = distance <= interactDistance;

        bool hasEnoughRunes = GameManager.Instance != null &&
                              GameManager.Instance.GetSoulCount >= requiredRunes;

        if (interactText != null)
        {
            if (playerInRange && hasEnoughRunes)
                interactText.text = "Pulsa E para invocar la runa";
            else if (playerInRange)
                interactText.text = "Necesitas más runas";
            else
                interactText.text = "";
        }

        if (playerInRange && hasEnoughRunes && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FinalSequence());
        }
    }

    private IEnumerator FinalSequence()
    {
        activated = true;

        if (interactText != null)
            interactText.text = "";

        StopPlayer();

        LookAtAltar();

        Vector3 spawnPos = spawnPoint != null
            ? spawnPoint.position
            : transform.position + Vector3.up * 1.2f;

        if (runePrefab != null)
            spawnedRune = Instantiate(runePrefab, spawnPos, Quaternion.identity);

        if (summonParticles != null)
            summonParticles.Play();

        if (altarLight != null)
            StartCoroutine(IncreaseLight());

        if (spawnedRune != null)
            yield return StartCoroutine(AnimateRune());

        yield return new WaitForSeconds(2f);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        //Desactivo el hud.
        if (gameplayHud != null)
            gameplayHud.SetActive(false);
        if (canvasVida != null)
            canvasVida.gameObject.SetActive(false);
        if (runasCanvas != null)
            runasCanvas.SetActive(false);


        Time.timeScale = 0f;
    }


    private void StopPlayer()
    {
        if (clickToMove != null)
        {
            clickToMove.StopManualMovement();
            clickToMove.enabled = false;
        }

        if (playerRoll != null)
            playerRoll.enabled = false;

        if (playerAgent != null && playerAgent.enabled && playerAgent.isOnNavMesh)
        {
            playerAgent.isStopped = true;
            playerAgent.ResetPath();
            playerAgent.velocity = Vector3.zero;
        }
    }

    private void LookAtAltar()
    {
        Vector3 lookDirection = transform.position - player.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
            player.rotation = Quaternion.LookRotation(lookDirection);
    }

    private IEnumerator AnimateRune()
    {
        float timer = 0f;
        Vector3 startPos = spawnedRune.transform.position;
        Vector3 endPos = startPos + Vector3.up * runeFloatHeight;

        while (timer < runeRiseDuration)
        {
            timer += Time.deltaTime;
            float t = timer / runeRiseDuration;

            spawnedRune.transform.position = Vector3.Lerp(startPos, endPos, t);
            spawnedRune.transform.Rotate(Vector3.up * runeRotateSpeed * Time.deltaTime);

            yield return null;
        }

        float extraTimer = 0f;
        float extraTime = 2f;

        while (extraTimer < extraTime)
        {
            extraTimer += Time.deltaTime;
            spawnedRune.transform.Rotate(Vector3.up * runeRotateSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private IEnumerator IncreaseLight()
    {
        float startIntensity = altarLight.intensity;
        float targetIntensity = startIntensity + 3f;
        float duration = 1.5f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            altarLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, timer / duration);
            yield return null;
        }
    }
}