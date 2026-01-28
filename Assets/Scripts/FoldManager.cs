using UnityEngine;
using UnityEngine.Tilemaps; // Wajib untuk manipulasi Tilemap
using System.Collections.Generic;

[System.Serializable]
public class FoldData 
{
    public float min, max, width;
    public bool isHorizontal;
    public bool pulledPositive;
    public List<GameObject> hiddenObjects;
    public Dictionary<GameObject, Vector3> originalPositions;
    
    // Simpan data tile yang dihapus agar bisa di-Undo
    public List<TileUndoData> hiddenTiles; 

    public FoldData(float min, float max, float w, bool horiz, bool pos, List<GameObject> objs, Dictionary<GameObject, Vector3> poses, List<TileUndoData> tiles) 
    {
        this.min = min; this.max = max; this.width = w;
        this.isHorizontal = horiz; this.pulledPositive = pos;
        this.hiddenObjects = objs; this.originalPositions = poses;
        this.hiddenTiles = tiles;
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
    public Tilemap targetTilemap; // Tarik Tilemap kamu ke sini di Inspector
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

        startNode.GetComponent<SpriteRenderer>().sprite = bautMati;
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
            if (playerPos > min + epsilon && playerPos < max - epsilon)
            {
                Destroy(player);
            }
        }

        List<GameObject> hiddenThisTime = new List<GameObject>();
        Dictionary<GameObject, Vector3> positionsBeforeFold = new Dictionary<GameObject, Vector3>();
        List<TileUndoData> tilesToUndo = new List<TileUndoData>();

        // --- LOGIKA TILEMAP PER TILE ---
        if (targetTilemap != null)
        {
            BoundsInt bounds = targetTilemap.cellBounds;
            // List sementara agar tidak bentrok saat manipulasi tile dalam loop
            Dictionary<Vector3Int, TileBase> nextTileLayout = new Dictionary<Vector3Int, TileBase>();

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                TileBase tile = targetTilemap.GetTile(pos);
                if (tile == null) continue;

                // Koordinat tile (x atau y)
                float tileCoord = isHorizontal ? pos.x : pos.y;

                // A. Tile di TENGAH (Hapus) - Kita tambah 0.5 karena posisi tile dihitung dari pivotnya
                if (tileCoord + 0.5f > min + epsilon && tileCoord + 0.5f < max - epsilon)
                {
                    tilesToUndo.Add(new TileUndoData { map = targetTilemap, pos = pos, tile = tile });
                    targetTilemap.SetTile(pos, null);
                }
                // B. Tile di LUAR (Geser)
                else
                {
                    Vector3Int shift = isHorizontal ? new Vector3Int(foldWidthInt, 0, 0) : new Vector3Int(0, foldWidthInt, 0);
                    
                    if (pulledPositive && tileCoord + 0.5f <= min + epsilon)
                    {
                        tilesToUndo.Add(new TileUndoData { map = targetTilemap, pos = pos, tile = tile });
                        targetTilemap.SetTile(pos, null);
                        nextTileLayout[pos + shift] = tile;
                    }
                    else if (!pulledPositive && tileCoord + 0.5f >= max - epsilon)
                    {
                        tilesToUndo.Add(new TileUndoData { map = targetTilemap, pos = pos, tile = tile });
                        targetTilemap.SetTile(pos, null);
                        nextTileLayout[pos - shift] = tile;
                    }
                }
            }
            // Terapkan hasil pergeseran tile
            foreach(var item in nextTileLayout) targetTilemap.SetTile(item.Key, item.Value);
        }

        // --- LOGIKA GAMEOBJECT (Foldable & Node) ---
        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Foldable"));
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Node"));

        foreach (GameObject obj in allTargets) 
        {
            if (!positionsBeforeFold.ContainsKey(obj))
                positionsBeforeFold.Add(obj, obj.transform.position);
        }

        Vector3 objShift = isHorizontal ? new Vector3(foldWidth, 0, 0) : new Vector3(0, foldWidth, 0);

        foreach (GameObject obj in allTargets)
        {
            if (obj.transform == endNode) continue;

            float objPos = isHorizontal ? obj.transform.position.x : obj.transform.position.y;

            if (objPos > min + epsilon && objPos < max - epsilon)
            {
                obj.SetActive(false);
                hiddenThisTime.Add(obj);
            }
            else
            {
                if (pulledPositive && objPos <= min + epsilon) obj.transform.position += objShift;
                else if (!pulledPositive && objPos >= max - epsilon) obj.transform.position -= objShift;
            }
        }

        // --- GESER PLAYER ---
        if (player != null)
        {
            float pPos = isHorizontal ? player.transform.position.x : player.transform.position.y;
            if (pulledPositive && pPos <= min + epsilon) player.transform.position += objShift;
            else if (!pulledPositive && pPos >= max - epsilon) player.transform.position -= objShift;
        }

        foldHistory.Push(new FoldData(min, max, foldWidth, isHorizontal, pulledPositive, hiddenThisTime, positionsBeforeFold, tilesToUndo));
    }

    void UndoFold()
    {
        if (foldHistory.Count == 0) return;
        FoldData lastFold = foldHistory.Pop();

        // 1. Kembalikan Tilemap
        if (targetTilemap != null)
        {
            // Bersihkan area yang mungkin sudah bergeser agar tidak tumpang tindih saat balik
            foreach (var t in lastFold.hiddenTiles) targetTilemap.SetTile(t.pos, null); 
            // Kembalikan tile ke posisi asli
            foreach (var t in lastFold.hiddenTiles) targetTilemap.SetTile(t.pos, t.tile);
        }

        // 2. Kembalikan GameObject
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