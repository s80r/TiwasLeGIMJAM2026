using UnityEngine;

public class FanSwitch : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject fanObject;
    
    [Header("Switch Visuals")]
    public GameObject leverVisual;
    public Sprite switchOnSprite;  
    public Sprite switchOffSprite; 
    public bool isOn = true; // Status Saklar: TRUE = ON, FALSE = OFF

    private SpriteRenderer spriteRenderer;
    private Collider2D myCollider;
    [SerializeField] private GameObject[] winds;
    void Awake()
    {
        spriteRenderer = leverVisual.GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (fanObject != null) winds = GameObject.FindGameObjectsWithTag("Angin");
        UpdateFanStatus();
    }

    public void SetStatus(bool status)
    {
        isOn = status;
        UpdateFanStatus();
    }

    void Update()
    {
        // Deteksi klik mouse kiri
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // --- LOGIKA ANTI GAGAL ---
            // Kita cek pakai dua cara sekaligus: OverlapPoint dan RaycastAll
            bool isClicked = false;
            
            // Cara 1: Cek apakah titik mouse ada di dalam collider saklar
            if (myCollider != null && myCollider.OverlapPoint(mousePos))
            {
                isClicked = true;
            }
            
            // Cara 2: Jika cara 1 gagal, tembak laser (Raycast) untuk cari saklar ini
            // Ini berguna jika saklar sedikit tertutup objek lain
            if (!isClicked)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject == this.gameObject)
                    {
                        isClicked = true;
                        break;
                    }
                }
            }

            if (isClicked)
            {
                isOn = !isOn; // TOGGLE: Balikkan status ON ke OFF atau sebaliknya
                Debug.Log("Saklar Berhasil Diklik! Status: " + (isOn ? "ON (Nyala)" : "OFF (Mati)"));
                UpdateFanStatus();
            }
        }
    }

    public void UpdateFanStatus()
    {
        if (fanObject == null) return;

        // Ambil semua effector (angin)
        AreaEffector2D[] effectors = fanObject.GetComponentsInChildren<AreaEffector2D>(true);
        foreach (var eff in effectors) 
        {
            if(eff != null) eff.enabled = isOn; // Jika isOn TRUE maka Effector ENABLED (ON)
        }

        // Ambil semua trigger (area sedot)
        Collider2D[] colliders = fanObject.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders) 
        {
            if (col != null && col.isTrigger) 
            {
                col.enabled = isOn; // Jika isOn TRUE maka Trigger ENABLED (ON)
            }
        }
        if (winds != null)
        {
            foreach (var wind in winds)
            {
                if (wind.transform.IsChildOf(fanObject.transform)) wind.SetActive(isOn);
            }
        }
        // Update Gambar Saklar
        if (spriteRenderer != null && switchOnSprite != null && switchOffSprite != null)
        {
            spriteRenderer.sprite = isOn ? switchOnSprite : switchOffSprite;
        }
    }
}