using UnityEngine;

public class BeatEmUpPlayerMovement : MonoBehaviour
{
    [Header("Gerakan Dasar")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpDuration = 0.8f; // durasi jump dalam detik

    [Header("Batas Arena")]
    [SerializeField] private float minY = -3f; // batas bawah arena
    [SerializeField] private float maxY = 3f;  // batas atas arena
    [SerializeField] private float minX = -8f; // batas kiri (opsional)
    [SerializeField] private float maxX = 8f;  // batas kanan (opsional)

    [Header("Serangan")]
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private float attackRange = 1f; // Define attack range

    [Header("Sistem Health")]
    [SerializeField] private float maxHealth = 100f;   // Maksimum darah pemain
    [SerializeField] private float damageAmount = 10f; // Jumlah damage yang diterima
    
    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 startScale;
    private float lastAttackTime;

    // Health system
    private float currentHealth;      // Darah saat ini
    private bool isDead = false;      // Status apakah pemain sudah mati

    // Jump system untuk beat em up
    private bool isJumping = false;
    private float jumpStartTime;
    private float groundY; // posisi Y saat mulai jump

    // Animator Parameter Hash
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
        
        // Inisialisasi health
        currentHealth = maxHealth;
        Debug.Log($"[PLAYER HEALTH] Health diinisialisasi: {currentHealth}/{maxHealth}");

        // Untuk beat em up, kita tidak perlu gravity
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        // Jangan izinkan input jika sedang dalam animasi serangan atau mati
        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") || isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        HandleMovement(); // Mengatur gerakan player
        HandleJump();     // Mengatur gerakan lompat
        HandleAttack();   // Mengatur serangan
        UpdateAnimations(); // Update status animasi
        
        // Debug: Tampilkan health setiap detik (opsional)
        if (Time.time % 5f < Time.deltaTime) // Setiap 5 detik
        {
            ShowHealthStatus();
        }
    }

    private void HandleMovement()
    {
        if (isJumping) return; // Jangan bergerak horizontal/vertikal saat sedang lompat

        // Input gerak horizontal dan vertikal
        float h = Input.GetAxisRaw("Horizontal");  // Arah horizontal (A/D atau Arrow Left/Right)
        float v = Input.GetAxisRaw("Vertical");    // Arah vertikal (jika diperlukan)

        // Batasi pergerakan dalam arena
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + h * moveSpeed * Time.deltaTime, minX, maxX); // Batasi X
        pos.y = Mathf.Clamp(pos.y + v * moveSpeed * Time.deltaTime, minY, maxY); // Batasi Y (opsional)
        transform.position = pos;

