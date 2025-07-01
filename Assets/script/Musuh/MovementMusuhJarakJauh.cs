using UnityEngine;
using System.Collections;

public class MovementMusuhJarakJauh : MonoBehaviour
{
    [Header("Movement & Combat")]
    [SerializeField] private float moveSpeed = 5f;               // Movement speed (not used in this case but could be useful if needed)
    [SerializeField] private float attackCooldown = 0.25f;       // Attack cooldown
    [SerializeField] private float attackRange = 8f;             // Range of the attack (used for projectile range)
    [SerializeField] private float minChaseDistance = 2f;        // Min distance to start chasing (not used for stationary enemy)
    
    [Header("Projectile System")]
    [SerializeField] private GameObject projectilePrefab;        // The projectile prefab that will be shot
    [SerializeField] private float projectileSpeed = 10f;        // The speed of the projectile
    [SerializeField] private float projectileDamage = 10f;       // Damage dealt by the projectile
    
    [Header("Health System")]
    [SerializeField] private float maxHealth = 50f;             // Max health of the enemy
    
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private Camera mainCamera;
    private Vector3 startScale;
    private float lastAttackTime;
    private float currentHealth;
    private bool isDead = false;
    
    // Animator parameters
    private static readonly int HurtHash = Animator.StringToHash("hurt");
    private static readonly int DeathHash = Animator.StringToHash("death");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startScale = transform.localScale;
        currentHealth = maxHealth;
        
        player = GameObject.FindWithTag("Player")?.transform;
        mainCamera = Camera.main;
        
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // We don't want the enemy moving physically, as it's stationary
    }

    private void Update()
    {
        if (isDead || player == null) return;
        
        // Attack only when the player is in the attack range
        if (IsPlayerInView() && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            StopMoving();
            AttackPlayer();
        }
    }

    private void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;  // Prevent any movement while in place
        anim?.SetBool("isMoving", false);  // Stop movement animation
    }

    private void AttackPlayer()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            anim?.SetTrigger("attack");   // Trigger attack animation
            StartCoroutine(ShootAfterDelay(0.5f));  // Shoot after a small delay
            lastAttackTime = Time.time;
        }
    }

    private IEnumerator ShootAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (projectilePrefab != null && player != null)
        {
            // Spawn the projectile
            Vector3 firePos = transform.position + Vector3.right * (0.5f * Mathf.Sign(transform.localScale.x));
            GameObject projectile = Instantiate(projectilePrefab, firePos, Quaternion.identity);

            // Add projectile component if it doesn't exist
            if (!projectile.GetComponent<EnemyProjectile>())
                projectile.AddComponent<EnemyProjectile>().Initialize(projectileDamage);

            Vector2 direction = (player.position - firePos).normalized;
            projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed; // Apply velocity to the projectile
        }
    }

    private bool IsPlayerInView()
    {
        if (player == null || mainCamera == null) return false;

        // Check if the player is within the view of the camera (on screen)
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.position);
        return viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        anim?.SetTrigger(HurtHash);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;   // Ensure it doesn't move after death
        anim?.SetTrigger(DeathHash);  // Trigger death animation

        GetComponent<Collider2D>().enabled = false;  // Disable the collider to prevent further interactions
        Destroy(gameObject, 2f);  // Destroy after the death animation ends
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        GetComponent<Collider2D>().enabled = true;  // Enable collider again
    }
}

// Enemy Projectile Component
public class EnemyProjectile : MonoBehaviour
{
    private float damage = 10f;
    private Camera mainCamera;

    public void Initialize(float projectileDamage)
    {
        damage = projectileDamage;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Destroy the projectile if it's out of camera view
        if (mainCamera != null && !IsInCameraView())
        {
            Destroy(gameObject);
        }
    }

    private bool IsInCameraView()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPos.x >= -0.1f && viewportPos.x <= 1.1f && viewportPos.y >= -0.1f && viewportPos.y <= 1.1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Projectile hit: " + other.name);

        if (other.CompareTag("Player"))
        {
            // Apply damage to the player via the projectile
            BeatEmUpPlayerMovement playerMovement = other.GetComponent<BeatEmUpPlayerMovement>();
            if (playerMovement != null)
            {
                Debug.Log("Damage applied to player via projectile");
                playerMovement.TakeDamage(damage);  // Apply damage directly via projectile
            }
            else
            {
                Debug.LogWarning("BeatEmUpPlayerMovement component not found on player.");
            }

            Destroy(gameObject);  // Destroy the projectile after it hits the player
        }
    }
}
