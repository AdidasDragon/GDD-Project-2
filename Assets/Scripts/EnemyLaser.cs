using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyLaser : MonoBehaviour

{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] public Transform playerTarget;

    private Rigidbody2D rb;

    private void Awake()
    {
        Debug.Log("Spawn Laser");
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        // find player target if not assigned
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        // compute direction and velocity in direction of player
        if (playerTarget != null)
        {
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.GetComponentInParent<EnemyController>() != null)
        {
            return;
        }
        BugController player = collision.GetComponent<BugController>();
        if (player == null)
        {
            player = collision.GetComponentInParent<BugController>();
        }

        if (player != null || collision.CompareTag("Player"))
        {
            if (player != null)
            {
                player.TakeDamage(damage);
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