using UnityEngine;
using System.Collections.Generic;

public class HighlightController : MonoBehaviour
{
    private List<Tile> highlightedTiles = new List<Tile>();

    public bool IsHighlighted(Tile tile)
    {
        return highlightedTiles.Contains(tile);
    }

    public void ClearHighlights()
    {
        for (int i = 0; i < highlightedTiles.Count; i++)
        {
            highlightedTiles[i].ResetColor();
        }

        highlightedTiles.Clear();
    }

    public void HighlightMoveRange(
        Dictionary<Vector2Int, Tile> gridDictionary,
        Tile origin,
        Unit unit
    )
    {
        if (unit == null || origin == null)
            return;

        ClearHighlights();

        foreach (var kvp in gridDictionary)
        {
            Tile tile = kvp.Value;

            Vector3Int a = OffsetToCube(origin.coordinates);
            Vector3Int b = OffsetToCube(tile.coordinates);

            int distance = Mathf.Max(
                Mathf.Abs(a.x - b.x),
                Mathf.Abs(a.y - b.y),
                Mathf.Abs(a.z - b.z)
            );

            if (distance > unit.moveRange)
                continue;

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
}