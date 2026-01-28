using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelAuto : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Ambil nomor index scene yang sedang aktif sekarang
            int sceneSaatIni = SceneManager.GetActiveScene().buildIndex;
            
            // 2. Hitung index scene selanjutnya
            int sceneSelanjutnya = sceneSaatIni + 1;

            // 3. Cek apakah masih ada scene selanjutnya?
            // SceneManager.sceneCountInBuildSettings adalah total jumlah scene
            if (sceneSelanjutnya < SceneManager.sceneCountInBuildSettings)
            {
                // Jika ada, load scene selanjutnya
                SceneManager.LoadScene(sceneSelanjutnya);
            }
            else
            {
                // Jika sudah tamat (tidak ada scene lagi), kembali ke Menu Utama (biasanya index 0)
                Debug.Log("Game Tamat! Kembali ke Menu.");
                SceneManager.LoadScene(0); 
            }
        }
    }
}