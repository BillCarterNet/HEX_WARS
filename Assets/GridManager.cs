using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int width = 15;
    public int height = 15;
    public float tileSpacing = 1.2f;
    public float hexSize = 0.75f;
    public int moveRange = 2;
    public GameObject tilePrefab;
    public GameObject assaultPrefab;
    public GameObject sniperPrefab;
    public Material playerMaterial;
    public Material enemyMaterial;

    public enum UnitAction
    {
        Attack,
        End,
        Cancel
    }

    private Tile currentlySelectedTile;
    // This "Dictionary" lets us look up a Tile using a Vector2Int coordinate
    private Dictionary<Vector2Int, Tile> gridDictionary = new Dictionary<Vector2Int, Tile>();
    private List<Tile> highlightedTiles = new List<Tile>();
    private Unit selectedUnit;
    private bool hasMoved = false;
    private Tile originalTile;
    private bool isActionMenuActive = false;

    public void SelectTile(Tile newTile)
    {
        Debug.Log($"CLICK: {newTile.coordinates}");

        // 1. MOVE FIRST (critical)
        if (selectedUnit != null && highlightedTiles.Contains(newTile))
        {
            // 🚫 Block movement if occupied
            if (newTile.currentUnit != null)
            {
                Debug.Log("Tile occupied!");
                return;
            }

            MoveUnit(selectedUnit, newTile, true);
            Debug.Log($"After MoveUnit → hasMoved = {hasMoved}");
            return;
        }

        // 2. deselect same tile
        if (currentlySelectedTile == newTile)
        {
            ShowActionMenu(selectedUnit);
            currentlySelectedTile.ResetColor();
            currentlySelectedTile = null;
            selectedUnit = null;
            ClearHighlights();
            return;
        }

        // 3. reset visuals
        if (currentlySelectedTile != null)
        {
            currentlySelectedTile.ResetColor();
        }

        ClearHighlights();

        // 4. select tile
        currentlySelectedTile = newTile;
        currentlySelectedTile.Highlight();

        // 5. unit selection
        if (newTile.currentUnit != null)
        {
            isActionMenuActive = false;
            
            selectedUnit = newTile.currentUnit;
            selectedUnit.originalTile = newTile;

            hasMoved = false;
            originalTile = newTile;

            Debug.Log($"Selected unit: {selectedUnit.unitName}");

            HighlightMoveRange(newTile);
        }
    }

    void MoveUnit(Unit unit, Tile targetTile, bool showMenu = true)
    {
        Debug.Log("MoveUnit CALLED → setting hasMoved = true");
        unit.hasMoved = true;
        
        Tile oldTile = null;

        if (unit.transform.parent != null)
        {
            oldTile = unit.transform.parent.GetComponent<Tile>();
        }

        if (oldTile != null)
        {
            oldTile.currentUnit = null;
        }

        targetTile.currentUnit = unit;

        unit.transform.SetParent(targetTile.transform);
        unit.transform.localPosition = new Vector3(0, 0.5f, 0);

        unit.gridPosition = targetTile.coordinates;

        unit.SetTile(targetTile);

        Debug.Log($"Moved {unit.unitName} to {targetTile.coordinates}");

        // Reset the old selected tile colour
        if (currentlySelectedTile != null)
        {
            currentlySelectedTile.ResetColor();
        }

        unit.hasMoved = true;

        ClearHighlights();

        if (showMenu)
        {
            ShowActionMenu(selectedUnit);
        }
    }

    void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void SpawnInitialUnits()
    {
        // Spawn Player Assault at 0,0
        SpawnUnit(new Vector2Int(0, 0), assaultPrefab, "Player Assault", 0, 2);
        
        // Spawn Player Sniper at 0,1
        SpawnUnit(new Vector2Int(1, 0), sniperPrefab, "Player Sniper", 0, 1);

        // Spawn Enemy Assault at the opposite corner
        SpawnUnit(new Vector2Int(width - 1, height - 1), assaultPrefab, "Enemy Assault", 1, 2);

        // Spawn Enemy Sniper next to the Enemy Assault
        SpawnUnit(new Vector2Int(width - 2, height - 1), sniperPrefab, "Enemy Sniper", 1, 1);
    }

    void SpawnUnit(Vector2Int coord, GameObject prefab, string unitName, int team, int moveRange)
    {
        if (gridDictionary.ContainsKey(coord))
        {
            Tile targetTile = gridDictionary[coord];
            
            // 1. Spawn it
            GameObject unitGO = Instantiate(prefab);
            
            // 2. Make it a child of the tile immediately
            unitGO.transform.SetParent(targetTile.transform);

            // 3. Reset LOCAL position (0,0,0 is now the center of the tile)
            unitGO.transform.localPosition = new Vector3(0, 0.5f, 0);
            unitGO.transform.localRotation = Quaternion.identity;

            Unit unitScript = unitGO.GetComponent<Unit>();
            if (unitScript != null)
            {
                unitScript.Initialize(unitName, team, moveRange, 2, coord);
                unitScript.SetManager(this);
                targetTile.currentUnit = unitScript;
                unitScript.SetTile(targetTile);

                Renderer r = unitGO.GetComponentInChildren<Renderer>();
                Debug.Log($"Setting material for {unitName} (Team {team})");
                Debug.Log(r);

                if (r != null)
                {
                    if (team == 0)
                        r.material = playerMaterial;
                    else
                        r.material = enemyMaterial;
                }
            }
        }
        else
        {
            Debug.LogError($"Could not find tile at {coord} to spawn {unitName}!");
        }
    }

    public void GenerateGrid()
    {
        ClearGrid();
        gridDictionary.Clear();

        float tileWidth = hexSize * 1.5f;
        float tileHeight = hexSize * Mathf.Sqrt(3f);
        int tileCount = 0;

        for (int x = 0; x < this.width; x++)
        {
            for (int z = 0; z < this.height; z++)
            {
                tileCount++;
                Debug.Log($"Creating Tile: {tileCount} - hex co-ordinates [{x}, {z}]");
                float xPos = x * tileWidth;
                float zPos = z * tileHeight + (x % 2 == 1 ? tileHeight / 2f : 0f);

                // Log the calculated position for debugging to 2dp
                Debug.Log($"Calculated centre position for Tile: ({xPos:F2}, {zPos:F2})");
                Vector3 position = new Vector3(xPos, 0, zPos);

                // Inside your loop in GridManager.cs:
                GameObject go = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                Tile t = go.GetComponent<Tile>();
                if (t != null) 
                {
                    t.Setup(x, z, this);
                    gridDictionary.Add(new Vector2Int(x, z), t); // Store it!
                }
            }
        }

        SpawnInitialUnits();

        Camera.main.transform.position = new Vector3(
            width * 0.5f,
            10f,
            -1 *height * 0.5f
        );

        Camera.main.transform.rotation = Quaternion.Euler(60f, 0f, 0f); 
    }

    void ClearHighlights()
    {
        for (int i = 0; i < highlightedTiles.Count; i++)
        {
            highlightedTiles[i].ResetColor();
        }

        highlightedTiles.Clear();
    }

    void HighlightMoveRange(Tile origin)
    {
        ClearHighlights();

        foreach (var kvp in gridDictionary)
        {
            Tile tile = kvp.Value;

            // Convert BOTH tiles to cube coordinates first
            Vector3Int a = OffsetToCube(origin.coordinates);
            Vector3Int b = OffsetToCube(tile.coordinates);

            int distance = Mathf.Max(
                Mathf.Abs(a.x - b.x),
                Mathf.Abs(a.y - b.y),
                Mathf.Abs(a.z - b.z)
            );

            if (distance > selectedUnit.moveRange)
                continue;

            // 🚫 Skip occupied tiles
            if (tile.currentUnit != null)
                continue;

            tile.HighlightMoveRange();
            highlightedTiles.Add(tile);
        }
    }

    Vector3Int OffsetToCube(Vector2Int coord)
    {
        int x = coord.x;
        int z = coord.y;

        int cubeX = x;
        int cubeZ = z - (x - (x & 1)) / 2;
        int cubeY = -cubeX - cubeZ;

        return new Vector3Int(cubeX, cubeY, cubeZ);
    }

    void ShowActionMenu(Unit unit)
    {
        isActionMenuActive = true;
        Debug.Log("=== ACTION MENU ===");
        Debug.Log($"hasMoved = {hasMoved}");

        if (unit == null)
        {
            Debug.Log("No unit selected");
            return;
        }

        Debug.Log($"Unit: {unit.unitName}");

        if (!unit.hasMoved)
            Debug.Log("Available: Attack, End, Cancel");
        else
            Debug.Log("Available: End, Cancel");
    }

    void EndTurn()
    {
        Debug.Log("END TURN");

        isActionMenuActive = false;

        // Reset the currently selected tile's color
        if (currentlySelectedTile != null)
        {
            currentlySelectedTile.ResetColor();
        }

        ClearHighlights();

        currentlySelectedTile = null;
        selectedUnit = null;

        if (selectedUnit != null)
        {
            selectedUnit.hasMoved = false;
            selectedUnit.originalTile = null;
        }
    }

    void Update()
    {
        if (!isActionMenuActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Pressed E → End Turn");
            EndTurn();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Pressed C → Cancel");
            CancelAction();
        }
    }

    void CancelAction()
    {
        Debug.Log("CANCEL");

        isActionMenuActive = false;

        if (selectedUnit == null)
            return;

        // If we moved → go back
        if (selectedUnit.hasMoved && selectedUnit.originalTile != null)
        {
            MoveUnit(selectedUnit, selectedUnit.originalTile, false);
            selectedUnit.hasMoved = false;

            // Re-highlight movement from original tile
            currentlySelectedTile = originalTile;
            HighlightMoveRange(originalTile);
        }
        else
        {
            // If no move happened, just end
            EndTurn();
        }
    }
}