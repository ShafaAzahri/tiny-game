using UnityEngine;

public class MovementMusuh : MonoBehaviour
{
    [Header("Gerakan Dasar")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private float attackRange = 2f; // Jarak serang ke pemain

    [Header("Batas Arena")]
    [SerializeField] private float minY = -3f; // batas bawah arena
    [SerializeField] private float maxY = 3f;  // batas atas arena
    [SerializeField] private float minX = -8f; // batas kiri (opsional)
    [SerializeField] private float maxX = 8f;  // batas kanan (opsional)

    [Header("Sistem Health Musuh")]
    [SerializeField] private float maxHealth = 50f;   // Maksimum darah musuh
    [SerializeField] private float attackDamage = 10f; // Damage yang diberikan ke player

    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 startScale;
    private float lastAttackTime;

    // Health system
    private float currentHealth;      // Darah saat ini
    private bool isDead = false;      // Status apakah musuh sudah mati

    // Parameter Animator Hash
    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int AttackHash = Animator.StringToHash("attack");
    private static readonly int HurtHash = Animator.StringToHash("hurt");
    private static readonly int DeathHash = Animator.StringToHash("death");

    private Transform player;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startScale = transform.localScale;

        // Inisialisasi health
        currentHealth = maxHealth;
        Debug.Log($"[ENEMY HEALTH] {gameObject.name} health diinisialisasi: {currentHealth}/{maxHealth}");

        player = GameObject.FindWithTag("Player").transform; // Mengambil player berdasarkan tag
        mainCamera = Camera.main; // Mengambil referensi kamera utama

        // Atur gravitasi ke 0 karena tidak diperlukan untuk gerakan horizontal/vertikal saja
        rb.gravityScale = 0f;

        // Set Rigidbody2D ke Kinematic agar tidak ada dorongan fisika
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Update()
    {
        // Jangan lakukan apapun jika musuh sudah mati
        if (isDead) return;

        // Cek apakah musuh berada dalam jangkauan tampilan kamera
        if (IsPlayerInView())
        {
            // Jika pemain berada dalam frame kamera, musuh akan mengejar
            ChasePlayer();
        }
        else
        {
            // Jika pemain tidak terlihat, musuh berhenti bergerak atau bergerak ke posisi default
            StopMoving();
        }
    }

    // Fungsi untuk mengecek apakah musuh berada dalam jangkauan kamera (viewport)
    private bool IsPlayerInView()
    {
        // Menghitung posisi pemain dalam ruang tampilan kamera (viewport)
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.position);

        // Memeriksa apakah pemain berada dalam jangkauan tampilan kamera
        return viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;
    }

