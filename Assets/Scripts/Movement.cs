using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject gerak;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Agar tidak jatuh ke bawah karena gravitasi
        rb.gravityScale = 0;
        // Agar tidak berputar saat menabrak benda
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Mendeteksi input WASD atau Panah
        // GetAxisRaw membuat gerakan terasa instan/presisi
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (moveInput != Vector2.zero){
            gerak.SetActive(true);
            Vector3 newScale = transform.localScale;
            if (moveX < 0) {
                newScale.x = -0.07f;
            }
            else{
                newScale.x = 0.07f;
            }
            transform.localScale = newScale;
        }
        else gerak.SetActive(false);
    }

    void FixedUpdate()
    {
        // Menerapkan kecepatan ke Rigidbody
        rb.linearVelocity = moveInput * moveSpeed;
    }
}