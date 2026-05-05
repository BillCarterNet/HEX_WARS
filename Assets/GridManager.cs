using UnityEngine;
using System.Collections.Generic;

public enum UnitAction
{
    Attack,
    End,
    Cancel
}
public class GridManager : MonoBehaviour
{
    public SelectionController selection;
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
    // This "Dictionary" lets us look up a Tile using a Vector2Int coordinate
    private Dictionary<Vector2Int, Tile> gridDictionary = new Dictionary<Vector2Int, Tile>();
    private List<Tile> highlightedTiles = new List<Tile>();
    private bool isActionMenuActive = false;

    // =========================
    // SELECTION SYSTEM
    // =========================
    void Awake()
    {
        selection = GetComponent<SelectionController>();
    }
    public void SelectTile(Tile newTile)
    {
        if (selection == null)
        {
            Debug.LogError("SelectionController is NULL");
            return;
        }
        Debug.Log($"Selected tile: {newTile.coordinates}");

        var unit = selection.SelectedUnit;

        // 1. MOVE FIRST
        if (unit != null && highlightedTiles.Contains(newTile))
        {
            if (newTile.currentUnit != null)
                return;

            MoveUnit(unit, newTile, true);
            return;
        }
        else if (selection.SelectedUnit != null && isActionMenuActive)
        {
            Debug.Log("Invalid click during action → cancelling");

            CancelAction();
            return;
        }

        // 2. clicking same unit → open menu
        if (unit != null && newTile.currentUnit == unit)
        {
            if (unit.state == UnitState.MovePreview)
            {
                unit.state = UnitState.ActionMenu;
                ShowActionMenu(unit);
            }
            return;
        }

        // 3. reset visuals
        if (selection.SelectedTile != null)
        {
            selection.SelectedTile.ResetColor();
        }

        ClearHighlights();

        // 4. select tile ONLY
        selection.Select(null, newTile);
        if (selection.SelectedTile != null)
        {
            selection.SelectedTile.Highlight();
        }

        // 5. unit selection
        if (newTile.currentUnit != null)
        {
            var selected = newTile.currentUnit;

            selection.Select(selected, newTile);

            selected.state = UnitState.MovePreview;
            selected.originalTile = newTile;
            selected.hasMoved = false;

            Debug.Log($"Selected unit: {selected.unitName}");

            HighlightMoveRange(newTile, selected);
        }
    }

    // =========================
    // MOVEMENT SYSTEM
    // =========================
    void MoveUnit(Unit unit, Tile targetTile, bool showMenu = true)
    {
        //Debug.Log($"MoveUnit | unit={unit} | targetTile={targetTile} | selection={(selection!=null)} | selUnit={selection?.SelectedUnit} | selTile={selection?.SelectedTile}"); 
        // Update unit state and hasMoved flag
        unit.state = UnitState.ActionMenu;
        unit.hasMoved = true;
        Tile oldTile = null;

        // 🚫 Block movement if occupied
        if (unit.transform.parent != null)
        {
            oldTile = unit.transform.parent.GetComponent<Tile>();
        }

        // Clear old tile's reference to the unit
        if (oldTile != null)
        {
            oldTile.currentUnit = null;
        }

        targetTile.currentUnit = unit;

        // Move the unit GameObject to the new tile
        unit.transform.SetParent(targetTile.transform);
        unit.transform.localPosition = new Vector3(0, 0.5f, 0);
        unit.gridPosition = targetTile.coordinates;
        unit.SetTile(targetTile);

        Debug.Log($"Moved {unit.unitName} to {targetTile.coordinates}");

        // Reset the old selected tile colour
        if (selection.SelectedTile != null)
        {
            selection.SelectedTile.ResetColor();
        }
        unit.hasMoved = true;
        ClearHighlights();

        // Show action menu if we just moved
        if (showMenu)
        {
            ShowActionMenu(unit);
        }
    }

    // =========================
    // UNIT SPAWNING
    // =========================
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

