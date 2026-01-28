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

    public FoldData(float min, float max, float w, bool horiz, bool pos, List<GameObject> objs, Dictionary<GameObject, Vector3> poses) 
    {
        this.min = min; this.max = max; this.width = w;
        this.isHorizontal = horiz; this.pulledPositive = pos;
        this.hiddenObjects = objs; this.originalPositions = poses;
    }
}

public class FoldManager : MonoBehaviour
{
    [Header("Setup")]
    public LayerMask nodeLayer;
    public GameObject player; 
    public float alignmentTolerance = 0.5f;

    private Transform startNode;
    private Transform endNode;
    private Stack<FoldData> foldHistory = new Stack<FoldData>();

    public Animator fihAnimator;
    public Sprite bautNyala;
    public Sprite bautMati;

    void Start()
    {
        fihAnimator = player.GetComponent<Animator>();
    }

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
        if (hit.collider != null && hit.collider.CompareTag("Node"))
        {
            startNode = hit.collider.transform;
            GameObject node = startNode.gameObject;
            node.GetComponent<SpriteRenderer>().sprite = bautNyala;
            fihAnimator.SetBool("isFolding", true);
        } 
    }

    void HandleRelease()
    {
        if (startNode == null) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, nodeLayer);

        if (hit.collider != null && hit.collider.transform != startNode)
        {
            endNode = hit.collider.transform;
            
            float diffX = Mathf.Abs(startNode.position.x - endNode.position.x);
            float diffY = Mathf.Abs(startNode.position.y - endNode.position.y);

            if (diffY < alignmentTolerance) 
                ExecuteFold(startNode.position, endNode.position, true, startNode.position.x < endNode.position.x);
            else if (diffX < alignmentTolerance) 
                ExecuteFold(startNode.position, endNode.position, false, startNode.position.y < endNode.position.y);
        }
        GameObject node = startNode.gameObject;
        node.GetComponent<SpriteRenderer>().sprite = bautMati;
        startNode = null;
        fihAnimator.SetBool("isFolding", false);
    }

    void ExecuteFold(Vector3 p1, Vector3 p2, bool isHorizontal, bool pulledPositive)
    {
        float epsilon = 0.001f; 
        float min = isHorizontal ? Mathf.Min(p1.x, p2.x) : Mathf.Min(p1.y, p2.y);
        float max = isHorizontal ? Mathf.Max(p1.x, p2.x) : Mathf.Max(p1.y, p2.y);
        float foldWidth = max - min;

        // 1. Cek Player Terjepit (Exclusive)
        if (player != null)
        {
            float playerPos = isHorizontal ? player.transform.position.x : player.transform.position.y;
            if (playerPos > min + epsilon && playerPos < max - epsilon)
            {
                Destroy(player);
            }
        }

        List<GameObject> hiddenThisTime = new List<GameObject>();
        Dictionary<GameObject, Vector3> positionsBeforeFold = new Dictionary<GameObject, Vector3>();

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Foldable"));
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Node"));

        foreach (GameObject obj in allTargets) 
        {
            if (!positionsBeforeFold.ContainsKey(obj))
                positionsBeforeFold.Add(obj, obj.transform.position);
        }

        Vector3 shift = isHorizontal ? new Vector3(foldWidth, 0, 0) : new Vector3(0, foldWidth, 0);

        foreach (GameObject obj in allTargets)
        {
            // --- PROTEKSI END NODE ---
            // End Node harus diam di tempat, tidak boleh bergeser atau hilang.
            if (obj.transform == endNode) continue;

            float objPos = isHorizontal ? obj.transform.position.x : obj.transform.position.y;

            // 2. Logika Hilang (Node/Foldable benar-benar di TENGAH)
            if (objPos > min + epsilon && objPos < max - epsilon)
            {
                obj.SetActive(false);
                hiddenThisTime.Add(obj);
            }
            // 3. Logika Geser (Termasuk StartNode agar menghimpit ke EndNode)
            else
            {
                if (pulledPositive)
                {
                    // Jika ditarik ke Kanan/Atas, semua yang di kiri/bawah ikut bergeser (termasuk startNode)
                    if (objPos <= min + epsilon) obj.transform.position += shift;
                }
                else
                {
                    // Jika ditarik ke Kiri/Bawah, semua yang di kanan/atas ikut bergeser (termasuk startNode)
                    if (objPos >= max - epsilon) obj.transform.position -= shift;
                }
            }
        }

        // 4. Geser Player (jika tidak terjepit)
        if (player != null)
        {
            float pPos = isHorizontal ? player.transform.position.x : player.transform.position.y;
            if (pulledPositive && pPos <= min + epsilon) player.transform.position += shift;
            else if (!pulledPositive && pPos >= max - epsilon) player.transform.position -= shift;
        }

        foldHistory.Push(new FoldData(min, max, foldWidth, isHorizontal, pulledPositive, hiddenThisTime, positionsBeforeFold));
    }

    void UndoFold()
    {
        if (foldHistory.Count == 0) return;
        FoldData lastFold = foldHistory.Pop();

        foreach (KeyValuePair<GameObject, Vector3> entry in lastFold.originalPositions)
        {
            if (entry.Key != null) 
            {
                entry.Key.transform.position = entry.Value;
                if (lastFold.hiddenObjects.Contains(entry.Key)) entry.Key.SetActive(true);
            }
        }
    }
}