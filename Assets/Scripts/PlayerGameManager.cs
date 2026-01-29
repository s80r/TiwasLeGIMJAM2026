using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGameManager : MonoBehaviour
{
    [Header("Settings")]
    public int levelAwal = 2; // Index scene Level 1 di Build Settings

    // 1. FUNGSI UNTUK MERESET DATA (Panggil ini lewat UI button jika ingin New Game)
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastCheckpointLevel");
        Debug.Log("Progress dihapus. Kembali ke awal.");
    }

    // 2. FUNGSI CHECKPOINT (Dipanggil saat player menyentuh Bendera/Checkpoint)
    public void PlayerCheckpoint()
    {
        int levelSekarang = SceneManager.GetActiveScene().buildIndex;
        
        // Simpan index scene saat ini ke memori permanen HP/PC
        PlayerPrefs.SetInt("LastCheckpointLevel", levelSekarang);
        PlayerPrefs.Save();
        
        Debug.Log("Checkpoint tersimpan! Level: " + levelSekarang);
    }

    // 3. FUNGSI MATI (Dipanggil saat player menyentuh Duri/Musuh/Jurang)
    public void PlayerDeath()
    {
        // Ambil data level terakhir yang di-checkpoint. 
        // Jika belum pernah checkpoint, default-nya adalah levelAwal (1).
        int levelTujuan = PlayerPrefs.GetInt("LastCheckpointLevel", levelAwal);

        Debug.Log("Player Mati! Mengulang dari Level: " + levelTujuan);
        
        // Pindah ke scene tersebut (akan mulai dari awal scene tersebut sesuai permintaanmu)
        SceneManager.LoadScene(levelTujuan);
    }
}