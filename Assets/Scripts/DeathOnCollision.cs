using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Settings")]
    public bool destroyInstantly = true;
    
    [Header("Visual Effects (Optional)")]
    public GameObject deathEffect; // Seret prefab partikel ke sini jika ada

    // 1. TERDETEKSI SAAT TABRAKAN FISIK (Dinding berduri, musuh, dll)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerDeath(collision.gameObject);
        }
    }

    // 2. TERDETEKSI SAAT MASUK AREA (Lava, jurang, area laser, dll)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerDeath(other.gameObject);
        }
    }

    private void HandlePlayerDeath(GameObject playerObj)
    {
        Debug.Log("<color=red>Bebaya!</color> Player menyentuh " + gameObject.name);

        // Munculkan efek partikel jika ada
        if (deathEffect != null)
        {
            Instantiate(deathEffect, playerObj.transform.position, Quaternion.identity);
        }

        if (destroyInstantly)
        {
            Destroy(playerObj);
        }
        else
        {
            // Jika tidak ingin hancur langsung, bisa matikan saja
            playerObj.SetActive(false);
        }
    }
}