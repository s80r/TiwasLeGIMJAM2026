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
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (moveInput != Vector2.zero)
        {
            gerak.SetActive(true);

            // 1. Logika Rotasi Halus (Menghadap arah WASD)
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            
            // Gunakan rotasi ini jika kamu mau objek 'gerak' atau arah tembakan mengikuti arah jalan
            // Tapi agar tidak kebalik, kita manipulasi scale berdasarkan arah X
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // 2. Logika Anti-Kebalik (Flip Scale)
            Vector3 newScale = transform.localScale;
            
            // Jika arahnya ke kiri (antara 90 sampai 270 derajat), flip Y scale-nya
            // Agar sprite tidak terlihat 'tengkurap' saat hadap kiri
            if (moveX < 0) {
                newScale.y = -Mathf.Abs(newScale.y); 
            }
            else if (moveX > 0) {
                newScale.y = Mathf.Abs(newScale.y);
            }
            transform.localScale = newScale;
        }
        else gerak.SetActive(false);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}