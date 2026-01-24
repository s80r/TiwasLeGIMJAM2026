using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FoldData 
{
    public float min, max, width;
    public bool isHorizontal; // True jika geser kiri-kanan, False jika atas-bawah
    public bool pulledPositive; // Arah tarikan (ke kanan/atas)
    public List<GameObject> hiddenObjects;
    public Dictionary<GameObject, Vector3> originalPositions;
    public GameObject hiddenNode;

    public FoldData(float min, float max, float w, bool horiz, bool pos, List<GameObject> objs, Dictionary<GameObject, Vector3> poses, GameObject node) 
    {
        this.min = min; this.max = max; this.width = w;
        this.isHorizontal = horiz; this.pulledPositive = pos;
        this.hiddenObjects = objs; this.originalPositions = poses;
        this.hiddenNode = node;
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
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Z)) UndoFold();
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
            Transform endNode = hit.collider.transform;
            float diffX = Mathf.Abs(startNode.position.x - endNode.position.x);
            float diffY = Mathf.Abs(startNode.position.y - endNode.position.y);

            // Cek apakah tarikan lebih ke arah Horizontal atau Vertikal
            if (diffY < alignmentTolerance) // HORIZONTAL
            {
                bool toRight = startNode.position.x < endNode.position.x;
                ExecuteFold(startNode.position, endNode.position, true, toRight);
            }
            else if (diffX < alignmentTolerance) // VERTIKAL
            {
                bool toUp = startNode.position.y < endNode.position.y;
                ExecuteFold(startNode.position, endNode.position, false, toUp);
            }
        }
        startNode = null;
    }

    void ExecuteFold(Vector3 p1, Vector3 p2, bool isHorizontal, bool pulledPositive)
    {
        float min = isHorizontal ? Mathf.Min(p1.x, p2.x) : Mathf.Min(p1.y, p2.y);
        float max = isHorizontal ? Mathf.Max(p1.x, p2.x) : Mathf.Max(p1.y, p2.y);
        float foldWidth = max - min;

        List<GameObject> hiddenThisTime = new List<GameObject>();
        Dictionary<GameObject, Vector3> positionsBeforeFold = new Dictionary<GameObject, Vector3>();
        GameObject[] foldables = GameObject.FindGameObjectsWithTag("Foldable");

        foreach (GameObject obj in foldables) positionsBeforeFold.Add(obj, obj.transform.position);

        foreach (GameObject obj in foldables)
        {
            float objPos = isHorizontal ? obj.transform.position.x : obj.transform.position.y;
            float epsilon = 0.01f;

            // Logika Sembunyikan/Potong (Hanya jika objek benar-benar di tengah area)
            if (objPos >= min - epsilon && objPos <= max + epsilon)
            {
                obj.SetActive(false);
                hiddenThisTime.Add(obj);
            }
            // Logika Geser
            else
            {
                Vector3 shift = isHorizontal ? new Vector3(foldWidth, 0, 0) : new Vector3(0, foldWidth, 0);
                
                if (isHorizontal) {
                    if (pulledPositive && objPos < min) obj.transform.position += shift;
                    else if (!pulledPositive && objPos > max) obj.transform.position -= shift;
                } else {
                    if (pulledPositive && objPos < min) obj.transform.position += shift;
                    else if (!pulledPositive && objPos > max) obj.transform.position -= shift;
                }
            }
        }

        GameObject nodeToHide = startNode.gameObject;
        nodeToHide.SetActive(false);
        foldHistory.Push(new FoldData(min, max, foldWidth, isHorizontal, pulledPositive, hiddenThisTime, positionsBeforeFold, nodeToHide));
    }

    void UndoFold()
    {
        if (foldHistory.Count == 0) return;
        FoldData lastFold = foldHistory.Pop();

        foreach (KeyValuePair<GameObject, Vector3> entry in lastFold.originalPositions)
        {
            if (entry.Key != null) {
                entry.Key.transform.position = entry.Value;
                if (lastFold.hiddenObjects.Contains(entry.Key)) entry.Key.SetActive(true);
            }
        }
        if (lastFold.hiddenNode != null) lastFold.hiddenNode.SetActive(true);
    }
}