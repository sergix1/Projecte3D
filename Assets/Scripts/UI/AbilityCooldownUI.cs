using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCooldownUI : MonoBehaviour
{
    public Image overlay;
    public TMP_Text cooldownText;

    private float cooldown;
    private float timer;

    void Start()
    {
        overlay.fillAmount = 0;
        cooldownText.text = "";
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            overlay.fillAmount = timer / cooldown;
            cooldownText.text = Mathf.CeilToInt(timer).ToString();
        }
        else
        {
            overlay.fillAmount = 0;
            cooldownText.text = "";
        }
    }

    public void StartCooldown(float time)
    {
        cooldown = time;
        timer = time;
        overlay.fillAmount = 1;
        cooldownText.text = time.ToString();
    }

    public bool CanUse()
    {
        return timer <= 0;
    }
}