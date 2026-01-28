using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public class FoldData 
{
    public float min, max, width;
    public bool isHorizontal;
    public bool pulledPositive;
    public List<GameObject> hiddenObjects;
    public Dictionary<GameObject, Vector3> originalPositions;
    public List<TileUndoData> hiddenTiles; 
    public Dictionary<FanSwitch, bool> switchStatuses;

    public FoldData(float min, float max, float w, bool horiz, bool pos, List<GameObject> objs, Dictionary<GameObject, Vector3> poses, List<TileUndoData> tiles, Dictionary<FanSwitch, bool> switches) 
    {
        this.min = min; this.max = max; this.width = w;
        this.isHorizontal = horiz; this.pulledPositive = pos;
        this.hiddenObjects = objs; this.originalPositions = poses;
        this.hiddenTiles = tiles;
        this.switchStatuses = switches;
    }
}

[System.Serializable]
public struct TileUndoData {
    public Tilemap map;
    public Vector3Int pos;
    public TileBase tile;
}

public class FoldManager : MonoBehaviour
{
    [Header("Setup")]
    public LayerMask nodeLayer;
    public GameObject player; 
    public Tilemap targetTilemap;
    public float alignmentTolerance = 0.5f;

    [Header("Visuals")]
    public Sprite bautNyala;
    public Sprite bautMati;

    private Transform startNode;
    private Transform endNode;
    private Stack<FoldData> foldHistory = new Stack<FoldData>();
    private Animator fihAnimator;

    void Start()
    {
        if (player != null) fihAnimator = player.GetComponent<Animator>();
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
            startNode.GetComponent<SpriteRenderer>().sprite = bautNyala;
            if (fihAnimator != null) fihAnimator.SetBool("isFolding", true);
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

        if(startNode != null) startNode.GetComponent<SpriteRenderer>().sprite = bautMati;
        if (fihAnimator != null) fihAnimator.SetBool("isFolding", false);
        startNode = null;
    }

    void ExecuteFold(Vector3 p1, Vector3 p2, bool isHorizontal, bool pulledPositive)
    {
        float epsilon = 0.001f; 
        float min = isHorizontal ? Mathf.Min(p1.x, p2.x) : Mathf.Min(p1.y, p2.y);
        float max = isHorizontal ? Mathf.Max(p1.x, p2.x) : Mathf.Max(p1.y, p2.y);
        float foldWidth = max - min;
        int foldWidthInt = Mathf.RoundToInt(foldWidth);

        // 1. Cek Player Terjepit
        if (player != null)
        {
            float playerPos = isHorizontal ? player.transform.position.x : player.transform.position.y;
            if (playerPos > min + epsilon && playerPos < max - epsilon) Destroy(player);
        }

        List<GameObject> hiddenThisTime = new List<GameObject>();
        Dictionary<GameObject, Vector3> positionsBeforeFold = new Dictionary<GameObject, Vector3>();
        List<TileUndoData> tilesToUndo = new List<TileUndoData>();
        Dictionary<FanSwitch, bool> switchStatuses = new Dictionary<FanSwitch, bool>();

        // --- TILEMAP LOGIC ---
        if (targetTilemap != null)
        {
            foreach (Vector3Int pos in targetTilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = targetTilemap.GetTile(pos);
                if (tile == null) continue;
                tilesToUndo.Add(new TileUndoData { map = targetTilemap, pos = pos, tile = tile });
                float tileCoord = isHorizontal ? pos.x : pos.y;
                if (tileCoord + 0.5f > min + epsilon && tileCoord + 0.5f < max - epsilon) targetTilemap.SetTile(pos, null);
            }

            Vector3Int tShift = isHorizontal ? new Vector3Int(foldWidthInt, 0, 0) : new Vector3Int(0, foldWidthInt, 0);
            List<TileUndoData> tilesToMove = new List<TileUndoData>();
            foreach(var t in tilesToUndo)
            {
                float coord = isHorizontal ? t.pos.x : t.pos.y;
                if (pulledPositive && coord + 0.5f <= min + epsilon) tilesToMove.Add(new TileUndoData { map = t.map, pos = t.pos + tShift, tile = t.tile });
                else if (!pulledPositive && coord + 0.5f >= max - epsilon) tilesToMove.Add(new TileUndoData { map = t.map, pos = t.pos - tShift, tile = t.tile });
            }

            foreach(var t in tilesToUndo) {
                float coord = isHorizontal ? t.pos.x : t.pos.y;
                if ((pulledPositive && coord + 0.5f <= min + epsilon) || (!pulledPositive && coord + 0.5f >= max - epsilon)) targetTilemap.SetTile(t.pos, null);
            }
            foreach(var t in tilesToMove) targetTilemap.SetTile(t.pos, t.tile);
        }

        // --- GAMEOBJECT LOGIC (Foldable, Node, Finish, Switch) ---
        FanSwitch[] allSwitches = Object.FindObjectsByType<FanSwitch>(FindObjectsSortMode.None);
        foreach (var s in allSwitches) switchStatuses.Add(s, s.isOn);

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Foldable"));
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Node"));
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Finish"));

        foreach (GameObject obj in allTargets) if (!positionsBeforeFold.ContainsKey(obj)) positionsBeforeFold.Add(obj, obj.transform.position);

        Vector3 objShift = isHorizontal ? new Vector3(foldWidth, 0, 0) : new Vector3(0, foldWidth, 0);
        foreach (GameObject obj in allTargets)
        {
            if (obj.transform == endNode || obj.CompareTag("Finish")) continue;
            float objPos = isHorizontal ? obj.transform.position.x : obj.transform.position.y;

            if (objPos > min + epsilon && objPos < max - epsilon) { obj.SetActive(false); hiddenThisTime.Add(obj); }
            else {
                if (pulledPositive && objPos <= min + epsilon) obj.transform.position += objShift;
                else if (!pulledPositive && objPos >= max - epsilon) obj.transform.position -= objShift;
            }
        }

        if (player != null) {
            float pPos = isHorizontal ? player.transform.position.x : player.transform.position.y;
            if (pulledPositive && pPos <= min + epsilon) player.transform.position += objShift;
            else if (!pulledPositive && pPos >= max - epsilon) player.transform.position -= objShift;
        }

        foldHistory.Push(new FoldData(min, max, foldWidth, isHorizontal, pulledPositive, hiddenThisTime, positionsBeforeFold, tilesToUndo, switchStatuses));
    }

    void UndoFold()
    {
        if (foldHistory.Count == 0) return;
        FoldData lastFold = foldHistory.Pop();

        if (targetTilemap != null) {
            targetTilemap.ClearAllTiles();
            foreach (var t in lastFold.hiddenTiles) t.map.SetTile(t.pos, t.tile);
        }

        foreach (KeyValuePair<GameObject, Vector3> entry in lastFold.originalPositions)
        {
            if (entry.Key != null) {
                entry.Key.transform.position = entry.Value;
                if (lastFold.hiddenObjects.Contains(entry.Key)) entry.Key.SetActive(true);
            }
        }

        foreach (KeyValuePair<FanSwitch, bool> sw in lastFold.switchStatuses) if (sw.Key != null) sw.Key.SetStatus(sw.Value);
    }
}