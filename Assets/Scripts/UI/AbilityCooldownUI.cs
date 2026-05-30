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

        if (cooldownText != null)
            cooldownText.text = "";
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer < 0)
                timer = 0;

            overlay.fillAmount = timer / cooldown;

            if (cooldownText != null)
                cooldownText.text = Mathf.CeilToInt(timer).ToString();
        }
        else
        {
            overlay.fillAmount = 0;

            if (cooldownText != null)
                cooldownText.text = "";
        }
    }

    public bool StartCooldown(float time)
    {
        if (timer > 0)
            return false;

        cooldown = time;
        timer = time;

        overlay.fillAmount = 1;

        if (cooldownText != null)
            cooldownText.text = Mathf.CeilToInt(time).ToString();

        return true;
    }

    public bool CanUse()
    {
        return timer <= 0;
    }
}