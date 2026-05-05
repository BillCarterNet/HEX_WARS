using UnityEngine;

public enum UnitState
{
    Idle,
    MovePreview,
    ActionMenu
}
public class Unit : MonoBehaviour
{
    public UnitState state;
    public string unitName;
    public int teamID; // 0 for Player, 1 for Enemy
    public int moveRange;
    public int attackRange;
    public Vector2Int gridPosition;
    public bool hasMoved;
    public Tile originalTile;
    private Tile currentTile;
    private GridManager manager;

    public void Initialize(string name, int team, int move, int range, Vector2Int startPos)
    {
        unitName = name;
        teamID = team;
        moveRange = move;
        attackRange = range;
        gridPosition = startPos;
        
        gameObject.name = $"{unitName} (Team {teamID})";
    }

    public void SetTile(Tile tile)
    {
        currentTile = tile;
    }

    public void SetManager(GridManager gridManager)
    {
        manager = gridManager;
    }

    void OnMouseDown()
    {
        if (manager != null && currentTile != null)
        {
            manager.SelectTile(currentTile);
        }
    }
}
