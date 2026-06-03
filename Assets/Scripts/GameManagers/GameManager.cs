using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TMP_Text soulText;
    private int soulCount;
    [SerializeField] private PopSoul soulUIPop;
    public int GetSoulCount => soulCount;
    [Header("Maldición+")]
    public static bool gamePlusActive;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddSouls(int soulAmount)
    {
        soulCount += soulAmount;
        UpdateSoulUI();
        if (soulUIPop != null)
        {
            soulUIPop.Pop();
        }

    }
    private void UpdateSoulUI()
    {
        if (soulText != null)
        {
            soulText.text = "" + soulCount;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
