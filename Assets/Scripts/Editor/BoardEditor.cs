using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using SweetSugar.Scripts.System;
public enum BrushTypes
{
    None,
    Snake,
    Ladder,
    Remove,
    Clear,
    Random,
    DebugCell
}

public class Brush
{
    private BrushTypes brush;
    public int clicks;
    public Cell previousCell;

    //How to read this?
    //No. of clicks:  1             2
    //             SnakeHead      SnakeTail
    //           LadderBottom     LadderTop
    private int[,] paintData = { { (int)CellType.SnakeHead, (int)CellType.SnakeTail }, { (int)CellType.LadderBottom, (int)CellType.LadderTop } };

    public Brush(BrushTypes _brush, int _clicks)
    {
        brush = _brush;
        clicks = _clicks;
    }

    public void SetBrush(BrushTypes newBrush)
    {
        if(previousCell != null)
        {
            if (previousCell.cellType == CellType.SnakeHead
                || previousCell.cellType == CellType.LadderBottom)
            {
                previousCell.cellType = CellType.Blank;
                previousCell.link = -1;
            }
        }


        this.brush = newBrush;
        clicks = 0;
    }

    public BrushTypes GetCurrentBrush(){return this.brush;}

    public CellType Paint(Cell cell)
    {
        int brushTypeId;
        switch (brush)
        {
            case BrushTypes.Snake:
                brushTypeId = 0;
                if (cell.cellType == CellType.SnakeHead || cell.cellType == CellType.SnakeTail
                    || cell.cellType == CellType.LadderBottom || cell.cellType == CellType.LadderTop)
                {
                    Cell linkCell = LevelEditor.levelData.gameBoard.grid[cell.link - 1];
                    linkCell.cellType = CellType.Blank;
                    linkCell.link = -1;
                    cell.link = -1;
                }

                if (clicks == 1)
                {
                    LevelEditor.levelData.gameBoard.grid[cell.cellId - 1].link = previousCell.cellId;
                    //cell.link = previousCell.cellId;
                    //previousCell.link = cell.cellId;
                    LevelEditor.levelData.gameBoard.grid[previousCell.cellId - 1].link = cell.cellId;
                }
                break;
            case BrushTypes.Ladder:
                brushTypeId = 1;

                if (cell.cellType == CellType.SnakeHead || cell.cellType == CellType.SnakeTail
                    || cell.cellType == CellType.LadderBottom || cell.cellType == CellType.LadderTop)
                {
                    Cell linkCell = LevelEditor.levelData.gameBoard.grid[cell.link - 1];
                    linkCell.cellType = CellType.Blank;
                    linkCell.link = -1;
                    cell.link = -1;
                }

                if (clicks == 1)
                {
                    cell.link = previousCell.cellId;
                    //previousCell.link = cell.cellId;
                    LevelEditor.levelData.gameBoard.grid[previousCell.cellId - 1].link = cell.cellId;
                }
                break;
            case BrushTypes.Remove:
                if(cell.cellType == CellType.SnakeHead || cell.cellType == CellType.SnakeTail 
                    || cell.cellType == CellType.LadderBottom || cell.cellType == CellType.LadderTop)
                {
                    if(cell.link != -1)
                    {
                        LevelEditor.levelData.gameBoard.grid[cell.link - 1].cellType = CellType.Blank;
                        LevelEditor.levelData.gameBoard.grid[cell.link - 1].link = -1;
                        //cell.link.cellType = CellType.Blank;
                        //cell.link.link = null;
                        cell.link = -1;
                    }
                }
                return CellType.Blank;
            case BrushTypes.DebugCell:
                Debug.Log(cell.cellId + " | " + cell.cellType + " | " + cell.link);
                return cell.cellType;
            default:
                return cell.cellType;
        }
        if(clicks >= 2) clicks = 0;
        clicks++;
        this.previousCell = cell;
        return (CellType)paintData[brushTypeId, clicks-1];
    }
}

[InitializeOnLoad]
public class LevelEditor : EditorWindow
{
    public static LevelEditor window;

    private int levelId, prevLevelId;
    private static string levelPath = "Assets/Resources/Levels/";
    private IEnumerable<BrushTypes> brushToolTypeItems;


    private int maxRows, maxCols, prevMaxRows, prevMaxCols;

    public static LevelData levelData;
    private Brush brush;

    private void Awake()
    {
        Debug.Log("Awakened");
        
    }


    public void OnEnable()
    {
        Debug.Log("OnEnable");
        brushToolTypeItems = EnumUtil.GetValues<BrushTypes>();
        brush = new Brush(BrushTypes.None, 0);
    }

    [MenuItem("Snake Ladder/Level editor")]
    public static void OpenWindow()
    {
        // Get existing open window or if none, make a new one:
        window = (LevelEditor)GetWindow(typeof(LevelEditor), false, "Level Editor");
        window.Show();
    }

    private void OnFocus()
    {
        if(levelData == null)
        {
            levelData = new LevelData(levelId, "Level_" + levelId, new GameBoard(maxRows, maxCols));
        }
    }

    private void OnLostFocus()
    {
        
    }

