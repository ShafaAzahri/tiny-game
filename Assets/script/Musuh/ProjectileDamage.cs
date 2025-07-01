using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [Header("Proyektil Settings")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifeTime = 5f; // Berapa lama proyektil bertahan
    
    private void Start()
    {
        // Hancurkan proyektil setelah waktu tertentu
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah mengenai player
        if (other.CompareTag("Player"))
        {
            // Berikan damage ke player
            BeatEmUpPlayerMovement playerMovement = other.GetComponent<BeatEmUpPlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(damage);
            }

            // Hancurkan proyektil
            Destroy(gameObject);
        }
        // Cek apakah mengenai dinding atau obstacle
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // Hancurkan proyektil
            Destroy(gameObject);
        }
    }

    // Fungsi untuk mengatur damage dari luar
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    // Fungsi untuk mengatur lifetime proyektil
    public void SetLifeTime(float newLifeTime)
    {
        lifeTime = newLifeTime;
    }
}