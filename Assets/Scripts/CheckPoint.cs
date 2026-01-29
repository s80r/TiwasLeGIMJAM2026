using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite spriteBelumDiambil;
    public Sprite spriteSudahDiambil;

    [Header("Audio")]
    public AudioClip suaraCheckpoint; // Masukkan file audio (MP3/WAV) ke sini
    [Range(0f, 1f)] public float volume = 0.7f; // Atur keras suara (0 sampai 1)

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private bool sudahDiambil = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        
        // Setup AudioSource secara otomatis agar tidak perlu ribet tambah manual
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = suaraCheckpoint;
        audioSource.volume = volume;

        if (sr != null && spriteBelumDiambil != null)
        {
            sr.sprite = spriteBelumDiambil;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !sudahDiambil)
        {
            PlayerGameManager manager = other.GetComponent<PlayerGameManager>();
            
            if (manager != null)
            {
                manager.PlayerCheckpoint();
                sudahDiambil = true;

                // 1. Ganti Sprite
                if (sr != null && spriteSudahDiambil != null)
                {
                    sr.sprite = spriteSudahDiambil;
                }

                // 2. Putar Suara
                if (suaraCheckpoint != null)
                {
                    audioSource.Play();
                }

                Debug.Log("Checkpoint Aktif! Suara Berbunyi.");
            }
        }
    }
}