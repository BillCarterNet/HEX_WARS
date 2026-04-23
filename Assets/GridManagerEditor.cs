using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridManager grid = (GridManager)target;

        if (GUILayout.Button("Generate Grid"))
        {
            grid.GenerateGrid();
        }
    }
}
