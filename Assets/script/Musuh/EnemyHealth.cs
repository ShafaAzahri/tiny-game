using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead = false;

    [Header("Damage Settings")]
    [SerializeField] private float invulnerabilityTime = 0.5f; // Waktu kebal setelah terkena damage
    private float lastDamageTime;

    private Animator anim;
    private MovementMusuh enemyMovement;

    // Animator Parameter Hash
    private static readonly int HurtHash = Animator.StringToHash("hurt");
    private static readonly int DeathHash = Animator.StringToHash("death");

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyMovement = GetComponent<MovementMusuh>();
        currentHealth = maxHealth;
    }

    // Fungsi untuk menerima damage
    public void TakeDamage(float damage)
    {
        // Cek apakah musuh masih dalam waktu kebal atau sudah mati
        if (isDead || Time.time - lastDamageTime < invulnerabilityTime) 
            return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            // Memicu animasi hurt
            anim.SetTrigger(HurtHash);
            Debug.Log($"Musuh terkena damage {damage}! Health tersisa: {currentHealth}");
        }
    }

    // Fungsi untuk menangani kematian musuh
    private void Die()
    {
        isDead = true;
        anim.SetTrigger(DeathHash);
        Debug.Log("Musuh mati!");
        
        // Disable enemy movement
        if (enemyMovement != null)
            enemyMovement.enabled = false;

        // Destroy enemy setelah animasi death selesai (opsional)
        Destroy(gameObject, 2f);
    }

    // Getter untuk status
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}