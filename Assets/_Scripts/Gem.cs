using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;

    [Header("Swipe Logic")]
    private Vector2 firstMousePosition;
    private Vector2 finalMousePosition;
    public float swipeThreshold = 0.5f; // Khoảng cách vuốt tối thiểu

    private GameBoard board;

    void Start()
    {
        board = FindFirstObjectByType<GameBoard>();
    }

    // Ghi lại vị trí khi bắt đầu nhấn chuột
    private void OnMouseDown()
    {
        firstMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    // Ghi lại vị trí khi thả chuột và tính toán
    private void OnMouseUp()
    {
        finalMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Chỉ tính là vuốt nếu khoảng cách đủ lớn
        if (Vector2.Distance(firstMousePosition, finalMousePosition) > swipeThreshold)
        {
            CalculateSwipe();
        }
    }

    // Tính toán hướng vuốt
    void CalculateSwipe()
    {
        // Tính góc của cú vuốt
        float swipeAngle = Mathf.Atan2(finalMousePosition.y - firstMousePosition.y, finalMousePosition.x - firstMousePosition.x) * Mathf.Rad2Deg;

        // Vuốt sang Phải
        if (swipeAngle > -45 && swipeAngle <= 45)
        {
            board.HandleSwipe(this, 1, 0); // (Viên ngọc, hướng X, hướng Y)
        }
        // Vuốt lên Trên
        else if (swipeAngle > 45 && swipeAngle <= 135)
        {
            board.HandleSwipe(this, 0, 1);
        }
        // Vuốt sang Trái
        else if (swipeAngle > 135 || swipeAngle <= -135)
        {
            board.HandleSwipe(this, -1, 0);
        }
        // Vuốt xuống Dưới
        else if (swipeAngle < -45 && swipeAngle >= -135)
        {
            board.HandleSwipe(this, 0, -1);
        }
    }

    // Giữ lại hiệu ứng hover (nổi lên)
    private void OnMouseEnter()
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1.0f);
    }

    private void OnMouseExit()
    {
        transform.localScale = Vector3.one;
    }
}