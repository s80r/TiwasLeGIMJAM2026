using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FoldData 
{
    public float minX;
    public float width;
    public List<GameObject> hiddenObjects;
    // Simpan posisi semua objek sebelum dilipat agar bisa dikembalikan ke titik EKSAK
    public Dictionary<GameObject, Vector3> originalPositions; 

    public FoldData(float x, float w, List<GameObject> objs, Dictionary<GameObject, Vector3> poses) 
    {
        minX = x;
        width = w;
        hiddenObjects = objs;
        originalPositions = poses;
    }
}

public class FoldManager : MonoBehaviour
{
    public LayerMask nodeLayer;
    public float alignmentTolerance = 0.5f;
    private Transform startNode;
    private Stack<FoldData> foldHistory = new Stack<FoldData>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleClick();
        if (Input.GetMouseButtonUp(0)) HandleRelease();
        if (Input.GetMouseButtonDown(1)) UndoFold();
    }

    void HandleClick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, nodeLayer);
        if (hit.collider != null && hit.collider.CompareTag("Node")) startNode = hit.collider.transform;
    }

    void HandleRelease()
    {
        if (startNode == null) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, nodeLayer);

        if (hit.collider != null && hit.collider.transform != startNode)
        {
            if (Mathf.Abs(startNode.position.y - hit.collider.transform.position.y) < alignmentTolerance)
                ExecuteFold(startNode.position, hit.collider.transform.position);
        }
        startNode = null;
    }

    void ExecuteFold(Vector3 p1, Vector3 p2)
    {
        float minX = Mathf.Min(p1.x, p2.x);
        float maxX = Mathf.Max(p1.x, p2.x);
        float foldWidth = maxX - minX;

        List<GameObject> hiddenThisTime = new List<GameObject>();
        Dictionary<GameObject, Vector3> positionsBeforeFold = new Dictionary<GameObject, Vector3>();
        
        GameObject[] foldables = GameObject.FindGameObjectsWithTag("Foldable");

        // TAHAP 1: Catat posisi semua objek SEBELUM ada yang berubah
        foreach (GameObject obj in foldables)
        {
            positionsBeforeFold.Add(obj, obj.transform.position);
        }

        // TAHAP 2: Eksekusi Perubahan
        foreach (GameObject obj in foldables)
        {
            float objX = obj.transform.position.x;
            float epsilon = 0.01f;

            if (objX >= minX - epsilon && objX <= maxX + epsilon)
            {
                obj.SetActive(false);
                hiddenThisTime.Add(obj);
            }
            else if (objX > maxX)
            {
                obj.transform.position -= new Vector3(foldWidth, 0, 0);
            }
        }

        foldHistory.Push(new FoldData(minX, foldWidth, hiddenThisTime, positionsBeforeFold));
    }

    void UndoFold()
    {
        if (foldHistory.Count == 0) return;

        FoldData lastFold = foldHistory.Pop();

        // KEMBALIKAN KE POSISI EKSAK YANG DICATAT SEBELUMNYA
        foreach (KeyValuePair<GameObject, Vector3> entry in lastFold.originalPositions)
        {
            if (entry.Key != null)
            {
                entry.Key.transform.position = entry.Value;
                
                // Jika objek tersebut tadinya disembunyikan, aktifkan lagi
                if (lastFold.hiddenObjects.Contains(entry.Key))
                {
                    entry.Key.SetActive(true);
                }
            }
        }
    }
}