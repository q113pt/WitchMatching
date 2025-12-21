using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections; // Cần thiết để dùng Coroutine

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
    public Image healthBar;

    [Header("Weaknesses & Resistances")]
    public List<GemResistance> resistances = new List<GemResistance>();

    [Header("Visual Effects")]
    public Color flashColor = Color.red; // Màu khi bị dính đòn
    public float flashDuration = 0.1f;    // Thời gian nháy
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        // Lấy SpriteRenderer để điều chỉnh màu sắc
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; // Lưu lại màu gốc (trắng)
        }

        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthBar();

        // Kích hoạt hiệu ứng nháy đỏ
        if (spriteRenderer != null)
        {
            StopAllCoroutines(); // Dừng các lần nháy trước đó nếu có
            StartCoroutine(FlashRedEffect());
        }

        if (currentHealth <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckWinLoss();
            }
        }
    }

    // Coroutine đổi màu Boss sang đỏ rồi quay lại bình thường
    IEnumerator FlashRedEffect()
    {
        spriteRenderer.color = flashColor; // Đổi sang màu đỏ
        yield return new WaitForSeconds(flashDuration); // Đợi một chút
        spriteRenderer.color = originalColor; // Trả về màu gốc
    }

    public float GetMultiplierForGem(string gemTag)
    {
        foreach (GemResistance res in resistances)
        {
            if (res.gemTag == gemTag) return res.multiplier;
        }
        return 1.0f;
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }
}