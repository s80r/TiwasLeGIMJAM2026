using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Settings")]
    public int indexSceneTujuan; // Masukkan nomor scene di Build Settings
    public bool resetCheckpointOnExit = true; // Jika true, checkpoint lama dihapus saat pindah level

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang menyentuh adalah Player
        if (other.CompareTag("Player"))
        {
            // Ambil referensi ke PlayerGameManager untuk mereset checkpoint jika perlu
            if (resetCheckpointOnExit)
            {
                PlayerPrefs.DeleteKey("LastCheckpointLevel");
                PlayerPrefs.Save();
            }

            Debug.Log("Pindah ke Scene: " + indexSceneTujuan);
            
            // Pindah ke scene tujuan
            SceneManager.LoadScene(indexSceneTujuan);
        }
    }
}