            // 4. Set up the Unit script
            Unit unitScript = unitGO.GetComponent<Unit>();
            if (unitScript != null)
            {
                // Initialize the unit with its name, team, move range, and starting position
                unitScript.Initialize(unitName, team, moveRange, 2, coord);
                unitScript.SetManager(this);
                targetTile.currentUnit = unitScript;
                unitScript.SetTile(targetTile);

                // 5. Set material based on team
                Renderer r = unitGO.GetComponentInChildren<Renderer>();
                Debug.Log($"Setting material for {unitName} (Team {team})");
                Debug.Log(r);

                //
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

    // =========================
    // GRID GENERATION
    // =========================
    void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
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

                // Instantiate the tile prefab at the calculated position
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

        // Position the camera to see the whole grid
        Camera.main.transform.position = new Vector3(
            width * 0.5f,
            10f,
            -1 *height * 0.5f
        );
        Camera.main.transform.rotation = Quaternion.Euler(60f, 0f, 0f); 
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

    // =========================
    // HIGHLIGHTING SYSTEM
    // =========================
    void ClearHighlights()
    {
        for (int i = 0; i < highlightedTiles.Count; i++)
        {
            highlightedTiles[i].ResetColor();
        }
        highlightedTiles.Clear();
    }

    void HighlightMoveRange(Tile origin, Unit unit)
    {
        Debug.Log($"HighlightMoveRange → unit: {unit.unitName}, moveRange: {unit.moveRange}");
        if (unit == null || origin == null)
        return;

        ClearHighlights();

        foreach (var kvp in gridDictionary)
        {
            Tile tile = kvp.Value;

            // Convert BOTH tiles to cube coordinates first
            Vector3Int a = OffsetToCube(origin.coordinates);
            Vector3Int b = OffsetToCube(tile.coordinates);

            // Then calculate distance using the cube coordinates
            int distance = Mathf.Max(
                Mathf.Abs(a.x - b.x),
                Mathf.Abs(a.y - b.y),
                Mathf.Abs(a.z - b.z)
            );

            // 🚫 Skip tiles outside of move range
            if (distance > unit.moveRange)
                continue;

            // 🚫 Skip occupied tiles
            if (tile.currentUnit != null)
                continue;

            // If we made it here, it's in range and unoccupied - highlight it!
            tile.HighlightMoveRange();
            highlightedTiles.Add(tile);
        }
    }

    // =========================
    // ACTION MENU
    // =========================
    void ShowActionMenu(Unit unit)
    {
        // Activate the action menu state
        isActionMenuActive = true;
        Debug.Log("=== ACTION MENU ===");
        Debug.Log($"hasMoved = {unit.hasMoved}");

        // If no unit is selected, we can't show any actions
        if (unit == null)
        {
            return;
        }
        Debug.Log($"Unit: {unit.unitName}");

        // Show available actions based on whether the unit has moved or not
        if (!unit.hasMoved)
            Debug.Log("Available: Attack, End, Cancel");
        else
            Debug.Log("Available: End, Cancel");
    }

    void EndTurn()
    {
        Debug.Log("END TURN");

        isActionMenuActive = false;

        ClearHighlights();

        var unit = selection.SelectedUnit;

        if (unit != null)
        {
            unit.hasMoved = false;
            unit.originalTile = null;
            unit.state = UnitState.Idle;
        }

        selection.Select(null, null);
    }

    void Update()
    {
        // If the action menu isn't active, we don't want to process these inputs
        if (!isActionMenuActive) return;

        // Listen for "End Turn" and "Cancel" inputs
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

        var unit = selection.SelectedUnit;

        if (unit == null)
            return;

        if (unit.hasMoved && unit.originalTile != null)
        {
            MoveUnit(unit, unit.originalTile, false);
            unit.hasMoved = false;
        }

        unit.state = UnitState.Idle;

        ClearHighlights();
        isActionMenuActive = false;

        selection.Select(null, null);
    }
}