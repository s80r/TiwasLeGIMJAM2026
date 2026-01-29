using UnityEngine;

public class Hazard : MonoBehaviour
{
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
        Debug.Log("<color=red>Bahaya!</color> Player menyentuh " + gameObject.name);

        // --- EFEK VISUAL ---
        // Munculkan efek partikel jika ada sebelum scene berpindah
        if (deathEffect != null)
        {
            Instantiate(deathEffect, playerObj.transform.position, Quaternion.identity);
        }

        // --- PANGGIL FUNGSI MATI ---
        // Memanggil fungsi dari script PlayerGameManager yang ada di Player
        // Ini akan memicu reload ke level 1 atau level checkpoint
        playerObj.GetComponent<PlayerGameManager>().PlayerDeath();

        // Bagian Destroy dan SetActive sudah dihapus agar tidak terjadi konflik
    }
}