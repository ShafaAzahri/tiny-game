// using UnityEngine;

// public class MovementMusuh : MonoBehaviour
// {
//     [Header("Gerakan Dasar")]
//     [SerializeField] private float moveSpeed = 5f;
//     [SerializeField] private float attackCooldown = 0.25f;
//     [SerializeField] private float attackRange = 2f; // Jarak serang ke pemain

//     [Header("Batas Arena")]
//     [SerializeField] private float minY = -3f; // batas bawah arena
//     [SerializeField] private float maxY = 3f;  // batas atas arena
//     [SerializeField] private float minX = -8f; // batas kiri (opsional)
//     [SerializeField] private float maxX = 8f;  // batas kanan (opsional)

//     private Rigidbody2D rb;
//     private Animator anim;
//     private Vector3 startScale;
//     private float lastAttackTime;

//     // Parameter Animator Hash
//     private static readonly int RunHash = Animator.StringToHash("run");
//     private static readonly int AttackHash = Animator.StringToHash("attack");
//     private static readonly int HurtHash = Animator.StringToHash("hurt");

//     private Transform player;
//     private Camera mainCamera;

//     private void Awake()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         anim = GetComponent<Animator>();
//         startScale = transform.localScale;

//         player = GameObject.FindWithTag("Player").transform; // Mengambil player berdasarkan tag
//         mainCamera = Camera.main; // Mengambil referensi kamera utama

//         // Atur gravitasi ke 0 karena tidak diperlukan untuk gerakan horizontal/vertikal saja
//         rb.gravityScale = 0f;

//         // Set Rigidbody2D ke Kinematic agar tidak ada dorongan fisika
//         rb.bodyType = RigidbodyType2D.Kinematic;
//     }

//     private void Update()
//     {
//         // Cek apakah pemain terlihat oleh kamera
//         if (IsPlayerInView())
//         {
//             // Jika pemain terlihat, musuh akan mengejar
//             ChasePlayer();
//         }
//         else
//         {
//             // Jika pemain tidak terlihat, musuh berhenti bergerak atau bergerak ke posisi default
//             StopMoving();
//         }
//     }

//     private bool IsPlayerInView()
//     {
//         // Menghitung posisi pemain dalam ruang tampilan kamera
//         Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.position);

//         // Memeriksa apakah pemain berada dalam jangkauan tampilan kamera
//         return viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;
//     }

//     private void ChasePlayer()
//     {
//         // Tentukan jarak antara musuh dan pemain
//         float distanceToPlayer = Vector2.Distance(transform.position, player.position);

//         // Jika jarak ke pemain lebih besar dari jangkauan serang, musuh akan mengejar
//         if (distanceToPlayer > attackRange)
//         {
//             // Tentukan arah menuju pemain
//             Vector2 direction = (player.position - transform.position).normalized;
//             rb.linearVelocity = direction * moveSpeed;

//             // Gerakan vertikal (atas/bawah) agar musuh bisa bergerak di arah Y
//             float verticalDirection = player.position.y - transform.position.y; // Hitung jarak vertikal
//             if (Mathf.Abs(verticalDirection) > 0.1f) // Cek apakah musuh harus bergerak ke atas/bawah
//             {
//                 float verticalMove = verticalDirection > 0 ? 1 : -1; // Tentukan arah atas/bawah
//                 Vector3 pos = transform.position;
//                 // Batasi pergerakan vertikal di antara minY dan maxY
//                 pos.y = Mathf.Clamp(pos.y + verticalMove * moveSpeed * Time.deltaTime, minY, maxY);
//                 transform.position = pos;
//             }

//             // Flip sprite berdasarkan arah horizontal (kanan/kiri)
//             if (direction.x > 0.01f && transform.localScale.x < 0)
//             {
//                 // Arahkan musuh ke kanan
//                 transform.localScale = new Vector3(Mathf.Abs(startScale.x), startScale.y, startScale.z);  // Menghadap kanan
//             }
//             else if (direction.x < -0.01f && transform.localScale.x > 0)
//             {
//                 // Arahkan musuh ke kiri
//                 transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z); // Menghadap kiri
//             }

//             // Set animasi gerak
//             anim.SetBool("isMoving", true);  // Pastikan parameter "isMoving" ada di Animator
//         }
//         else
//         {
//             // Jika sudah cukup dekat, berhenti bergerak
//             StopMoving();
//         }
//     }


//     private void StopMoving()
//     {
//         // Jika musuh tidak melihat pemain atau sudah dekat, berhenti bergerak
//         rb.linearVelocity = Vector2.zero;
//         anim.SetBool("isMoving", false);  // Set parameter "isMoving" ke false saat berhenti
//     }
// }


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

    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 startScale;
    private float lastAttackTime;

    // Parameter Animator Hash
    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int AttackHash = Animator.StringToHash("attack");
    private static readonly int HurtHash = Animator.StringToHash("hurt");

    private Transform player;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startScale = transform.localScale;

        player = GameObject.FindWithTag("Player").transform; // Mengambil player berdasarkan tag
        mainCamera = Camera.main; // Mengambil referensi kamera utama

        // Atur gravitasi ke 0 karena tidak diperlukan untuk gerakan horizontal/vertikal saja
        rb.gravityScale = 0f;

        // Set Rigidbody2D ke Kinematic agar tidak ada dorongan fisika
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Update()
    {
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
            rb.linearVelocity = direction * moveSpeed;

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
            // Jika sudah cukup dekat, berhenti bergerak
            StopMoving();
        }
    }

    private void StopMoving()
    {
        // Jika musuh tidak melihat pemain atau sudah dekat, berhenti bergerak
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);  // Set parameter "isMoving" ke false saat berhenti
    }
}
