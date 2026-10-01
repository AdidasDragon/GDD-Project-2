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
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // fetch transform of player
            playerTarget = collision.transform;
            // call player taking damage here (remember to do this)

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}