        // Flip sprite berdasarkan arah horizontal
        if (h > 0.01f) 
            transform.localScale = new Vector3(+Mathf.Abs(startScale.x), startScale.y, startScale.z);  // Menghadap kanan
        else if (h < -0.01f) 
            transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z);  // Menghadap kiri
    }

    private void HandleJump()
    {
        // Input lompat
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping) // Jika tekan tombol space
        {
            StartJump();  // Mulai lompat
            
            // Kombinasi: lompat sambil serang
            if (Input.GetKey(KeyCode.F)) 
                TriggerAttack();
        }

        // Update posisi saat lompat
        if (isJumping)
        {
            float jumpProgress = (Time.time - jumpStartTime) / jumpDuration;
            
            if (jumpProgress >= 1f)
            {
                EndJump();  // Selesaikan lompat
            }
            else
            {
                // Hitung ketinggian menggunakan fungsi parabola
                float jumpHeight = Mathf.Sin(jumpProgress * Mathf.PI) * jumpForce;
                Vector3 pos = transform.position;
                pos.y = groundY + jumpHeight;
                transform.position = pos;

                // Gerakan horizontal saat di udara (opsional)
                float h = Input.GetAxisRaw("Horizontal");
                if (h != 0)
                {
                    pos.x = Mathf.Clamp(pos.x + h * moveSpeed * 0.5f * Time.deltaTime, minX, maxX); // Kurangi kecepatan horizontal saat lompat
                    transform.position = pos;

                    // Update flip saat di udara
                    if (h > 0.01f) 
                        transform.localScale = new Vector3(+Mathf.Abs(startScale.x), startScale.y, startScale.z);  // Menghadap kanan
                    else if (h < -0.01f) 
                        transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z);  // Menghadap kiri
                }
            }
        }
    }

    private void StartJump()
    {
        isJumping = true;
        jumpStartTime = Time.time;
        groundY = transform.position.y; // Set posisi Y saat lompat
        anim.SetTrigger(JumpHash);
        anim.SetBool(IsJumpingHash, true);
    }

    private void EndJump()
    {
        isJumping = false;
        Vector3 pos = transform.position;
        pos.y = groundY; // Kembali ke posisi semula setelah lompat
        transform.position = pos;
        anim.SetBool(IsJumpingHash, false);
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= lastAttackTime + attackCooldown)
        {
            TriggerAttack(); // Memanggil animasi serangan
            // Detect collision with enemy
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.CompareTag("Musuh")) // Assuming "Enemy" tag is assigned to the enemy
                {
                    enemy.GetComponent<MovementMusuh>().TakeDamage(10f);  // Apply damage to enemy
                    Debug.Log("[PLAYER ATTACK] Musuh terkena damage!");
                    break;
                }
            }
        }
    }

    private void TriggerAttack()
    {
        anim.ResetTrigger(AttackHash);  // Reset trigger serangan agar bisa dipicu lagi
        anim.SetTrigger(AttackHash);    // Memanggil trigger animasi serangan
        lastAttackTime = Time.time;
    }

    private void UpdateAnimations()
    {
        if (!isJumping)
        {
            float h = Input.GetAxisRaw("Horizontal");
            bool isMoving = h != 0; // Cek jika pemain bergerak

            anim.SetBool(RunHash, isMoving); // Update animasi run (berjalan)
        }
    }

    // SISTEM HEALTH - Fungsi utama untuk menerima damage
    public void TakeDamage()
    {
        TakeDamage(damageAmount); // Gunakan default damage amount
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Jangan terima damage jika sudah mati

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth); // Pastikan health tidak negatif

        Debug.Log($"[PLAYER HEALTH] Pemain terkena damage {damage}! Health sekarang: {currentHealth}/{maxHealth}");

        // Trigger animasi hurt
        anim.SetTrigger(HurtHash);

        // Cek apakah pemain mati
        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            // Tampilkan status health dalam persentase
            float healthPercentage = (currentHealth / maxHealth) * 100f;
            Debug.Log($"[PLAYER HEALTH] Health tersisa: {healthPercentage:F1}%");
            
            // Warning jika health rendah
            if (healthPercentage <= 25f)
            {
                Debug.LogWarning("[PLAYER HEALTH] PERINGATAN! Health kritis!");
            }
        }
    }

    // Fungsi untuk menyembuhkan (heal)
    public void Heal(float healAmount)
    {
        if (isDead) return; // Jangan heal jika sudah mati

        float oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth); // Pastikan tidak melebihi max health

        float actualHealing = currentHealth - oldHealth;
        Debug.Log($"[PLAYER HEALTH] Pemain disembuhkan {actualHealing}! Health sekarang: {currentHealth}/{maxHealth}");
    }

    // Fungsi untuk menampilkan status health
    public void ShowHealthStatus()
    {
        float healthPercentage = (currentHealth / maxHealth) * 100f;
        Debug.Log($"[PLAYER HEALTH] Status Health: {currentHealth}/{maxHealth} ({healthPercentage:F1}%)");
    }

    // Fungsi untuk mendapatkan current health
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // Fungsi untuk mendapatkan max health
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    // Fungsi untuk mengecek apakah pemain masih hidup
    public bool IsAlive()
    {
        return !isDead;
    }

    // Fungsi untuk menangani kematian
    private void Die()
    {
        isDead = true;  // Set status mati menjadi true
        anim.SetTrigger(DeathHash);  // Memanggil animasi death
        
        Debug.Log("=== PLAYER DEATH ===");
        Debug.Log("[PLAYER HEALTH] Pemain telah mati!");
        Debug.Log("[GAME OVER] Game Over! Restart diperlukan.");
        Debug.Log("===================");

        // Hentikan semua gerakan
        rb.linearVelocity = Vector2.zero;

        // Logic untuk game over, misalnya restart level atau tampilkan layar game over
        StartCoroutine(HandleGameOver());
    }

    // Coroutine untuk menangani game over
    private System.Collections.IEnumerator HandleGameOver()
    {
        yield return new UnityEngine.WaitForSeconds(3f); // Tunggu 3 detik
        Debug.Log("[GAME OVER] Waktu restart otomatis...");
        
        // Opsi restart otomatis (uncomment jika diperlukan)
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // Fungsi untuk reset health (untuk testing atau restart)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        Debug.Log($"[PLAYER HEALTH] Health direset ke: {currentHealth}/{maxHealth}");
    }

    // Debug: Tambahkan kontrol keyboard untuk testing (H untuk heal, R untuk reset)
    private void OnEnable()
    {
        Debug.Log("[PLAYER HEALTH] Kontrol Debug:");
        Debug.Log("- Tekan H untuk heal +20");
        Debug.Log("- Tekan R untuk reset health");
        Debug.Log("- Tekan L untuk tampilkan status health");
    }

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(20f);
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetHealth();
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShowHealthStatus();
        }
    }

    private void LateUpdate()
    {
        HandleDebugInput(); // Handle debug input
    }
}