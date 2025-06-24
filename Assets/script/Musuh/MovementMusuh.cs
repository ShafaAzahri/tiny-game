using UnityEngine;

public class MovementMusuh : MonoBehaviour
{
    [Header("Gerakan Dasar")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private float attackRange = 2f; // Jarak serang ke pemain

    [Header("Sistem Jarak X (Beat Em Up Style)")]
    [SerializeField] private float maxXDistance = 6f; // Jarak maksimum di sumbu X sebelum berhenti mengejar
    [SerializeField] private bool useXDistanceLimit = true; // Toggle untuk mengaktifkan/nonaktifkan fitur ini
    [SerializeField] private bool showXDistanceGizmo = true; // Tampilkan gizmo untuk debugging

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

    // Variables untuk sistem X Distance
    private bool isWithinXRange = true; // Apakah masih dalam jangkauan X yang diizinkan

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
            // Cek jarak X jika fitur diaktifkan
            if (useXDistanceLimit)
            {
                CheckXDistance();
            }

            // Jika pemain berada dalam frame kamera dan dalam jangkauan X, musuh akan mengejar
            if (isWithinXRange || !useXDistanceLimit)
            {
                ChasePlayer();
            }
            else
            {
                // Jika terlalu jauh di sumbu X, berhenti mengejar
                StopChasingDueToXDistance();
            }
        }
        else
        {
            // Jika pemain tidak terlihat, musuh berhenti bergerak
            StopMoving();
        }
    }

    // Fungsi untuk mengecek jarak di sumbu X
    private void CheckXDistance()
    {
        float xDistance = Mathf.Abs(transform.position.x - player.position.x);
        isWithinXRange = xDistance <= maxXDistance;

        // Debug log untuk monitoring
        if (!isWithinXRange)
        {
            Debug.Log($"[X DISTANCE] {gameObject.name} terlalu jauh dari player di sumbu X! Jarak: {xDistance:F2}, Max: {maxXDistance}");
        }
    }

    // Fungsi baru untuk menangani berhenti mengejar karena jarak X
    private void StopChasingDueToXDistance()
    {
        // Berhenti bergerak
        rb.linearVelocity = Vector2.zero;

        // Set animasi untuk berhenti bergerak
        if (anim != null)
        {
            anim.SetBool("isMoving", false);
        }

        // Musuh akan menghadap ke arah player meski tidak mengejar
        FacePlayer();

        Debug.Log($"[X DISTANCE] {gameObject.name} berhenti mengejar karena jarak X terlalu jauh");
    }

    // Fungsi untuk menghadap player tanpa bergerak
    private void FacePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Flip sprite berdasarkan arah horizontal (kanan/kiri)
        if (direction.x > 0.01f && transform.localScale.x < 0)
        {
            // Arahkan musuh ke kanan
            transform.localScale = new Vector3(Mathf.Abs(startScale.x), startScale.y, startScale.z);
        }
        else if (direction.x < -0.01f && transform.localScale.x > 0)
        {
            // Arahkan musuh ke kiri
            transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z);
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
            if (anim != null)
            {
                anim.SetBool("isMoving", true);  // Pastikan parameter "isMoving" ada di Animator
            }
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
        if (anim != null)
        {
            anim.SetBool("isMoving", false);  // Set parameter "isMoving" ke false saat berhenti
        }

        // Serang pemain
        AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            if (anim != null)
            {
                anim.SetTrigger("attack");  // Trigger animasi serangan
            }

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
        if (anim != null)
        {
            anim.SetTrigger(HurtHash);
        }

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

    // FUNGSI BARU UNTUK MENGATUR X DISTANCE
    
    // Fungsi untuk mengatur jarak X maksimum secara runtime
    public void SetMaxXDistance(float newMaxDistance)
    {
        maxXDistance = newMaxDistance;
        Debug.Log($"[X DISTANCE] {gameObject.name} jarak X maksimum diubah ke: {maxXDistance}");
    }

    // Fungsi untuk mendapatkan jarak X saat ini ke player
    public float GetCurrentXDistance()
    {
        if (player != null)
        {
            return Mathf.Abs(transform.position.x - player.position.x);
        }
        return 0f;
    }

    // Fungsi untuk mengaktifkan/nonaktifkan sistem X distance
    public void SetUseXDistanceLimit(bool useLimit)
    {
        useXDistanceLimit = useLimit;
        Debug.Log($"[X DISTANCE] {gameObject.name} sistem X distance limit: {(useLimit ? "AKTIF" : "NONAKTIF")}");
    }

    // Fungsi untuk mengecek apakah musuh dalam jangkauan X
    public bool IsWithinXRange()
    {
        return isWithinXRange;
    }

    // Fungsi untuk mendapatkan info musuh (debugging)
    public void GetEnemyInfo()
    {
        Debug.Log($"=== {gameObject.name.ToUpper()} INFO ===");
        Debug.Log($"Health: {currentHealth}/{maxHealth}");
        Debug.Log($"Attack Damage: {attackDamage}");
        Debug.Log($"Move Speed: {moveSpeed}");
        Debug.Log($"Attack Range: {attackRange}");
        Debug.Log($"Max X Distance: {maxXDistance}");
        Debug.Log($"Current X Distance: {GetCurrentXDistance():F2}");
        Debug.Log($"Within X Range: {isWithinXRange}");
        Debug.Log($"Status: {(isDead ? "MATI" : "HIDUP")}");
        Debug.Log("================================");
    }

    // Visualisasi attack range dan X distance di Scene view (debugging)
    private void OnDrawGizmosSelected()
    {
        // Attack range (merah)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // X Distance limit (hijau) - hanya jika fitur diaktifkan dan ada player
        if (useXDistanceLimit && showXDistanceGizmo && player != null)
        {
            Gizmos.color = Color.green;
            
            // Garis horizontal dari posisi enemy sejauh maxXDistance ke kiri dan kanan
            Vector3 leftLimit = new Vector3(transform.position.x - maxXDistance, transform.position.y, transform.position.z);
            Vector3 rightLimit = new Vector3(transform.position.x + maxXDistance, transform.position.y, transform.position.z);
            
            Gizmos.DrawLine(leftLimit, rightLimit);
            
            // Garis vertikal di batas kiri dan kanan
            Gizmos.DrawLine(leftLimit + Vector3.up * 0.5f, leftLimit - Vector3.up * 0.5f);
            Gizmos.DrawLine(rightLimit + Vector3.up * 0.5f, rightLimit - Vector3.up * 0.5f);
            
            // Jika player ada, tampilkan garis dari enemy ke player untuk visualisasi jarak
            if (player != null)
            {
                float currentXDistance = Mathf.Abs(transform.position.x - player.position.x);
                
                // Jika dalam jangkauan, warna biru. Jika tidak, warna kuning
                Gizmos.color = isWithinXRange ? Color.blue : Color.yellow;
                
                // Garis horizontal dari enemy ke player (hanya sumbu X)
                Vector3 playerXPos = new Vector3(player.position.x, transform.position.y, transform.position.z);
                Gizmos.DrawLine(transform.position, playerXPos);
                
                // Label jarak (tidak bisa ditampilkan dengan Gizmos, tapi bisa dilihat di debug log)
            }
        }
    }
}