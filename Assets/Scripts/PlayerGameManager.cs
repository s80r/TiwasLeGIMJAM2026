using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Dibutuhkan untuk Coroutine

public class PlayerGameManager : MonoBehaviour
{
    [Header("Settings")]
    public int levelAwal = 2; 
    public float delaySebelumRespawn = 1.0f; // Jeda waktu sebelum pindah level

    [Header("Death Effects")]
    public GameObject deathEffectPrefab; // Masukkan prefab partikel/sprite mati
    public AudioClip suaraMati;          // Masukkan file audio mati
    [Range(0f, 1f)] public float volumeMati = 0.8f;

    private bool sedangMati = false; // Mencegah fungsi mati terpanggil berkali-kali

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastCheckpointLevel");
        Debug.Log("Progress dihapus.");
    }

    public void PlayerCheckpoint()
    {
        int levelSekarang = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("LastCheckpointLevel", levelSekarang);
        PlayerPrefs.Save();
        Debug.Log("Checkpoint tersimpan! Level: " + levelSekarang);
    }

    // Fungsi ini dipanggil oleh duri/rintangan
    public void PlayerDeath()
    {
        if (!sedangMati)
        {
            StartCoroutine(ProsesKematian());
        }
    }

    IEnumerator ProsesKematian()
    {
        sedangMati = true;
        Debug.Log("Player Mati!");

        // 1. Munculkan Efek Visual (Sprite/Partikel)
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Putar Suara Mati
        if (suaraMati != null)
        {
            // Menggunakan PlayClipAtPoint agar suara tetap bunyi meskipun objek Player hancur/pindah
            AudioSource.PlayClipAtPoint(suaraMati, transform.position, volumeMati);
        }

        // 3. Sembunyikan Player (opsional, agar terlihat seperti hancur)
        // Kita matikan SpriteRenderer dan pergerakannya saja, jangan Destroy dulu
        if(GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        if(GetComponent<Rigidbody2D>() != null) GetComponent<Rigidbody2D>().simulated = false;

        // 4. Tunggu sebentar
        yield return new WaitForSeconds(delaySebelumRespawn);

        // 5. Pindah Level
        int levelTujuan = PlayerPrefs.GetInt("LastCheckpointLevel", levelAwal);
        SceneManager.LoadScene(levelTujuan);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(levelAwal);
        }
    }
}