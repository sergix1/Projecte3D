using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TMP_Text soulText;
    private int soulCount;
    [SerializeField] private PopSoul soulUIPop;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
