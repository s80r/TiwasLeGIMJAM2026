using UnityEngine;
using System.Collections.Generic;

// --- DATA UNTUK RIWAYAT LIPATAN ---
[System.Serializable]
public class FoldData 
{
    public float minX;
    public float maxX;
    public float width;
    public bool pulledToRight;
    public List<GameObject> hiddenObjects;
    public Dictionary<GameObject, Vector3> originalPositions; 
    public GameObject hiddenNode; 

    public FoldData(float min, float max, float w, bool toRight, List<GameObject> objs, Dictionary<GameObject, Vector3> poses, GameObject node) 
    {
        minX = min;
        maxX = max;
        width = w;
        pulledToRight = toRight;
        hiddenObjects = objs;
        originalPositions = poses;
        hiddenNode = node;
    }
}

// --- MANAGER UTAMA ---
public class FoldManager : MonoBehaviour
{
    [Header("Setup")]
    public LayerMask nodeLayer;
    public float alignmentTolerance = 0.5f;

    private Transform startNode;
    private Stack<FoldData> foldHistory = new Stack<FoldData>();

    void Update()
    {
        // Klik Kiri untuk Mulai
        if (Input.GetMouseButtonDown(0)) HandleClick();
        
        // Lepas Klik untuk Eksekusi
        if (Input.GetMouseButtonUp(0)) HandleRelease();
        
        // Klik Kanan atau Tombol Z untuk Undo
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Z)) UndoFold();
    }

    void HandleClick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, nodeLayer);
        
        if (hit.collider != null && hit.collider.CompareTag("Node"))
        {
            startNode = hit.collider.transform;
            Debug.Log("Mulai dari: " + startNode.name);
        }
    }

    void HandleRelease()
    {
        if (startNode == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, nodeLayer);

        if (hit.collider != null && hit.collider.transform != startNode)
        {
            // Cek apakah sejajar secara horizontal
            if (Mathf.Abs(startNode.position.y - hit.collider.transform.position.y) < alignmentTolerance)
            {
                // Tentukan arah tarikan kursor
                bool toRight = startNode.position.x < hit.collider.transform.position.x;
                ExecuteFold(startNode.position, hit.collider.transform.position, toRight);
            }
        }
        startNode = null;
    }

    void ExecuteFold(Vector3 p1, Vector3 p2, bool pulledToRight)
    {
        float minX = Mathf.Min(p1.x, p2.x);
        float maxX = Mathf.Max(p1.x, p2.x);
        float foldWidth = maxX - minX;

        List<GameObject> hiddenThisTime = new List<GameObject>();
        Dictionary<GameObject, Vector3> positionsBeforeFold = new Dictionary<GameObject, Vector3>();
        
        // Cari semua objek yang bisa dilipat
        GameObject[] foldables = GameObject.FindGameObjectsWithTag("Foldable");

        // 1. Catat posisi asli semua objek
        foreach (GameObject obj in foldables)
        {
            if (!positionsBeforeFold.ContainsKey(obj))
                positionsBeforeFold.Add(obj, obj.transform.position);
        }

        // 2. Terapkan logika melipat
        foreach (GameObject obj in foldables)
        {
            float objX = obj.transform.position.x;
            float epsilon = 0.01f;

            // Jika di area tengah: Sembunyikan
            if (objX >= minX - epsilon && objX <= maxX + epsilon)
            {
                obj.SetActive(false);
                hiddenThisTime.Add(obj);
            }
            // Jika di luar area tengah: Geser sesuai arah kursor
            else
            {
                if (pulledToRight && objX < minX) // Tarik ke kanan, bagian kiri terseret
                {
                    obj.transform.position += new Vector3(foldWidth, 0, 0);
                }
                else if (!pulledToRight && objX > maxX) // Tarik ke kiri, bagian kanan terseret
                {
                    obj.transform.position -= new Vector3(foldWidth, 0, 0);
                }
            }
        }

        // 3. Sembunyikan Node Awal (Start Node)
        GameObject nodeToHide = startNode.gameObject;
        nodeToHide.SetActive(false);

        // Simpan ke riwayat
        foldHistory.Push(new FoldData(minX, maxX, foldWidth, pulledToRight, hiddenThisTime, positionsBeforeFold, nodeToHide));
        Debug.Log("Level Dilipat ke arah " + (pulledToRight ? "Kanan" : "Kiri"));
    }

    void UndoFold()
    {
        if (foldHistory.Count == 0) return;

        FoldData lastFold = foldHistory.Pop();

        // Kembalikan semua objek ke posisi snapshot-nya
        foreach (KeyValuePair<GameObject, Vector3> entry in lastFold.originalPositions)
        {
            if (entry.Key != null)
            {
                entry.Key.transform.position = entry.Value;
                
                // Aktifkan kembali jika tadinya sembunyi
                if (lastFold.hiddenObjects.Contains(entry.Key))
                {
                    entry.Key.SetActive(true);
                }
            }
        }

        // Munculkan kembali Node awal
        if (lastFold.hiddenNode != null)
        {
            lastFold.hiddenNode.SetActive(true);
        }
        
        Debug.Log("Lipatan dikembalikan.");
    }
}