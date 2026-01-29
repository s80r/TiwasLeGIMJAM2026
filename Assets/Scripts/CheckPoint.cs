using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual Feedback (Opsional)")]
    public Color warnaAktif = Color.green; // Warna saat checkpoint tersentuh
    private bool sudahDiambil = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang menabrak adalah Player dan belum pernah diambil
        if (other.CompareTag("Player") && !sudahDiambil)
        {
            // 1. Panggil fungsi PlayerCheckpoint di script utama Player
            PlayerGameManager manager = other.GetComponent<PlayerGameManager>();
            
            if (manager != null)
            {
                manager.PlayerCheckpoint();
                sudahDiambil = true;

                // 2. Feedback Visual: Ubah warna objek (opsional)
                // Biar player tahu kalau checkpoint-nya sudah aktif
                if (GetComponent<SpriteRenderer>() != null)
                {
                    GetComponent<SpriteRenderer>().color = warnaAktif;
                }

                Debug.Log("Checkpoint Berhasil Diaktifkan!");
            }
        }
    }
}