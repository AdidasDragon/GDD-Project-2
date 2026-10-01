using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SyntaxLaser : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<BugController>() != null)
        {
            return;
        }
        if (collision.CompareTag("Enemy") || collision.GetComponent<EnemyController>() != null)
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.takeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }
        if (collision.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}