using UnityEngine;
using TMPro;

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
    [SerializeField] private int maxLives = 3; // Jumlah nyawa maksimal
    [SerializeField] private int hitsToLoseLife = 5; // Jumlah hit untuk mengurangi nyawa

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI livesText; // Text untuk menampilkan nyawa
    [SerializeField] private TextMeshProUGUI hitCounterText; // Text untuk menampilkan hit counter (opsional)
    [SerializeField] private GameObject skillUI; // UI skill icon/indicator
    [SerializeField] private float skillCooldownTime = 2f; // Cooldown skill dalam detik

    [Header("Skill")]
    [SerializeField] private GameObject skillPrefab;  // Prefab skill
    [SerializeField] private Transform skillSpawnPoint;  // Titik spawn skill

    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 startScale;
    private float lastAttackTime;
    private float currentHealth;
    private bool isDead = false;
    private bool isJumping = false;
    private float jumpStartTime;
    private float groundY;

    // Hit counter variables
    private int currentLives;
    private int hitCounter = 0;
    
    // Skill variables
    private float lastSkillTime = -999f; // Waktu terakhir skill digunakan
    private bool isSkillReady = true;

    private Vector2 movementInput;

    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int AttackHash = Animator.StringToHash("attack");
    private static readonly int SkillTriggerHash = Animator.StringToHash("SkillTrigger");
    private static readonly int HurtHash = Animator.StringToHash("hurt");  // Animator hash untuk hurt

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startScale = transform.localScale;
        currentHealth = maxHealth;
        currentLives = maxLives;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        UpdateUI();
    }

    private void Update()
    {
        // Debug untuk status player
        if (isDead)
        {
            Debug.LogWarning("Player is DEAD - No movement allowed");
            return;
        }

        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        HandleJump();
        HandleAttack();
        HandleSkill();
        UpdateSkillCooldown();
        UpdateSkillUI();
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
    }

    private void EndJump()
    {
        isJumping = false;
        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
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
                    enemy.GetComponent<MovementMusuh>()?.TakeDamage(damageAmount);
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

    private void HandleSkill()
    {
        if (Input.GetKeyDown(KeyCode.G) && isSkillReady && Time.time >= lastAttackTime + attackCooldown)
        {
            TriggerSkill();
            SpawnSkill();
            lastSkillTime = Time.time;
            isSkillReady = false;
            UpdateSkillUI(); // Update UI langsung setelah skill digunakan
        }
    }

    private void TriggerSkill()
    {
        anim.SetTrigger(SkillTriggerHash);
        lastAttackTime = Time.time;
    }

    private void SpawnSkill()
    {
        Vector3 spawnPosition = transform.position + new Vector3(1.5f, 0, 0);
        
        if (skillPrefab != null)
        {
            GameObject skill = Instantiate(skillPrefab, spawnPosition, Quaternion.identity);
            Destroy(skill, skill.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);  // Destroy skill after animation ends

            // Check collision with enemies
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(skill.transform.position, attackRange);
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.CompareTag("Musuh"))
                {
                    enemy.GetComponent<MovementMusuh>()?.TakeDamage(damageAmount);  // Apply damage to enemy
                    break;
                }
            }
        }
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

        // Tambah hit counter
        hitCounter++;

        // Pastikan animasi hurt dipanggil hanya jika tidak sedang dalam animasi lain yang menghalangi
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("hurt"))
        {
            anim.SetTrigger(HurtHash);  // Animasi hurt dipanggil
        }

        // Cek apakah sudah mencapai jumlah hit untuk mengurangi nyawa
        if (hitCounter >= hitsToLoseLife)
        {
            LoseLife();
        }

        UpdateUI();

        if (currentHealth <= 0f) Die();
    }

    private void LoseLife()
    {
        currentLives--;
        hitCounter = 0; // Reset hit counter
        
        Debug.Log($"Nyawa berkurang! Sisa nyawa: {currentLives}");
        
        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            // Reset health ketika kehilangan nyawa (opsional)
            currentHealth = maxHealth;
        }
    }

    private void UpdateUI()
    {
        // Update teks nyawa
        if (livesText != null)
        {
            livesText.text = $"{currentLives}";
        }

        // Update hit counter (opsional)
        if (hitCounterText != null)
        {
            hitCounterText.text = $"Hit: {hitCounter}/{hitsToLoseLife}";
        }
    }

    private void UpdateSkillCooldown()
    {
        // Cek apakah skill sudah ready setelah cooldown
        if (!isSkillReady && Time.time >= lastSkillTime + skillCooldownTime)
        {
            isSkillReady = true;
        }
    }

    private void UpdateSkillUI()
    {
        // Tampilkan/sembunyikan skill UI berdasarkan status
        if (skillUI != null)
        {
            skillUI.SetActive(isSkillReady);
        }
    }

    private void Die()
    {
        if (isDead) return; // Hindari pemanggilan berkali-kali

        Debug.Log("Player Die: Setting player to dead state");
        
        isDead = true;
        
        if (anim != null)
        {
            anim.SetTrigger("death");
        }
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }


    // Method untuk reset game atau respawn (opsional)
    public void ResetPlayer()
    {
        Debug.Log("ResetPlayer: Player reset complete");
        isDead = false;
        Debug.Log($"isDead set to: {isDead}");
        isJumping = false;
        currentLives = maxLives;
        hitCounter = 0;
        currentHealth = maxHealth;
        isSkillReady = true;
        lastSkillTime = -999f;
            // Reset animator
        if (anim != null)
        {
            Debug.Log("Resetting Animator");
            anim.Rebind(); 
            anim.Update(0f);
        }

        // Reset velocity dan movement
        if (rb != null)
        {
            Debug.Log("Resetting Rigidbody");
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        UpdateUI();
        UpdateSkillUI();
        Debug.Log("ResetPlayer: Player reset complete");
    }

    public void ForceReset()
    {
        Debug.Log("FORCE RESET PLAYER");
        ResetPlayer();
        isDead = false;
    }

    // Getter methods
    public float GetCurrentHealth() => currentHealth;
    public bool IsAlive() => !isDead;
    public int GetCurrentLives() => currentLives;
    public int GetHitCounter() => hitCounter;
    public bool IsSkillReady() => isSkillReady;
    public float GetSkillCooldownProgress() => isSkillReady ? 1f : Mathf.Clamp01((Time.time - lastSkillTime) / skillCooldownTime);


}