using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    private Transform target;
    private float damage;

    public void Setup(Transform targetBoss, float dmg)
    {
        target = targetBoss;
        damage = dmg;
    }

    void Update()
    {
        if (target == null) return;

        // Bay về hướng Boss
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Khi chạm vào Boss (khoảng cách < 0.2)
        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            // 1. Gây sát thương thực tế
            target.GetComponent<Boss>().TakeDamage(damage);

            // 2. Hiện số nhảy ngay tại đầu Boss
            GameManager.Instance.SpawnDamageText(damage.ToString(), target.position);

            // 3. Biến mất
            Destroy(gameObject);
        }
    }
}