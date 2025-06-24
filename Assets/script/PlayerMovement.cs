using UnityEngine;

public class BeatEmUpPlayerMovement : MonoBehaviour
{
    [Header("Movement")]    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpDuration = 0.8f;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private float attackRange = 1f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damageAmount = 10f;
    
    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 startScale;
    private float lastAttackTime;
    private float currentHealth;
    private bool isDead = false;
    private bool isJumping = false;
    private float jumpStartTime;
    private float groundY;

    private Vector2 movementInput;

    // Animator Hashes
    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int AttackHash = Animator.StringToHash("attack");
    private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
    private static readonly int HurtHash = Animator.StringToHash("hurt");
    private static readonly int DeathHash = Animator.StringToHash("death");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startScale = transform.localScale;
        currentHealth = maxHealth;
        rb.gravityScale = 0f; // Disable gravity for 2.5D movement
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (isDead || anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        HandleJump();
        HandleAttack();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!isJumping && !isDead)
        {
            rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);

            // Flip sprite
            if (movementInput.x > 0.01f)
                transform.localScale = new Vector3(Mathf.Abs(startScale.x), startScale.y, startScale.z);
            else if (movementInput.x < -0.01f)
                transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z);
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartJump();
            if (Input.GetKey(KeyCode.F)) TriggerAttack();
        }

        if (isJumping)
        {
            float jumpProgress = (Time.time - jumpStartTime) / jumpDuration;

            if (jumpProgress >= 1f)
            {
                EndJump();
            }
            else
            {
                float jumpHeight = Mathf.Sin(jumpProgress * Mathf.PI) * jumpForce;
                Vector3 pos = transform.position;
                pos.y = groundY + jumpHeight;

                // Air movement (x axis only)
                float h = Input.GetAxisRaw("Horizontal");
                if (h != 0)
                {
                    pos.x += h * moveSpeed * 0.5f * Time.deltaTime;
                    transform.localScale = new Vector3(h > 0 ? Mathf.Abs(startScale.x) : -Mathf.Abs(startScale.x), startScale.y, startScale.z);
                }

                transform.position = pos;
            }
        }
    }

    private void StartJump()
    {
        isJumping = true;
        jumpStartTime = Time.time;
        groundY = transform.position.y;
        anim.SetTrigger(JumpHash);
        anim.SetBool(IsJumpingHash, true);
    }

    private void EndJump()
    {
        isJumping = false;
        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
        anim.SetBool(IsJumpingHash, false);
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= lastAttackTime + attackCooldown)
        {
            TriggerAttack();

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.CompareTag("Musuh"))
                {
                    enemy.GetComponent<MovementMusuh>()?.TakeDamage(10f);
                    break;
                }
            }
        }
    }

    private void TriggerAttack()
    {
        anim.SetTrigger(AttackHash);
        lastAttackTime = Time.time;
    }

    private void UpdateAnimations()
    {
        if (!isJumping)
        {
            bool isMoving = movementInput != Vector2.zero;
            anim.SetBool(RunHash, isMoving);
        }
    }

    public void TakeDamage(float damage = 0)
    {
        if (isDead) return;

        damage = damage > 0 ? damage : damageAmount;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);

        anim.SetTrigger(HurtHash);

        if (currentHealth <= 0f) Die();
    }

    public void Heal(float healAmount)
    {
        if (!isDead)
            currentHealth = Mathf.Clamp(currentHealth + healAmount, 0f, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        anim.SetTrigger(DeathHash);
        rb.linearVelocity = Vector2.zero;
        StartCoroutine(HandleGameOver());
    }

    private System.Collections.IEnumerator HandleGameOver()
    {
        yield return new WaitForSeconds(3f);
        // Restart logic here if needed
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsAlive() => !isDead;
    public void ResetHealth() { currentHealth = maxHealth; isDead = false; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject obj = collision.gameObject;

        Debug.Log("Menabrak objek: " + obj.name);

        // Contoh logika saat nabrak objek apapun yang punya collider:
        rb.linearVelocity = Vector2.zero;
        
    }

    private void OnDrawGizmosSelected()
    {
        // Menampilkan radius serangan saat di editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
