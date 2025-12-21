using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Cần thư viện này để chuyển màn

[System.Serializable]
public struct ElementalEffect
{
    public string gemTag;
    public GameObject smallEffect;
    public GameObject bigEffect;
    public GameObject stormEffect;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Logic")]
    public int movesLeft = 15;
    public Boss boss;
    public bool isGameOver = false;
    private bool isPaused = false; // Trạng thái tạm dừng

    [Header("Prefabs & Effects")]
    public GameObject damageTextPrefab;
    public List<ElementalEffect> effects;

    [Header("UI References")]
    public Text movesText;
    public GameObject winPanel;   // Kéo WinPanel vào đây
    public GameObject losePanel;  // Kéo LosePanel vào đây
    public GameObject pausePanel; // Kéo PausePanel vào đây

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1; // Đảm bảo game chạy bình thường

        // Đảm bảo các bảng tắt hết khi bắt đầu
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);

        UpdateMovesText();
    }

    // --- CÁC HÀM LOGIC GAME ---

    public void DecreaseMove()
    {
        if (isGameOver) return;
        movesLeft--;
        UpdateMovesText();
        CheckWinLoss();
    }

    public void ProcessMatches(List<GameObject> chain)
    {
        if (isGameOver || chain == null || chain.Count == 0 || boss == null) return;

        int chainLength = chain.Count;
        string gemTag = chain[0].tag;
        Vector3 spawnPos = chain[chainLength / 2].transform.position;

        float baseDamage = 0;
        if (chainLength >= 7) baseDamage = 100;
        else if (chainLength >= 5) baseDamage = 50;
        else baseDamage = 10;

        float multiplier = boss.GetMultiplierForGem(gemTag);
        float finalDamage = baseDamage * multiplier;

        SpawnProjectile(gemTag, chainLength, spawnPos, finalDamage);
    }

    void SpawnProjectile(string tag, int length, Vector3 pos, float dmg)
    {
        foreach (var effect in effects)
        {
            if (effect.gemTag == tag)
            {
                GameObject prefab = null;
                if (length >= 7) prefab = effect.stormEffect;
                else if (length >= 5) prefab = effect.bigEffect;
                else prefab = effect.smallEffect;

                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab, pos, Quaternion.identity);
                    go.GetComponent<Projectile>().Setup(boss.transform, dmg);
                }
                break;
            }
        }
    }

    public void SpawnDamageText(string val, Vector3 pos)
    {
        if (damageTextPrefab == null) return;
        GameObject go = Instantiate(damageTextPrefab, pos + Vector3.up * 1.5f, Quaternion.identity);
        go.GetComponent<DamageText>().SetText("-" + val, Color.yellow);
    }

    void UpdateMovesText()
    {
        if (movesText != null) movesText.text = "Lượt: " + movesLeft;
    }

    public void CheckWinLoss()
    {
        if (isGameOver) return;

        if (boss != null && boss.currentHealth <= 0) WinGame();
        else if (movesLeft <= 0) LoseGame();
    }

    // --- XỬ LÝ THẮNG / THUA ---

    private void WinGame()
    {
        isGameOver = true;
        Debug.Log("CHIẾN THẮNG!");

        // Hiện bảng thắng
        if (winPanel != null) winPanel.SetActive(true);

        // Dừng game
        Time.timeScale = 0;
    }

    private void LoseGame()
    {
        isGameOver = true;
        Debug.Log("THẤT BẠI!");

        // Hiện bảng thua
        if (losePanel != null) losePanel.SetActive(true);

        // Dừng game
        Time.timeScale = 0;
    }

    // --- CÁC HÀM CHO NÚT BẤM (BUTTONS) ---

    // 1. Nút Pause (Tạm dừng / Tiếp tục)
    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0; // Dừng thời gian
            if (pausePanel) pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1; // Chạy lại bình thường
            if (pausePanel) pausePanel.SetActive(false);
        }
    }

    // 2. Nút Restart (Chơi lại màn hiện tại)
    public void RestartLevel()
    {
        // Load lại Scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 3. Nút Next Level (Qua màn tiếp theo)
    public void NextLevel()
    {
        // Logic: Lấy index màn hiện tại + 1
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Kiểm tra xem có màn tiếp theo không
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Đã hết màn chơi! Quay về Menu chính.");
            SceneManager.LoadScene(0); // Quay về màn đầu tiên (Menu)
        }
    }

    // 4. Nút Quit (Thoát game)
    public void QuitGame()
    {
        Debug.Log("Thoát Game!");
        Application.Quit();
    }
}