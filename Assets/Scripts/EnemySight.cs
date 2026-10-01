using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private bool moving;
    private EnemyController parentScript;

    void Start()
    {
        moving = false;
    }

    void Update()
    {
        // Continuously keeps movement active while player is in range if they step outside stopDistance
        if (parentScript != null && moving)
        {
            parentScript.StartCoroutine(parentScript.MoveToPlayer());
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Detect player entering trigger
        if (collision.CompareTag("Player") && !moving)
        {
            parentScript = GetComponentInParent<EnemyController>();
            parentScript.playerTarget = collision.transform;
            moving = true; // Prevent multiple triggers
            Debug.Log("Enemy sees player");
            parentScript.StartCoroutine(parentScript.MoveToPlayer());
            
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // Detect player leaving trigger
        if (collision.CompareTag("Player"))
        {
            moving = false;
            if (parentScript != null)
            {
                parentScript.StopMoving();
                parentScript.playerTarget = null;
            }
            parentScript = null;
        }
    }
}
