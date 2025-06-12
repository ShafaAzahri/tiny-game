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
    
    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 startScale;
    private float lastAttackTime;
    
    // Jump system untuk beat em up
    private bool isJumping = false;
    private float jumpStartTime;
    private float groundY; // posisi Y saat mulai jump
    private Vector2 jumpStartPos;
    
    // Animator Parameter Hash
    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int AttackHash = Animator.StringToHash("attack");
    private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
    private static readonly int VerticalMoveHash = Animator.StringToHash("verticalMove"); // untuk gerak atas-bawah
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startScale = transform.localScale;
        
        // Untuk beat em up, kita tidak perlu gravity
        rb.gravityScale = 0f;
    }
    
    private void Update()
    {
        // Blokir input lain saat sedang dalam animasi serangan
        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        HandleMovement();
        HandleJump();
        HandleAttack();
        UpdateAnimations();
    }
    
    private void HandleMovement()
    {
        // Jangan bergerak horizontal/vertical jika sedang jumping
        if (isJumping) return;
        
        // Input gerak horizontal dan vertikal
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        // Batasi pergerakan dalam arena
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + h * moveSpeed * Time.deltaTime, minX, maxX);
        pos.y = Mathf.Clamp(pos.y + v * moveSpeed * Time.deltaTime, minY, maxY);
        transform.position = pos;
        
        // Flip sprite berdasarkan arah horizontal
        if (h > 0.01f)
            transform.localScale = new Vector3(+Mathf.Abs(startScale.x), startScale.y, startScale.z);
        else if (h < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z);
    }
    
    private void HandleJump()
    {
        // Input jump
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartJump();
            
            // Combo: lompat sambil serang
            if (Input.GetKey(KeyCode.F))
                TriggerAttack();
        }
        
        // Update posisi saat jumping
        if (isJumping)
        {
            float jumpProgress = (Time.time - jumpStartTime) / jumpDuration;
            
            if (jumpProgress >= 1f)
            {
                // Jump selesai
                EndJump();
            }
            else
            {
                // Hitung ketinggian menggunakan parabola
                float jumpHeight = Mathf.Sin(jumpProgress * Mathf.PI) * jumpForce;
                Vector3 pos = transform.position;
                pos.y = groundY + jumpHeight;
                transform.position = pos;
                
                // Bisa bergerak horizontal saat di udara (opsional)
                float h = Input.GetAxisRaw("Horizontal");
                if (h != 0)
                {
                    pos.x = Mathf.Clamp(pos.x + h * moveSpeed * 0.5f * Time.deltaTime, minX, maxX);
                    transform.position = pos;
                    
                    // Update flip saat di udara
                    if (h > 0.01f)
                        transform.localScale = new Vector3(+Mathf.Abs(startScale.x), startScale.y, startScale.z);
                    else if (h < -0.01f)
                        transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z);
                }
            }
        }
    }
    
    private void StartJump()
    {
        isJumping = true;
        jumpStartTime = Time.time;
        groundY = transform.position.y;
        jumpStartPos = transform.position;
        anim.SetTrigger(JumpHash);
        anim.SetBool(IsJumpingHash, true);
    }
    
    private void EndJump()
    {
        isJumping = false;
        Vector3 pos = transform.position;
        pos.y = groundY; // kembali ke posisi Y semula
        transform.position = pos;
        anim.SetBool(IsJumpingHash, false);
    }
    
    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= lastAttackTime + attackCooldown)
        {
            TriggerAttack();
        }
    }
    
    private void TriggerAttack()
    {
        anim.ResetTrigger(AttackHash);
        anim.SetTrigger(AttackHash);
        lastAttackTime = Time.time;
    }
    
    private void UpdateAnimations()
    {
        if (!isJumping)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            
            // Running animation (horizontal atau vertical movement)
            bool isMoving = (h != 0 || v != 0);
            anim.SetBool(RunHash, isMoving);
            
            // Vertical movement animation (opsional - untuk animasi khusus naik/turun)
            anim.SetFloat(VerticalMoveHash, v);
        }
    }
    
    // Fungsi helper untuk mengatur batas arena dari script lain
    public void SetArenaBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }
    
    // Getter untuk checking state
    public bool IsJumping => isJumping;
    public Vector2 GetMovementInput() => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
}