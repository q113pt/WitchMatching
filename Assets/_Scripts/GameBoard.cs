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
    public GameObject[] gemPrefabs;
    public GameObject[,] allGems;

    private List<GameObject> matchedGems = new List<GameObject>();

    void Start()
    {
        allGems = new GameObject[width, height];
        SetupBoard();
    }

    private void SetupBoard()
    {
        // Tạo ô gạch nền
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.localPosition = new Vector2(x, y); // Dùng localPosition
                tile.name = $"Tile ({x}, {y})";
            }
        }

        // Tạo ngọc
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int gemIndex = Random.Range(0, gemPrefabs.Length);
                while (CausesMatchOnStart(x, y, gemPrefabs[gemIndex]))
                {
                    gemIndex = Random.Range(0, gemPrefabs.Length);
                }

                GameObject gem = Instantiate(gemPrefabs[gemIndex], transform);
                gem.transform.localPosition = new Vector2(x, y); // Dùng localPosition
                gem.name = $"Gem ({x}, {y})";

                Gem g = gem.GetComponent<Gem>();
                g.column = x;
                g.row = y;
                allGems[x, y] = gem;
            }
        }
    }

    private bool CausesMatchOnStart(int col, int row, GameObject gem)
    {
        if (col > 1)
        {
            if (allGems[col - 1, row].tag == gem.tag && allGems[col - 2, row].tag == gem.tag) return true;
        }
        if (row > 1)
        {
            if (allGems[col, row - 1].tag == gem.tag && allGems[col, row - 2].tag == gem.tag) return true;
        }
        return false;
    }

    public void HandleSwipe(Gem swipedGem, int xDir, int yDir)
    {
        int targetCol = swipedGem.column + xDir;
        int targetRow = swipedGem.row + yDir;

        if (targetCol >= 0 && targetCol < width && targetRow >= 0 && targetRow < height)
        {
            GameObject targetGemObject = allGems[targetCol, targetRow];
            if (targetGemObject != null)
            {
                Gem targetGem = targetGemObject.GetComponent<Gem>();
                StartCoroutine(SwapAndCheck(swipedGem, targetGem));
            }
        }
    }

    private IEnumerator SwapAndCheck(Gem gem1, Gem gem2)
    {
        SwapGems(gem1, gem2);
        yield return new WaitForSeconds(0.3f);

        FindAllMatches(); // Gọi hàm tìm kiếm mới
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

    private void SwapGems(Gem gem1, Gem gem2)
    {
        allGems[gem1.column, gem1.row] = gem2.gameObject;
        allGems[gem2.column, gem2.row] = gem1.gameObject;

        int tempCol = gem1.column;
        int tempRow = gem1.row;
        gem1.column = gem2.column; gem1.row = gem2.row;
        gem2.column = tempCol; gem2.row = tempRow;

        StartCoroutine(SmoothMove(gem1.transform, new Vector2(gem1.column, gem1.row)));
        StartCoroutine(SmoothMove(gem2.transform, new Vector2(gem2.column, gem2.row)));
    }

    private IEnumerator SmoothMove(Transform targetTransform, Vector2 targetLocalPos)
    {
        float elapsedTime = 0;
        float duration = 0.2f;
        Vector2 startingLocalPos = targetTransform.localPosition;

        while (elapsedTime < duration)
        {
            targetTransform.localPosition = Vector2.Lerp(startingLocalPos, targetLocalPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        targetTransform.localPosition = targetLocalPos;
    }

    // --- THUẬT TOÁN FLOOD FILL (LOANG) ---
    // Giúp tìm toàn bộ cụm ngọc dính nhau (chữ T, L, U) thay vì cắt lẻ
    public void FindAllMatches()
    {
        matchedGems.Clear();
        bool[,] visited = new bool[width, height]; // Đánh dấu các ô đã kiểm tra

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y] && allGems[x, y] != null)
                {
                    List<GameObject> currentCluster = new List<GameObject>();

                    // Bắt đầu loang để tìm cụm cùng màu
                    FindConnectedGems(x, y, allGems[x, y].tag, visited, currentCluster);

                    // Nếu cụm tìm được >= 3 viên thì tính là match
                    if (currentCluster.Count >= 3)
                    {
                        // 1. Gửi sang GameManager để tạo hiệu ứng (1 lần duy nhất cho cả cụm)
                        GameManager.Instance.ProcessMatches(currentCluster);

                        // 2. Thêm vào danh sách chờ hủy
                        foreach (GameObject gem in currentCluster)
                        {
                            AddToMatches(gem);
                        }
                    }
                }
            }
        }
    }

    // Hàm đệ quy để loang sang 4 hướng
    private void FindConnectedGems(int x, int y, string tag, bool[,] visited, List<GameObject> cluster)
    {
        // Kiểm tra biên và điều kiện dừng
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (visited[x, y]) return;
        if (allGems[x, y] == null || allGems[x, y].tag != tag) return;

        visited[x, y] = true;
        cluster.Add(allGems[x, y]);

        // Loang sang 4 ô xung quanh
        FindConnectedGems(x + 1, y, tag, visited, cluster);
        FindConnectedGems(x - 1, y, tag, visited, cluster);
        FindConnectedGems(x, y + 1, tag, visited, cluster);
        FindConnectedGems(x, y - 1, tag, visited, cluster);
    }

    private void AddToMatches(GameObject gem)
    {
        if (!matchedGems.Contains(gem)) matchedGems.Add(gem);
    }

    private IEnumerator DestroyAndRefillBoard()
    {
        foreach (GameObject gem in matchedGems)
        {
            if (gem != null)
            {
                int col = gem.GetComponent<Gem>().column;
                int row = gem.GetComponent<Gem>().row;
                allGems[col, row] = null;

                // Hiệu ứng bay lên
                gem.GetComponent<Collider2D>().enabled = false;
                Rigidbody2D rb = gem.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;

                float upwardForce = Random.Range(15f, 20f);
                rb.AddForce(new Vector2(0f, upwardForce), ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-500f, 500f));

                Destroy(gem, 2f);
            }
        }
        matchedGems.Clear();
        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(CollapseColumns());
        yield return StartCoroutine(RefillBoard());

        FindAllMatches();
        if (matchedGems.Count > 0)
        {
            yield return StartCoroutine(DestroyAndRefillBoard());
        }
    }

    private IEnumerator CollapseColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int emptySpotIndex = 0;
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null)
                {
                    if (y != emptySpotIndex)
                    {
                        allGems[x, emptySpotIndex] = allGems[x, y];
                        allGems[x, y] = null;
                        Gem g = allGems[x, emptySpotIndex].GetComponent<Gem>();
                        g.row = emptySpotIndex;
                        StartCoroutine(SmoothMove(allGems[x, emptySpotIndex].transform, new Vector2(x, emptySpotIndex)));
                    }
                    emptySpotIndex++;
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    Vector2 startLocalPos = new Vector2(x, y + height);
                    int gemIndex = Random.Range(0, gemPrefabs.Length);

                    GameObject gem = Instantiate(gemPrefabs[gemIndex], transform);
                    gem.transform.localPosition = startLocalPos;

                    Gem g = gem.GetComponent<Gem>();
                    g.column = x;
                    g.row = y;
                    allGems[x, y] = gem;

                    StartCoroutine(SmoothMove(gem.transform, new Vector2(x, y)));
                }
            }
        }
        yield return null;
    }
}