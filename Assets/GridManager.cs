using UnityEngine;
using System.Collections.Generic;

public enum UnitAction
{
    Attack,
    End,
    Cancel
}

// GridManager responsibilities:
// - Grid generation
// - Unit movement
// - Tile highlighting
// - Unit spawning
//
// Selection state is owned by SelectionController.

public class GridManager : MonoBehaviour
{
    public int width = 15;
    public int height = 15;
    public float hexSize = 0.75f;

    public SelectionController selection;
    public HighlightController highlights;
    
    public GameObject tilePrefab;
    public GameObject assaultPrefab;
    public GameObject sniperPrefab;
    public Material playerMaterial;
    public Material enemyMaterial;

    // This "Dictionary" lets us look up a Tile using a Vector2Int coordinate
    private Dictionary<Vector2Int, Tile> gridDictionary = new Dictionary<Vector2Int, Tile>();
    private bool isActionMenuActive = false;

    // =========================
    // SELECTION SYSTEM
    // =========================

    void Awake()
    {
        selection = GetComponent<SelectionController>();
        highlights = GetComponent<HighlightController>();
    }

    public void SelectTile(Tile newTile)
    {
        // 🚫 If we don't have a reference to the SelectionController, we can't do anything!
        if (selection == null)
        {
            Debug.LogError("SelectionController is NULL");
            return;
        }
        Debug.Log($"Selected tile: {newTile.coordinates}");

        // Cache the currently selected unit for easy access
        var unit = selection.SelectedUnit;

        // 1. MOVE FIRST
        if (unit != null && highlights.IsHighlighted(newTile))
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

        highlights.ClearHighlights();

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

            highlights.HighlightMoveRange(gridDictionary, newTile, selected);
        }
    }

    // =========================
    // MOVEMENT SYSTEM
    // =========================

    void MoveUnit(Unit unit, Tile targetTile, bool showMenu = true)
    {
        // Update unit state and hasMoved flag
        unit.state = UnitState.ActionMenu;
        Tile oldTile = null;

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
        highlights.ClearHighlights();

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

                // Set the material based on the unit's team
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

        highlights.ClearHighlights();

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

        highlights.ClearHighlights();
        isActionMenuActive = false;

        selection.Select(null, null);
    }
}