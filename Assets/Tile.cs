using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int coordinates;
    public Unit currentUnit;

    private Renderer rend;
    private Color originalColor;
    private GridManager manager; // Reference to the boss

    public void Setup(int x, int z, GridManager gridManager)
    {
        coordinates = new Vector2Int(x, z);
        gameObject.name = $"Tile_{x}_{z}";
        manager = gridManager; // Store the reference
        
        rend = GetComponent<Renderer>();
        if (rend != null) originalColor = rend.material.color;
    }

    void OnMouseDown()
    {
        Debug.Log($"TILE CLICKED {coordinates}");
        manager.SelectTile(this);
    }

    public void ResetColor()
    {
        if (rend != null)
            rend.material.color = originalColor;
    }

    public void Highlight()
    {
        if (rend != null)
            rend.material.color = Color.yellow;
    }

    public void HighlightMoveRange()
    {
        if (rend != null)
            rend.material.color = Color.green;
    }
}