    private void ChasePlayer()
    {
        // Tentukan jarak antara musuh dan pemain
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Jika jarak ke pemain lebih besar dari jangkauan serang, musuh akan mengejar
        if (distanceToPlayer > attackRange)
        {
            // Tentukan arah menuju pemain
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;  // Musuh bergerak ke arah pemain

            // Gerakan vertikal (atas/bawah) agar musuh bisa bergerak di arah Y
            float verticalDirection = player.position.y - transform.position.y; // Hitung jarak vertikal
            if (Mathf.Abs(verticalDirection) > 0.1f) // Cek apakah musuh harus bergerak ke atas/bawah
            {
                float verticalMove = verticalDirection > 0 ? 1 : -1; // Tentukan arah atas/bawah
                Vector3 pos = transform.position;
                // Batasi pergerakan vertikal di antara minY dan maxY
                pos.y = Mathf.Clamp(pos.y + verticalMove * moveSpeed * Time.deltaTime, minY, maxY);
                transform.position = pos;
            }

            // Flip sprite berdasarkan arah horizontal (kanan/kiri)
            if (direction.x > 0.01f && transform.localScale.x < 0)
            {
                // Arahkan musuh ke kanan
                transform.localScale = new Vector3(Mathf.Abs(startScale.x), startScale.y, startScale.z);  // Menghadap kanan
            }
            else if (direction.x < -0.01f && transform.localScale.x > 0)
            {
                // Arahkan musuh ke kiri
                transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z); // Menghadap kiri
            }

            // Set animasi gerak
            anim.SetBool("isMoving", true);  // Pastikan parameter "isMoving" ada di Animator
        }
        else
        {
            // Jika sudah cukup dekat dengan pemain, berhenti bergerak dan serang
            StopMoving();
        }
    }

    private void StopMoving()
    {
        // Berhenti bergerak
        rb.linearVelocity = Vector2.zero;  // Menghentikan gerakan musuh

        // Set animasi untuk berhenti bergerak
        anim.SetBool("isMoving", false);  // Set parameter "isMoving" ke false saat berhenti

        // Serang pemain
        AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            anim.SetTrigger("attack");  // Trigger animasi serangan

            Debug.Log($"[ENEMY ATTACK] {gameObject.name} menyerang pemain!");

            // Deteksi tabrakan jika musuh menyerang pemain
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D playerCollider in hitPlayers)
            {
                if (playerCollider.CompareTag("Player"))
                {
                    // Ambil komponen BeatEmUpPlayerMovement bukan PlayerHealth
                    BeatEmUpPlayerMovement playerMovement = playerCollider.GetComponent<BeatEmUpPlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.TakeDamage(attackDamage);  // Berikan damage ke pemain
                        Debug.Log($"[ENEMY ATTACK] Pemain terkena damage {attackDamage} dari {gameObject.name}!");
                    }
                    break;  // Berhenti mencari setelah menemukan pemain
                }
            }

            lastAttackTime = Time.time;
        }
    }

    // SISTEM HEALTH MUSUH
    public void TakeDamage(float damage)
    {
        if (isDead) return; // Jangan terima damage jika sudah mati

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth); // Pastikan health tidak negatif

        Debug.Log($"[ENEMY HEALTH] {gameObject.name} terkena damage {damage}! Health sekarang: {currentHealth}/{maxHealth}");

        // Trigger animasi hurt
        anim.SetTrigger(HurtHash);

        // Cek apakah musuh mati
        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            // Tampilkan status health dalam persentase
            float healthPercentage = (currentHealth / maxHealth) * 100f;
            Debug.Log($"[ENEMY HEALTH] {gameObject.name} health tersisa: {healthPercentage:F1}%");
            
            // Warning jika health rendah
            if (healthPercentage <= 30f)
            {
                Debug.LogWarning($"[ENEMY HEALTH] {gameObject.name} health kritis!");
            }
        }
    }

    // Fungsi untuk menyembuhkan musuh (jika diperlukan)
    public void Heal(float healAmount)
    {
        if (isDead) return; // Jangan heal jika sudah mati

        float oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth); // Pastikan tidak melebihi max health

        float actualHealing = currentHealth - oldHealth;
        Debug.Log($"[ENEMY HEALTH] {gameObject.name} disembuhkan {actualHealing}! Health sekarang: {currentHealth}/{maxHealth}");
    }

    // Fungsi untuk menampilkan status health musuh
    public void ShowHealthStatus()
    {
        float healthPercentage = (currentHealth / maxHealth) * 100f;
        Debug.Log($"[ENEMY HEALTH] {gameObject.name} Status Health: {currentHealth}/{maxHealth} ({healthPercentage:F1}%)");
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

    // Fungsi untuk mengecek apakah musuh masih hidup
    public bool IsAlive()
    {
        return !isDead;
    }

    // Fungsi untuk menangani kematian musuh
    private void Die()
    {
        isDead = true;  // Set status mati menjadi true
        
        Debug.Log("=== KEMATIAN MUSUH ===");
        Debug.Log($"[HEALTH MUSUH] {gameObject.name} telah mati!");
        Debug.Log("==================");

        // Hentikan semua gerakan
        rb.linearVelocity = Vector2.zero;

        // Trigger animasi death (jika ada)
        if (anim != null)
        {
            anim.SetTrigger(DeathHash); // Memicu animasi death
        }

        // Nonaktifkan collider agar tidak bisa diserang lagi
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Hancurkan musuh setelah delay (opsional)
        StartCoroutine(HandleDeath());
    }


    // Coroutine untuk menangani kematian
    private System.Collections.IEnumerator HandleDeath()
    {
        yield return new UnityEngine.WaitForSeconds(2f); // Tunggu 2 detik
        
        Debug.Log($"[ENEMY DEATH] {gameObject.name} akan dihancurkan...");
        
        // Hancurkan GameObject musuh
        Destroy(gameObject);
    }

    // Fungsi untuk reset health musuh (untuk testing)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        
        // Aktifkan kembali collider
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
        
        Debug.Log($"[ENEMY HEALTH] {gameObject.name} health direset ke: {currentHealth}/{maxHealth}");
    }

    // Fungsi untuk mendapatkan info musuh (debugging)
    public void GetEnemyInfo()
    {
        Debug.Log($"=== {gameObject.name.ToUpper()} INFO ===");
        Debug.Log($"Health: {currentHealth}/{maxHealth}");
        Debug.Log($"Attack Damage: {attackDamage}");
        Debug.Log($"Move Speed: {moveSpeed}");
        Debug.Log($"Attack Range: {attackRange}");
        Debug.Log($"Status: {(isDead ? "MATI" : "HIDUP")}");
        Debug.Log("================================");
    }

    // Visualisasi attack range di Scene view (debugging)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}