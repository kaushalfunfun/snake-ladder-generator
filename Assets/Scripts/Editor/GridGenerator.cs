using UnityEditor;
using UnityEngine;

public class GridGenerator : EditorWindow
{
    #region GridSettings
    Vector2 gridSize = new Vector2(0, 0);
    Vector2 roomPadding = new Vector2(1, 1);
    Vector2 startingCoordinates = new Vector2(0, 0);
    Vector2 roomDimensions = new Vector2(4f, 2.5f);

    Object emptyRoomObject = null;
    GameObject emptyRoomGameObject = null;

    private Vector2 truePadding;

    GameObject gridManager;
    #endregion

    [MenuItem("Window/Grid Generator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GridGenerator));
    }

    void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        gridSize = EditorGUILayout.Vector2Field("Amount of Columns and Rows", gridSize);
        roomPadding = EditorGUILayout.Vector2Field("Amount of Padding between Rooms", roomPadding);
        roomDimensions = EditorGUILayout.Vector2Field("Room Dimensions", roomDimensions);
        startingCoordinates = EditorGUILayout.Vector2Field("Starting Coordinates", startingCoordinates);
        EditorGUILayout.Space();

        emptyRoomObject = EditorGUILayout.ObjectField("EmptyRoom Prefab", emptyRoomObject, typeof(GameObject), true);

        EditorGUILayout.Space();


        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Grid"))
        {
            //Configure the grids properties
            ApplyGridSettings();

            //if we already have a grid, then destroy it
            DestroyOldGrid();

            //Generate the grid
            GenerateGrid();
        }
    }

    private void ApplyGridSettings()
    {
        emptyRoomGameObject = emptyRoomObject as GameObject;

        truePadding = roomPadding * roomDimensions;

    }

    private void GenerateGrid()
    {
        Vector3 spawnPos;
        gridManager = new GameObject("Grid Manager");
        //Starts a loop for instantiating the Grid
        for (float x = 0; x < gridSize.x * truePadding.x; x += truePadding.x)
        {
            for (float y = 0; y < gridSize.y * truePadding.y; y += truePadding.y)
            {
                spawnPos = new Vector3(startingCoordinates.x + x, startingCoordinates.y + y, -1f);

                GameObject spawnedRoom = Instantiate(emptyRoomGameObject, spawnPos, Quaternion.identity);
                spawnedRoom.transform.parent = gridManager.transform;

            }
        }
    }

    private void DestroyOldGrid()
    {
        if (gridManager != null)
        {
            DestroyImmediate(gridManager);
        }
    }
}