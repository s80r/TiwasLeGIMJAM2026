using UnityEngine;

public class Saklar : MonoBehaviour
{
    [Header("Masukkan Objek Kipas di Sini")]
    public GameObject kipas;

    // Fungsi ini otomatis dipanggil Unity saat mouse mengklik objek ini
    private void OnMouseDown()
    {
        if (kipas != null)
        {
            // Ambil status aktif saat ini (True/False)
            bool statusSaatIni = kipas.activeSelf;

            // SetActive ke kebalikan status saat ini
            // Tanda seru (!) artinya "TIDAK" atau "KEBALIKAN"
            kipas.SetActive(!statusSaatIni);

            Debug.Log("Saklar ditekan. Status Kipas sekarang: " + !statusSaatIni);
        }
    }
}