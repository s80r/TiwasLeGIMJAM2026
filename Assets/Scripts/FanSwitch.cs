using UnityEngine;

public class FanSwitch : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject fanObject; 
    
    [Header("Switch Visuals")]
    public Sprite switchOnSprite;  
    public Sprite switchOffSprite; 
    public bool isOn = true;       

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateFanStatus(); // Jalankan sekali saat start agar sinkron
    }

    void OnMouseDown()
    {
        isOn = !isOn; 
        Debug.Log("Saklar diklik! Status sekarang: " + (isOn ? "ON" : "OFF"));
        UpdateFanStatus();
    }

    void UpdateFanStatus()
    {
        if (fanObject == null)
        {
            Debug.LogError("Waduh! Kolom Fan Object di saklar masih kosong, bang!");
            return;
        }

        // 1. Matikan/Nyalakan SEMUA AreaEffector2D (termasuk yang ada di Child)
        AreaEffector2D[] effectors = fanObject.GetComponentsInChildren<AreaEffector2D>(true);
        foreach (var eff in effectors)
        {
            eff.enabled = isOn;
        }

        // 2. Matikan/Nyalakan SEMUA Hazard (termasuk yang ada di Child)
        Hazard[] hazards = fanObject.GetComponentsInChildren<Hazard>(true);
        foreach (var haz in hazards)
        {
            haz.enabled = isOn;
        }

        // 3. Matikan/Nyalakan Collider pemicu (Trigger) jika ada
        // Ini jaga-jaga kalau tarikannya pakai PointEffector atau pemicu lainnya
        Collider2D[] colliders = fanObject.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            // Kita hanya matikan trigger-nya saja agar player tidak tersedot,
            // tapi tetap bisa menabrak body kipasnya.
            if (col.isTrigger) col.enabled = isOn;
        }

        // 4. Update tampilan saklar
        if (spriteRenderer != null && switchOnSprite != null && switchOffSprite != null)
        {
            spriteRenderer.sprite = isOn ? switchOnSprite : switchOffSprite;
        }
    }
}