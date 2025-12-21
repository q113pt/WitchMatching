using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    public float speed = 2f;      // Tốc độ bay lên
    public float lifeTime = 1f;   // Thời gian tồn tại
    private Text textElement;

    void Start()
    {
        textElement = GetComponentInChildren<Text>();
        Destroy(gameObject, lifeTime); // Tự xóa sau 1 giây
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Hiệu ứng mờ dần
        if (textElement != null)
        {
            Color c = textElement.color;
            c.a -= Time.deltaTime / lifeTime;
            textElement.color = c;
        }
    }

    public void SetText(string val, Color color)
    {
        if (textElement == null) textElement = GetComponentInChildren<Text>();
        textElement.text = val;
        textElement.color = color;
    }
}