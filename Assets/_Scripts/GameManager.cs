using UnityEngine;
using UnityEngine.UI; // Cần cho UI
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // --- Biến Singleton ---
    // Giúp các script khác dễ dàng gọi GameManager.Instance
    public static GameManager Instance;

    [Header("Game Logic")]
    public int movesLeft = 20; // Số lượt chơi
    public Boss boss; // Kéo Boss vào đây

    [Header("UI Elements")]
    public Text movesText; // Kéo UI Text hiển thị số lượt vào đây

    void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        UpdateMovesText();
    }

    // Hàm này được gọi từ GameBoard sau một nước đi HỢP LỆ
    public void DecreaseMove()
    {
        movesLeft--;
        UpdateMovesText();

        // Kiểm tra thắng/thua ngay sau khi trừ lượt
        CheckWinLoss();
    }

    // Hàm xử lý logic tính sát thương
    // GameBoard sẽ gọi hàm này và truyền vào danh sách các chuỗi match
    public void ProcessMatches(List<GameObject> chain)
    {
        if (chain.Count == 0 || boss == null)
        {
            return;
        }

        int chainLength = chain.Count;
        string gemTag = chain[0].tag; // Tất cả ngọc trong chuỗi đều cùng tag
        float baseDamage = 0;

        // --- 1. TÍNH SÁT THƯƠNG CƠ BẢN DỰA TRÊN SỐ LƯỢNG ---
        switch (chainLength)
        {
            case 3:
                baseDamage = 10;
                break;
            case 4:
                baseDamage = 20; // Thưởng thêm cho match-4
                break;
            case 5:
                baseDamage = 40;
                break;
            case 6:
                baseDamage = 70; // Thưởng lớn
                break;
            default: // Match 7 hoặc nhiều hơn
                baseDamage = 100;
                break;
        }

        // --- 2. LẤY HỆ SỐ NHÂN TỪ BOSS ---
        float multiplier = boss.GetMultiplierForGem(gemTag);

        // --- 3. TÍNH SÁT THƯƠNG CUỐI CÙNG VÀ GÂY SÁT THƯƠNG ---
        float finalDamage = baseDamage * multiplier;

        Debug.Log($"Match {chainLength} viên {gemTag}. Sát thương: {baseDamage} * {multiplier} = {finalDamage}");
        boss.TakeDamage(finalDamage);

        // CheckWinLoss(); // Không cần check ở đây, vì hàm TakeDamage của Boss sẽ tự check
    }

    // Cập nhật UI hiển thị số lượt
    void UpdateMovesText()
    {
        if (movesText != null)
        {
            movesText.text = "Lượt: " + movesLeft.ToString();
        }
    }

    // Kiểm tra điều kiện thắng/thua
    public void CheckWinLoss()
    {
        if (boss.currentHealth <= 0)
        {
            // Thắng!
            Debug.Log("CHIẾN THẮNG!");
            // Tạm thời dừng game, sau này bạn có thể tải màn chơi tiếp theo
            Time.timeScale = 0;
        }
        else if (movesLeft <= 0)
        {
            // Thua!
            Debug.Log("GAME OVER!");
            // Tạm thời dừng game, sau này bạn có thể hiện bảng "Thua"
            Time.timeScale = 0;
        }
    }
}