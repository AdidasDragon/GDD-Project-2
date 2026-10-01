using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    public Transform playerTarget;
    private bool isMoving = false;
    float laserCooldown;

    [SerializeField] private GameObject Enemy_Laser_Prefab;
    [SerializeField] private Animator Enemy_animation;


    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 2.5f; // stop when within this distance
    [SerializeField] private float heightOffset = 2f; // stop when within this distance
    [SerializeField] public float health;
    [SerializeField] public float fireRate = 3f;
    [SerializeField] public float attackRange = 7f;

    #region Start_Update_Section
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        laserCooldown= 0f;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // cooldown timer goes down overtime
        if (laserCooldown > 0)
        {
            laserCooldown -= Time.deltaTime;
        }

        // shoot at player when in range and cooldown is 0
        if (canShoot())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= attackRange)
            {
                shootPlayer();
                laserCooldown = fireRate; // Reset cooldown
            }
        } 

        if (playerTarget==null)
        {
            Enemy_animation.SetBool("isAttacking", false);
        }
    }
    #endregion

    #region MovementFunctions 

    public IEnumerator MoveToPlayer()
    {
        // prevents multiple enumerator calls
        if (isMoving == true) yield break;
        isMoving = true;

        while (playerTarget != null)
        {
            Vector2 abovePosition = playerTarget.position + Vector3.up * heightOffset;
            float distance = Vector2.Distance(transform.position, abovePosition);

            // Stop if too close
            if (distance <= stopDistance)
                break;

            transform.position = Vector2.MoveTowards(
                transform.position,
                abovePosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        isMoving = false;
    }

    public void StopMoving()
    {
        StopAllCoroutines();
        isMoving = false;
    }
    #endregion

    #region attackFunctions
    void shootPlayer()
    {
        if (this.canShoot())
        {
            // spawn lazer at this (enemy) position
            Vector3 spawnPos = transform.position;
            Quaternion spawnRot = transform.rotation;
            Debug.Log("I see enemy, I can now shoot at them");
            //change animation to attacking animation
            Enemy_animation.SetBool("isAttacking", true);
            Instantiate(Enemy_Laser_Prefab, spawnPos, spawnRot);
        }
    }

    public bool canShoot()
    {
        return (playerTarget != null && laserCooldown <= 0);
    }
    #endregion

    #region HealthFunctions

    private void takeDamage(float amount)
    {
        this.health -= amount;

        // if this enemy has no health he dies!
        if (this.health <= 0)
        {
            Die();
        }
        // play enemy hurt animation if still alive
        Enemy_animation.SetTrigger("isHurt"); 
    }

    public void Die()
    {
        Enemy_animation.SetTrigger("isDying");
        StartCoroutine(WaitAndDestroy());
    }

    private IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(2); // match animation duration
        Destroy(gameObject);
    }
    #endregion
}