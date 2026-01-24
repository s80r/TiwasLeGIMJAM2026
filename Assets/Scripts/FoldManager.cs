using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FoldData 
{
    public float min, max, width;
    public bool isHorizontal;
    public bool pulledPositive;
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
    [Header("Setup")]
    public LayerMask nodeLayer;
    public GameObject player; 
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

            if (diffY < alignmentTolerance) 
                ExecuteFold(startNode.position, endNode.position, true, startNode.position.x < endNode.position.x);
            else if (diffX < alignmentTolerance) 
                ExecuteFold(startNode.position, endNode.position, false, startNode.position.y < endNode.position.y);
        }
        startNode = null;
    }

    void ExecuteFold(Vector3 p1, Vector3 p2, bool isHorizontal, bool pulledPositive)
    {
        // Jika player sudah hancur sebelumnya, kita abaikan pengecekan player
        if (player == null) {
            PerformFoldLogic(p1, p2, isHorizontal, pulledPositive);
            return;
        }

        float min = isHorizontal ? Mathf.Min(p1.x, p2.x) : Mathf.Min(p1.y, p2.y);
        float max = isHorizontal ? Mathf.Max(p1.x, p2.x) : Mathf.Max(p1.y, p2.y);
        float epsilon = 0.05f;

        // --- CEK PLAYER TERJEPIT ---
        float playerPos = isHorizontal ? player.transform.position.x : player.transform.position.y;
        if (playerPos >= min - epsilon && playerPos <= max + epsilon)
        {
            Debug.Log("Player hancur terjepit!");
            Destroy(player); // Player hilang dari hierarki
            // Kita tetap lanjutkan lipatan meskipun player hancur
        }

        PerformFoldLogic(p1, p2, isHorizontal, pulledPositive);
    }

    void PerformFoldLogic(Vector3 p1, Vector3 p2, bool isHorizontal, bool pulledPositive)
    {
        float min = isHorizontal ? Mathf.Min(p1.x, p2.x) : Mathf.Min(p1.y, p2.y);
        float max = isHorizontal ? Mathf.Max(p1.x, p2.x) : Mathf.Max(p1.y, p2.y);
        float foldWidth = max - min;
        float epsilon = 0.05f;

        List<GameObject> hiddenThisTime = new List<GameObject>();
        Dictionary<GameObject, Vector3> positionsBeforeFold = new Dictionary<GameObject, Vector3>();
        GameObject[] foldables = GameObject.FindGameObjectsWithTag("Foldable");

        foreach (GameObject obj in foldables) positionsBeforeFold.Add(obj, obj.transform.position);

        foreach (GameObject obj in foldables)
        {
            float objPos = isHorizontal ? obj.transform.position.x : obj.transform.position.y;

            if (objPos >= min - epsilon && objPos <= max + epsilon)
            {
                obj.SetActive(false);
                hiddenThisTime.Add(obj);
            }
            else
            {
                Vector3 shift = isHorizontal ? new Vector3(foldWidth, 0, 0) : new Vector3(0, foldWidth, 0);
                if (pulledPositive && objPos < min) obj.transform.position += shift;
                else if (!pulledPositive && objPos > max) obj.transform.position -= shift;
            }
        }

        // Geser player jika dia masih ada dan tidak terjepit
        if (player != null)
        {
            float pPos = isHorizontal ? player.transform.position.x : player.transform.position.y;
            Vector3 pShift = isHorizontal ? new Vector3(foldWidth, 0, 0) : new Vector3(0, foldWidth, 0);
            if (pulledPositive && pPos < min) player.transform.position += pShift;
            else if (!pulledPositive && pPos > max) player.transform.position -= pShift;
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