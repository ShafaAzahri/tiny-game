using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;  // Kecepatan peluru
    private Vector2 direction;  // Arah peluru

    private void Update()
    {
        // Gerakkan peluru ke arah yang telah ditentukan
        transform.Translate(direction * speed * Time.deltaTime);
    }

    // Fungsi untuk mengatur arah peluru
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
    }

    // Deteksi peluru mengenai pemain
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BeatEmUpPlayerMovement playerMovement = other.GetComponent<BeatEmUpPlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(10f);  // Ganti 10f dengan damage yang sesuai
            }

            // Hancurkan peluru setelah mengenai pemain
            Destroy(gameObject);
        }
    }
}
