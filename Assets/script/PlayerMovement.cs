using UnityEngine;
using TMPro;

public class BeatEmUpPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpDuration = 0.8f;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private float attackRange = 1f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private int maxLives = 3; 
    [SerializeField] private int hitsToLoseLife = 5; 

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI livesText; 
    [SerializeField] private TextMeshProUGUI hitCounterText; 
    [SerializeField] private GameObject skillUI; 
    [SerializeField] private float skillCooldownTime = 2f; 

    [Header("Skill Settings")]
    [SerializeField] private GameObject skillPrefab;  
    [SerializeField] private Transform skillSpawnPoint;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 startScale;
    private float lastAttackTime;
    private float currentHealth;
    private bool isDead = false;
    private bool isJumping = false;
    private float jumpStartTime;
    private float groundY;

    private int currentLives;
    private int hitCounter = 0;

    private float lastSkillTime = -999f; 
    private bool isSkillReady = true;

    private Vector2 movementInput;

    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int AttackHash = Animator.StringToHash("attack");
    private static readonly int SkillTriggerHash = Animator.StringToHash("SkillTrigger");
    private static readonly int HurtHash = Animator.StringToHash("hurt");

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
        if (isDead) return;

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
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);

        if (movementInput.x != 0)
        {
            FlipSprite(movementInput.x);
        }
    }

    private void FlipSprite(float direction)
    {
        transform.localScale = new Vector3(Mathf.Sign(direction) * Mathf.Abs(startScale.x), startScale.y, startScale.z);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartJump();
        }

        if (isJumping)
        {
            HandleJumping();
        }
    }

    private void StartJump()
    {
        isJumping = true;
        jumpStartTime = Time.time;
        groundY = transform.position.y;
        anim.SetTrigger(JumpHash);
    }

    private void HandleJumping()
    {
        float jumpProgress = (Time.time - jumpStartTime) / jumpDuration;

        if (jumpProgress >= 1f)
        {
            EndJump();
        }
        else
        {
            float jumpHeight = Mathf.Sin(jumpProgress * Mathf.PI) * jumpForce;
            transform.position = new Vector3(transform.position.x, groundY + jumpHeight, transform.position.z);
        }
    }

    private void EndJump()
    {
        isJumping = false;
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
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
            UpdateSkillUI();
        }
    }

    private void TriggerSkill()
    {
        anim.SetTrigger(SkillTriggerHash);
        lastAttackTime = Time.time;
    }

    private void SpawnSkill()
    {
        if (skillPrefab == null) return;

        Vector3 spawnPosition = transform.position + new Vector3(1.5f, 0, 0);
        GameObject skill = Instantiate(skillPrefab, spawnPosition, Quaternion.identity);

        Destroy(skill, skill.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(skill.transform.position, attackRange);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Musuh"))
            {
                enemy.GetComponent<MovementMusuh>()?.TakeDamage(damageAmount);
                break;
            }
        }
    }

    private void UpdateAnimations()
    {
        if (!isJumping)
        {
            anim.SetBool(RunHash, movementInput != Vector2.zero);
        }
    }

    public void TakeDamage(float damage = 0)
    {
        if (isDead) return;

        damage = damage > 0 ? damage : damageAmount;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);

        hitCounter++;

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("hurt"))
        {
            anim.SetTrigger(HurtHash);
        }

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
        hitCounter = 0;

        Debug.Log($"Nyawa berkurang! Sisa nyawa: {currentLives}");

        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    private void UpdateUI()
    {
        if (livesText != null)
        {
            livesText.text = $"{currentLives}";
        }

        if (hitCounterText != null)
        {
            hitCounterText.text = $"Hit: {hitCounter}/{hitsToLoseLife}";
        }
    }

    private void UpdateSkillCooldown()
    {
        if (!isSkillReady && Time.time >= lastSkillTime + skillCooldownTime)
        {
            isSkillReady = true;
        }
    }

    private void UpdateSkillUI()
    {
        if (skillUI != null)
        {
            skillUI.SetActive(isSkillReady);
        }
    }

    private void Die()
    {
        if (isDead) return;

        Debug.Log("Player Die: Setting player to dead state");

        isDead = true;
        anim.SetTrigger("death");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void ResetPlayer()
    {
        isDead = false;
        isJumping = false;
        currentLives = maxLives;
        hitCounter = 0;
        currentHealth = maxHealth;
        isSkillReady = true;
        lastSkillTime = -999f;

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        UpdateUI();
        UpdateSkillUI();
    }

    public void ForceReset()
    {
        ResetPlayer();
    }

    // Getter methods
    public float GetCurrentHealth() => currentHealth;
    public bool IsAlive() => !isDead;
    public int GetCurrentLives() => currentLives;
    public int GetHitCounter() => hitCounter;
    public bool IsSkillReady() => isSkillReady;
    public float GetSkillCooldownProgress() => isSkillReady ? 1f : Mathf.Clamp01((Time.time - lastSkillTime) / skillCooldownTime);
}
