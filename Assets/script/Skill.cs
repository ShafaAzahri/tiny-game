// // Script pada Skill (misalnya PinkLightning)
// using UnityEngine;

// public class Skill : MonoBehaviour
// {
//     [SerializeField] private float damage = 20f;  // Damage yang diberikan skill
//     [SerializeField] private float destroyDelay = 0.5f;  // Waktu tunggu sebelum skill dihancurkan

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         // Cek jika terkena musuh
//         if (other.CompareTag("Musuh"))
//         {
//             // Mengurangi HP musuh
//             other.GetComponent<MovementMusuh>()?.TakeDamage(damage);
//         }
//     }

//     private void Start()
//     {
//         // Hancurkan skill setelah beberapa waktu sesuai dengan durasi animasi
//         Destroy(gameObject, destroyDelay);
//     }
// }
