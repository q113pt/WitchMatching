using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour
{
    [Header("Board Dimensions")]
    public int width;
    public int height;

    [Header("Game Objects")]
    public GameObject tilePrefab;
    public GameObject[] gemPrefabs; // Mảng chứa các prefab ngọc
    public GameObject[,] allGems; // Mảng 2 chiều để lưu trữ tất cả ngọc trên bảng

    private Gem selectedGem; // Viên ngọc người chơi chọn đầu tiên
    private List<GameObject> matchedGems = new List<GameObject>(); // Danh sách các ngọc trùng khớp để phá hủy

    void Start()
    {
        allGems = new GameObject[width, height];
        SetupBoard();
    }

    // Thiết lập bảng chơi ban đầu
    private void SetupBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.parent = this.transform;
                tile.name = $"Tile ({x}, {y})";
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);
                int gemIndex = Random.Range(0, gemPrefabs.Length);

                // Tránh tạo match ngay từ đầu
                while (CausesMatchOnStart(x, y, gemPrefabs[gemIndex]))
                {
                    gemIndex = Random.Range(0, gemPrefabs.Length);
                }

                GameObject gem = Instantiate(gemPrefabs[gemIndex], position, Quaternion.identity);
                gem.transform.parent = this.transform;
                gem.name = $"Gem ({x}, {y})";

                // Gán vị trí cho viên ngọc
                gem.GetComponent<Gem>().column = x;
                gem.GetComponent<Gem>().row = y;

                allGems[x, y] = gem;
            }
        }
    }

    // Kiểm tra xem viên ngọc sắp tạo có gây ra match không
    private bool CausesMatchOnStart(int col, int row, GameObject gem)
    {
        if (col > 1) // Kiểm tra 2 viên bên trái
        {
            if (allGems[col - 1, row].tag == gem.tag && allGems[col - 2, row].tag == gem.tag)
            {
                return true;
            }
        }
        if (row > 1) // Kiểm tra 2 viên bên dưới
        {
            if (allGems[col, row - 1].tag == gem.tag && allGems[col, row - 2].tag == gem.tag)
            {
                return true;
            }
        }
        return false;
    }

    public void HandleSwipe(Gem swipedGem, int xDir, int yDir)
    {
        // Lấy vị trí của viên ngọc được vuốt
        int startCol = swipedGem.column;
        int startRow = swipedGem.row;

        // Tính vị trí của viên ngọc mục tiêu
        int targetCol = startCol + xDir;
        int targetRow = startRow + yDir;

        // Kiểm tra xem vị trí mục tiêu có nằm trong ranh giới bàn chơi không
        if (targetCol >= 0 && targetCol < width && targetRow >= 0 && targetRow < height)
        {
            // Lấy GameObject của viên ngọc mục tiêu
            GameObject targetGemObject = allGems[targetCol, targetRow];

            if (targetGemObject != null)
            {
                // Lấy component Gem từ GameObject
                Gem targetGem = targetGemObject.GetComponent<Gem>();

                // Bắt đầu Coroutine để hoán đổi và kiểm tra
                // Chúng ta tận dụng lại hàm SwapAndCheck mà không cần sửa nó
                StartCoroutine(SwapAndCheck(swipedGem, targetGem));
            }
        }
        // Nếu vuốt ra ngoài ranh giới hoặc vào ô trống, không làm gì cả.
    }

    // Coroutine để hoán đổi, kiểm tra, và hoán đổi lại nếu không hợp lệ
    private IEnumerator SwapAndCheck(Gem gem1, Gem gem2)
    {
        SwapGems(gem1, gem2);
        yield return new WaitForSeconds(0.3f);

        FindAllMatches();
        if (matchedGems.Count > 0)
        {
            GameManager.Instance.DecreaseMove();

            yield return StartCoroutine(DestroyAndRefillBoard());
        }
        else
        {
            SwapGems(gem1, gem2);
        }
    }

    // Hàm logic hoán đổi 2 viên ngọc
    private void SwapGems(Gem gem1, Gem gem2)
    {
        // Lưu trữ toàn bộ thông tin của gem1
        int tempCol = gem1.column;
        int tempRow = gem1.row;
        Vector2 tempPos = gem1.transform.position;

        // Cập nhật mảng và thông tin cho gem1 dựa trên gem2
        allGems[gem2.column, gem2.row] = gem1.gameObject;
        gem1.column = gem2.column;
        gem1.row = gem2.row;
        gem1.transform.position = gem2.transform.position;

        // Cập nhật mảng và thông tin cho gem2 dựa trên thông tin đã lưu
        allGems[tempCol, tempRow] = gem2.gameObject;
        gem2.column = tempCol;
        gem2.row = tempRow;
        gem2.transform.position = tempPos;
    }

    // Hàm tìm tất cả các kết quả trùng khớp trên bảng
    // Trong GameBoard.cs
    // THAY THẾ TOÀN BỘ HÀM CŨ BẰNG HÀM NÀY
    public void FindAllMatches()
    {
        matchedGems.Clear(); // Xóa danh sách để chuẩn bị phá hủy

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject currentGem = allGems[x, y];
                if (currentGem == null) continue;

                // KIỂM TRA THEO CHIỀU NGANG
                if (x > 0 && allGems[x - 1, y] != null && allGems[x - 1, y].tag == currentGem.tag)
                {
                    continue;
                }

                List<GameObject> horizontalMatches = new List<GameObject>();
                for (int i = x; i < width; i++)
                {
                    if (allGems[i, y] != null && allGems[i, y].tag == currentGem.tag)
                    {
                        horizontalMatches.Add(allGems[i, y]);
                    }
                    else
                    {
                        break;
                    }
                }
                if (horizontalMatches.Count >= 3)
                {
                    // --- THAY ĐỔI Ở ĐÂY ---
                    // 1. Gửi chuỗi này đến GameManager để tính sát thương
                    GameManager.Instance.ProcessMatches(horizontalMatches);

                    // 2. Thêm vào danh sách để phá hủy
                    foreach (GameObject gem in horizontalMatches)
                    {
                        if (!matchedGems.Contains(gem)) matchedGems.Add(gem);
                    }
                }
            }
        }

        // Tương tự, kiểm tra theo chiều dọc
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject currentGem = allGems[x, y];
                if (currentGem == null) continue;

                if (y > 0 && allGems[x, y - 1] != null && allGems[x, y - 1].tag == currentGem.tag)
                {
                    continue;
                }

                List<GameObject> verticalMatches = new List<GameObject>();
                for (int i = y; i < height; i++)
                {
                    if (allGems[x, i] != null && allGems[x, i].tag == currentGem.tag)
                    {
                        verticalMatches.Add(allGems[x, i]);
                    }
                    else
                    {
                        break;
                    }
                }
                if (verticalMatches.Count >= 3)
                {
                    // --- THAY ĐỔI Ở ĐÂY ---
                    // 1. Gửi chuỗi này đến GameManager để tính sát thương
                    GameManager.Instance.ProcessMatches(verticalMatches);

                    // 2. Thêm vào danh sách để phá hủy
                    foreach (GameObject gem in verticalMatches)
                    {
                        if (!matchedGems.Contains(gem))
                        {
                            matchedGems.Add(gem);
                        }
                    }
                }
            }
        }
    }

    // Coroutine để phá hủy và lấp đầy lại bảng
    private IEnumerator DestroyAndRefillBoard()
    {
        // 1. Phá hủy các viên ngọc đã match
        foreach (GameObject gem in matchedGems)
        {
            if (gem != null)
            {
                // Lấy vị trí để cập nhật mảng allGems
                int col = gem.GetComponent<Gem>().column;
                int row = gem.GetComponent<Gem>().row;
                allGems[col, row] = null;

                // --- TẠO HIỆU ỨNG BAY ---
                // 1. Tắt collider để nó không va vào ngọc khác
                gem.GetComponent<Collider2D>().enabled = false;

                // 2. Thêm Rigidbody để nó chịu tác dụng của vật lý
                Rigidbody2D rb = gem.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0; // Chúng ta không muốn nó rơi xuống

                // 3. "Bắn" nó bay đi ngẫu nhiên
                Vector2 randomForce = new Vector2(Random.Range(-150f, 150f), Random.Range(100f, 200f));
                rb.AddForce(randomForce);

                // 4. Phá hủy nó sau 1 giây
                Destroy(gem, 1f);
            }
        }
        matchedGems.Clear();
        yield return new WaitForSeconds(0.2f); // Đợi hiệu ứng nổ

        // 2. Làm ngọc ở trên rơi xuống (Gravity)
        yield return StartCoroutine(CollapseColumns());

        // 3. Tạo ngọc mới lấp vào chỗ trống
        yield return StartCoroutine(RefillBoard());

        // 4. Kiểm tra xem có match mới nào được tạo ra không (phản ứng chuỗi)
        FindAllMatches();
        if (matchedGems.Count > 0)
        {
            // Nếu có, lặp lại quá trình
            yield return StartCoroutine(DestroyAndRefillBoard());
        }
    }

    // Coroutine làm các viên ngọc rơi xuống
    private IEnumerator CollapseColumns()
    {
        for (int x = 0; x < width; x++)
        {
            // "Con trỏ" chỉ đến ô trống dưới cùng mà viên ngọc tiếp theo sẽ rơi vào
            int emptySpotIndex = 0;
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null) // Nếu ô này có ngọc
                {
                    // Cập nhật tọa độ logic cho viên ngọc
                    allGems[x, y].GetComponent<Gem>().row = emptySpotIndex;

                    // Di chuyển viên ngọc trong mảng xuống vị trí của con trỏ
                    if (emptySpotIndex != y) // Chỉ di chuyển nếu cần
                    {
                        allGems[x, emptySpotIndex] = allGems[x, y];
                        allGems[x, y] = null;
                    }

                    emptySpotIndex++; // Di chuyển con trỏ lên trên cho viên ngọc tiếp theo
                }
            }
        }

        // Sau khi logic mảng đã đúng, giờ mới cập nhật vị trí thực tế
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null)
                {
                    allGems[x, y].transform.position = new Vector2(x, y);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
    }

    // Coroutine lấp đầy bảng bằng các viên ngọc mới
    private IEnumerator RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    Vector2 position = new Vector2(x, y);
                    int gemIndex = Random.Range(0, gemPrefabs.Length);
                    GameObject gem = Instantiate(gemPrefabs[gemIndex], position, Quaternion.identity);
                    gem.transform.parent = this.transform;
                    gem.name = $"Gem ({x}, {y})";

                    gem.GetComponent<Gem>().column = x;
                    gem.GetComponent<Gem>().row = y;

                    allGems[x, y] = gem;
                }
            }
        }
        yield return null;
    }
}