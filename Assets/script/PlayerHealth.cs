using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead = false;

    [Header("Damage Settings")]
    [SerializeField] private float invulnerabilityTime = 1f; // Waktu kebal setelah terkena damage
    private float lastDamageTime;

    private Animator anim;
    private BeatEmUpPlayerMovement playerMovement;

    // Animator Parameter Hash
    private static readonly int HurtHash = Animator.StringToHash("hurt");
    private static readonly int DeathHash = Animator.StringToHash("death");

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<BeatEmUpPlayerMovement>();
        currentHealth = maxHealth;
    }

    // Fungsi untuk menerima damage
    public void TakeDamage(float damage)
    {
        // Cek apakah player masih dalam waktu kebal
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
            Debug.Log($"Player terkena damage {damage}! Health tersisa: {currentHealth}");
        }
    }

    // Fungsi untuk menangani kematian player
    private void Die()
    {
        isDead = true;
        anim.SetTrigger(DeathHash);
        Debug.Log("Player mati!");
        
        // Disable player movement
        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    // Fungsi untuk heal player (opsional)
    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log($"Player heal {healAmount}! Health sekarang: {currentHealth}");
    }

    // Getter untuk status
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}