    void OnCellSelect(Cell cell)
    {
        //Debug.Log(cell.cellId);
        //if (cell.link != -1)
        //{
        //    Debug.Log(cell.cellId);
        //    levelData.gameBoard.grid[cell.link - 1].link = -1;
        //    levelData.gameBoard.grid[cell.link - 1].cellType = CellType.Blank;
        //}
        cell.ChangeCellType(brush.Paint(cell));


    }

    void GridSizeChanged()
    {
        Debug.Log("grid changed!");
        levelData = new LevelData(levelId, "Level_" + levelId, new GameBoard(maxRows, maxCols));
        int dir = -1;
        int currentRowMax = maxCols * maxRows;
        for (int row = 0; row < maxRows; row++)
        {
            int flag = dir == 1 ? currentRowMax - (maxCols) : currentRowMax + 1;
            for (int col = 0; col < maxCols; col++)
            {
                int cellId = flag + dir;
                Cell cell = levelData.gameBoard.grid[cellId - 1];
                cell.cellId = cellId;
                flag = cellId;
            }
            dir = dir * -1;
            currentRowMax = currentRowMax - maxCols;
        }
    }

    void SaveLevel()
    {
        ScriptableLevelManager.CreateOrSaveFileLevel(levelData);
        maxRows = 0;
        maxCols = 0;
        levelId = 0;
        prevMaxCols = 0;
        prevMaxRows = 0;
        prevLevelId = 0;
    }
    void LoadLevel()
    {
        LevelData _levelData = ScriptableLevelManager.LoadLevel(levelId);
        if (_levelData == null)
        {
            Debug.LogError("No such level exists!");
            return;
        }
        levelData = _levelData;
        maxRows = _levelData.gameBoard.maxRows;
        maxCols = _levelData.gameBoard.maxCols;
        levelId = _levelData.levelId;
        prevMaxCols = maxCols;
        prevMaxRows = maxRows;
        prevLevelId = levelId;
    }

    private void OnGUI()
    {
        //Draw fields for row & col, level num & plug with save button & load.
        GUITools();
        GUILevelNum();
        GUIRowCol();
        if (GUILayout.Button("Save", GUILayout.Width(50), GUILayout.Height(50)))
        {
            //Save current data & wipe fields
            //LevelData _levelData = new LevelData(levelId, "Level_" + levelId, new GameBoard(maxRows, maxCols));
            SaveLevel();
        }

        if (GUILayout.Button("Load", GUILayout.Width(50), GUILayout.Height(50)))
        {
            //Load current data & populate fields
            LoadLevel();
        }
        GUIGrid();
    }

    void GUIRowCol()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Column", GUILayout.Width(50));
        GUILayout.Space(100);
        maxCols = EditorGUILayout.IntField("", prevMaxCols, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Rows", GUILayout.Width(50));
        GUILayout.Space(100);
        maxRows = EditorGUILayout.IntField("", prevMaxRows, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (prevMaxCols != maxCols || prevMaxRows != maxRows)
        {
            //Grid changed!
            GridSizeChanged();
        }

        prevMaxCols = maxCols;
        prevMaxRows = maxRows;
    }

    void GUILevelNum()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("LevelId", GUILayout.Width(50));
        GUILayout.Space(100);
        levelId = EditorGUILayout.IntField("", prevLevelId, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        if(prevLevelId != levelId)
        {
            //LevelId changed!
        }
        prevLevelId = levelId;
    }

    void GUIGrid()
    {
        GUILayout.BeginVertical();

        int dir = -1;
        int currentRowMax = maxCols * maxRows;

        for (int row = 0; row < maxRows; row++)
        {
            GUILayout.BeginHorizontal();
            int flag = dir == 1 ? currentRowMax - (maxCols) : currentRowMax + 1;
            for (int col = 0; col < maxCols; col++)
            {
                int cellId = flag + dir;
                if (cellId == 1) GUI.color = Color.red;                
                else if (cellId == maxRows * maxCols) GUI.color = Color.green;
                else GUI.color = Color.white;

                Cell cell = levelData.gameBoard.grid[cellId - 1];

                if (GUILayout.Button(new GUIContent(cellId.ToString(), (cell.cellId + " | " + cell.cellType + " | " + cell.link)), GUILayout.Width(50), GUILayout.Height(50)))
                {
                    OnCellSelect(cell);
                }
                flag = cellId;
            }
            dir = dir * -1;
            currentRowMax = currentRowMax - maxCols;
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    void GUITools()
    {
        GUILayout.BeginHorizontal();
        {
            //GUILayout.Space(30);
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Tools:", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                {
                    UnityEngine.GUI.color = new Color(1, 1, 1, 1f);

                    foreach (BrushTypes brushType in brushToolTypeItems)
                    {
                        if (GUILayout.Button(new GUIContent(brushType.ToString()), GUILayout.Width(50), GUILayout.Height(50)))
                        {
                            brush.SetBrush(brushType);
                            
                        }
                    }
                    UnityEngine.GUI.color = new Color(1, 1, 1, 1f);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("Current Tool: " + brush.GetCurrentBrush().ToString(), EditorStyles.whiteMiniLabel);
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
    }
}
