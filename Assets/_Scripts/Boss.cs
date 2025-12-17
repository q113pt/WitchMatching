using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct GemResistance
{
    public string gemTag;
    public float multiplier;
}

public class Boss : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 1000;
    public float currentHealth;

    [Header("UI")]
    // Dùng 'Image' để điều khiển 'fillAmount'
    public Image healthBar;

    [Header("Weaknesses & Resistances")]
    public List<GemResistance> resistances = new List<GemResistance>();

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckWinLoss();
            }
        }
    }

    public float GetMultiplierForGem(string gemTag)
    {
        foreach (GemResistance res in resistances)
        {
            if (res.gemTag == gemTag)
            {
                return res.multiplier;
            }
        }
        return 1.0f;
    }

    // Hàm cập nhật UI dùng fillAmount
    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            // Cập nhật thanh máu bằng cách thay đổi giá trị Fill Amount
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }
}