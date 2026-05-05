using UnityEngine;

public class SelectionController : MonoBehaviour
{
    public Unit SelectedUnit { get; private set; }
    public Tile SelectedTile { get; private set; }

    public void Select(Unit unit, Tile tile)
    {
        SelectedUnit = unit;
        SelectedTile = tile;
    }

    public void Clear()
    {
        SelectedUnit = null;
        SelectedTile = null;
    }
}
