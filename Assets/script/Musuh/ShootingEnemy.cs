// using UnityEngine;

// public class ShootingEnemy : MonoBehaviour
// {
//     [Header("Sistem Tembak")]
//     [SerializeField] private float shootRange = 10f;  // Jarak tembak
//     [SerializeField] private float shootCooldown = 1f; // Waktu cooldown tembakan
//     [SerializeField] private GameObject bulletPrefab;  // Prefab peluru/proyektil
//     [SerializeField] private Transform shootPoint;     // Titik dari mana peluru ditembakkan

//     [Header("Sistem Health Musuh")]
//     [SerializeField] private float maxHealth = 50f;   // Maksimum darah musuh
//     [SerializeField] private float attackDamage = 10f; // Damage yang diberikan ke pemain

//     private float currentHealth; // Darah saat ini
//     private bool isDead = false; // Status apakah musuh sudah mati

//     private Transform player;   // Referensi ke pemain
//     private float lastShootTime; // Waktu tembakan terakhir

//     // Parameter Animator Hash
//     private static readonly int ShootHash = Animator.StringToHash("shoot");

//     private void Awake()
//     {
//         currentHealth = maxHealth;
//         player = GameObject.FindWithTag("Player").transform; // Menemukan player berdasarkan tag
//     }

//     private void Update()
//     {
//         if (isDead) return;

//         // Cek apakah pemain berada dalam jangkauan tembak
//         float distanceToPlayer = Vector2.Distance(transform.position, player.position);

//         // Jika pemain berada dalam jarak tembak dan cooldown sudah lewat
//         if (distanceToPlayer <= shootRange && Time.time - lastShootTime >= shootCooldown)
//         {
//             ShootAtPlayer();
//         }
//     }

//     private void ShootAtPlayer()
//     {
//         // Trigger animasi tembakan jika ada
//         if (anim != null)
//         {
//             anim.SetTrigger(ShootHash);
//         }

//         // Menembak peluru dari titik tertentu
//         if (bulletPrefab != null && shootPoint != null)
//         {
//             // Buat peluru dan arahkan ke player
//             GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
//             Vector2 direction = (player.position - shootPoint.position).normalized;

//             // Set arah dan kecepatan peluru
//             Bullet bulletScript = bullet.GetComponent<Bullet>();
//             if (bulletScript != null)
//             {
//                 bulletScript.SetDirection(direction);
//                 bulletScript.SetDamage(attackDamage);
//             }
//         }

//         // Update waktu tembakan terakhir
//         lastShootTime = Time.time;
//     }

//     // Fungsi untuk mengurangi health musuh
//     public void TakeDamage(float damage)
//     {
//         if (isDead) return;

//         currentHealth -= damage;
//         currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

//         if (currentHealth <= 0f)
//         {
//             Die();
//         }
//     }

//     // Fungsi untuk menangani kematian musuh
//     private void Die()
//     {
//         isDead = true;
//         // Musuh mati, hentikan semua aktivitas atau animasi
//         // Misalnya nonaktifkan komponen atau destroy objek setelah beberapa detik
//         Destroy(gameObject);
//     }
// }
