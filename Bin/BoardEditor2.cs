using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using SweetSugar.Scripts.System;

public enum BrushTools
{
    None,
    Snake,
    Ladder,
    Remove,
    Clear,
    Random
}

[Serializable]
public class Link
{
    public Cell head;
    public Cell tail;

    public Link(Cell h, Cell t)
    {
        head = h;
        tail = t;
    }

    public Link DeepCopy()
    {
        var other = (Link)MemberwiseClone();
        return other;
    }
}


public enum CellType
{
    Blank,
    LadderTop,
    LadderBottom,
    SnakeHead,
    SnakeTail
}
[Serializable]
public class Cell
{
    public int cellId;
    public int cellType = (int)CellType.Blank;
    public Link link;

    public Cell()
    {

    }

    public Cell(int _cellId, int _cellType, Link _link)
    {
        cellId = _cellId;
        cellType = _cellType;
        link = _link;
    }

    public Cell DeepCopy()
    {
        var other = (Cell)MemberwiseClone();
        if (link != null)
            other.link = link.DeepCopy();
        else
            other.link = null;
        other.cellType = this.cellType;
        return other;
    }
}

[Serializable]
public class LevelData
{
    public Cell[] gameBoard;
    public string levelName;
    public int levelNum;
    public int maxRows;
    public int maxCols;

    public LevelData(Cell[] _gm, int levelNum, int maxRows, int maxCols)
    {
        gameBoard = _gm;
        InitGameBoardList();
        levelName = "Level_" + levelNum;
        this.levelNum = levelNum;
        this.maxRows = maxRows;
        this.maxCols = maxCols;
    }

    void InitGameBoardList()
    {
        for(int i=0; i<gameBoard.Length; i++)
        {
            gameBoard[i] = new Cell();
        }
    }

    public LevelData DeepCopy(int levelNum)
    {
        var other = (LevelData)MemberwiseClone();
        other.levelNum = levelNum;
        other.levelName = "Level_" + levelNum;
        other.gameBoard = new Cell[maxRows * maxCols];
        for (int i = 0; i < other.gameBoard.Length; i++)
        {
            //Debug.Log(gameBoard[i].cellId + ": " + (CellType)gameBoard[i].cellType);
            other.gameBoard[i] = gameBoard[i].DeepCopy();
        }
        
        return other;
    }
}

[InitializeOnLoad] // ??
public class BoardEditor2 : EditorWindow
{
    private int maxCols, maxRows;
    private int prevMaxCols, prevMaxRows;
    private static BoardEditor2 window;
    BrushTools currentBrush = BrushTools.None;
    LevelData levelData;
    //Cell[] gameField;
    Cell previousCell = null;
    int sameBrushClick = 0;

    private int levelNumber = 1;
    string levelPath = "Assets/Resources/Levels/";

    private GameObject emptyBlock;
    Texture2D emptyBlockTexture;

    private IEnumerable<BrushTools> brushToolTypeItems;

    [MenuItem("Snake Ladder/Level editor")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        window = (BoardEditor2)GetWindow(typeof(BoardEditor2), false, "Level Editor");
        window.Show();
        
        Debug.Log("Init get called!");
    }

    private void Initialize()
    {
        brushToolTypeItems = EnumUtil.GetValues<BrushTools>();

        levelData = new LevelData(new Cell[maxRows * maxCols], levelNumber, maxRows, maxCols);

        int dir = -1;
        int currentRowMax = maxCols * maxRows;

        for (int row = 0; row < maxRows; row++)
        {
            int flag = dir == 1 ? currentRowMax - (maxCols) : currentRowMax + 1;
            for (int col = 0; col < maxCols; col++)
            {
                int cellId = flag + dir;
                levelData.gameBoard[cellId - 1].cellId = cellId;
                flag = cellId;
            }
            dir = dir * -1;
            currentRowMax = currentRowMax - maxCols;
        }
    }

    private void OnFocus()
    {
        levelData = ScriptableLevelManager.LoadLevel(levelNumber);
        if(levelData == null)
        {
            Initialize();
            //levelData = new LevelData(new Cell[maxRows * maxCols], levelNumber, maxRows, maxCols);
        }
    }

    private void OnLostFocus()
    {
        //SaveLevel();
        ScriptableLevelManager.CreateFileLevel(levelNumber, levelData);
    }

    void SaveLevel()
    {
        var path = "Assets/Resources/Levels/";

        if (Resources.Load("Levels/Level_" + 1))
        {
            //SaveLevel(path, level, _levelData);
        }
        else
        {
            string fileName = "Level_" + 1;
            var newLevelData = ScriptableObjectUtility.CreateAsset<LevelContainer>(path, fileName);
            newLevelData.SetData(levelData.DeepCopy(1));
            EditorUtility.SetDirty(newLevelData);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI()
    {
        //Initialize();
        brushToolTypeItems = EnumUtil.GetValues<BrushTools>();
        GUILevelSelector();
        GUIRowCol();
        GUITools();
        GUIGameField();

        if(GUILayout.Button(new GUIContent("Debug Board"), GUILayout.Width(50), GUILayout.Height(50)))
        {
            DebugGameBoard();
        }
    }

    void LoadTexture()
    {
        if (emptyBlock == null)
        {
            emptyBlock = Resources.Load("Blocks/" + "EmptyBlock") as GameObject;
            emptyBlockTexture = emptyBlock.GetComponent<SpriteRenderer>().sprite.texture;
        }
    }

    void GUILevelSelector()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Level editor", GUILayout.Width(150));
            if (GUILayout.Button("Test level", GUILayout.Width(158)))
            {
                TestLevel();
            }

            if (GUILayout.Button("Save", GUILayout.Width(50)))
            {
                SaveLevel(levelNumber);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level", GUILayout.Width(50));
            GUILayout.Space(100);
            if (GUILayout.Button("<<", GUILayout.Width(50)))
            {
                PreviousLevel();
            }
            string changeLvl = GUILayout.TextField(" " + levelNumber, GUILayout.Width(50));
            try
            {
                if (int.Parse(changeLvl) != levelNumber)
                {
                    LevelData ld = LoadLevel(int.Parse(changeLvl));
                    if (ld!=null)
                    {
                        levelData = ld;
                        levelNumber = int.Parse(changeLvl);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            if (GUILayout.Button(">>", GUILayout.Width(50)))
            {
                NextLevel();
            }

            if (GUILayout.Button(new GUIContent("+", "add level"), GUILayout.Width(20)))
            {
                AddLevel();
            }

            if (GUILayout.Button(new GUIContent("- ", "remove current level"), GUILayout.Width(20)))
            {
                RemoveLevel();
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
    }

    void GUIRowCol()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Column", GUILayout.Width(50));
        GUILayout.Space(100);
        maxCols = EditorGUILayout.IntField("", maxCols, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Rows", GUILayout.Width(50));
        GUILayout.Space(100);
        maxRows = EditorGUILayout.IntField("", maxRows, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if(prevMaxCols != maxCols || prevMaxRows != maxRows)
        {
            Initialize();
        }

        prevMaxCols = maxCols;
        prevMaxRows = maxRows;
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

                    foreach (BrushTools brushType in brushToolTypeItems)
                    {
                        if(GUILayout.Button(new GUIContent(brushType.ToString()), GUILayout.Width(50), GUILayout.Height(50)))
                        {
                            Debug.Log(brushType.ToString());
                            currentBrush = brushType;
                            sameBrushClick = 0;
                        }
                    }
                    UnityEngine.GUI.color = new Color(1, 1, 1, 1f);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
    }

    void GUIGameField()
    {
        //List<RectTexture> rects = new List<RectTexture>();
        GUILayout.BeginVertical();
            
        int dir = -1;
        int currentRowMax = maxCols * maxRows;
        
        for (int row = 0; row < maxRows; row++)
        {
            GUILayout.BeginHorizontal();
            int flag = dir == 1 ? currentRowMax - (maxCols) : currentRowMax + 1;
            for (int col = 0; col < maxCols; col++)
            {
                //if (emptyBlockTexture == null)
                //{
                //    Debug.LogError("Cell texture is null");
                //    return;
                //}

                int cellId = flag + dir;
                if(cellId == 1)
                {
                    GUI.color = Color.red;
                }
                else if(cellId == maxRows * maxCols)
                {
                    GUI.color = Color.green;
                }
                else {
                    GUI.color = Color.white;
                }

                levelData.gameBoard[cellId - 1].cellId = cellId;
                //levelData.gameBoard[cellId - 1].cellType = (int)CellType.Blank;
                if (GUILayout.Button(new GUIContent(cellId.ToString()), GUILayout.Width(50), GUILayout.Height(50)))
                {
                    OnCellSelect(cellId);
                }
                flag = cellId;
            }
            dir = dir * -1;
            currentRowMax = currentRowMax - maxCols;
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    void OnCellSelect(int cellId)
    {
        Debug.Log("cell: " + cellId);
        if (currentBrush == BrushTools.None)
            currentBrush = BrushTools.Remove;

        //Cell currentCell = gameField[cellId - 1];
        int currentCellIndex = cellId - 1;

        Debug.Log(currentBrush);
        switch (currentBrush)
        {
            case BrushTools.None:
                break;
            case BrushTools.Snake:
                Debug.Log("comes here");
                if (sameBrushClick >= 2)
                    sameBrushClick = 0;
                if (sameBrushClick == 0) {
                    Debug.Log("first click!" + currentBrush);
                    // put snake head
                    //levelData.gameBoard[currentCellIndex] = new Cell(levelData.gameBoard[currentCellIndex].cellId, levelData.gameBoard[currentCellIndex].cellType, new Link(levelData.gameBoard[currentCellIndex], null));
                    
                    levelData.gameBoard[currentCellIndex].cellType = (int)CellType.SnakeHead;
                    Debug.Log(levelData.gameBoard[currentCellIndex].cellId + ": " + levelData.gameBoard[currentCellIndex].cellType);
                    levelData.gameBoard[currentCellIndex].link = new Link(levelData.gameBoard[currentCellIndex], null);
                }
                else if(sameBrushClick == 1)
                {
                    Debug.Log("second click!" + currentBrush);
                    // put snake tail
                    levelData.gameBoard[currentCellIndex].cellType = (int)CellType.SnakeTail;
                    levelData.gameBoard[currentCellIndex].link = new Link(previousCell, levelData.gameBoard[currentCellIndex]);
                    previousCell.link.tail = levelData.gameBoard[currentCellIndex];
                }
                break;
            case BrushTools.Ladder:
                if (sameBrushClick >= 2)
                    sameBrushClick = 0;
                if (sameBrushClick == 0)
                {
                    // put ladder bottom
                    levelData.gameBoard[currentCellIndex].cellType = (int)CellType.LadderBottom;
                    levelData.gameBoard[currentCellIndex].link = new Link(null, levelData.gameBoard[currentCellIndex]);
                }
                else if (sameBrushClick == 1)
                {
                    // put ladder top
                    levelData.gameBoard[currentCellIndex].cellType = (int)CellType.LadderTop;
                    levelData.gameBoard[currentCellIndex].link = new Link(levelData.gameBoard[currentCellIndex], previousCell);
                    previousCell.link.head = levelData.gameBoard[currentCellIndex];
                }
                break;
            case BrushTools.Remove:
                if (levelData.gameBoard[currentCellIndex].cellType == (int)CellType.Blank)
                    break;
                switch ((CellType)levelData.gameBoard[currentCellIndex].cellType)
                {
                    case CellType.LadderTop:
                        levelData.gameBoard[currentCellIndex].cellType = (int)CellType.Blank;
                        levelData.gameBoard[currentCellIndex].link.tail.cellType = (int)CellType.Blank;
                        levelData.gameBoard[currentCellIndex].link.tail.link = null;
                        levelData.gameBoard[currentCellIndex].link = null;
                        break;
                    case CellType.LadderBottom:
                        levelData.gameBoard[currentCellIndex].cellType = (int)CellType.Blank;
                        levelData.gameBoard[currentCellIndex].link.head.cellType = (int)CellType.Blank;
                        levelData.gameBoard[currentCellIndex].link.head.link = null;
                        levelData.gameBoard[currentCellIndex].link = null;
                        break;
                    case CellType.SnakeHead:
                        levelData.gameBoard[currentCellIndex].cellType = (int)CellType.Blank;
                        levelData.gameBoard[currentCellIndex].link.tail.cellType = (int)CellType.Blank;
                        levelData.gameBoard[currentCellIndex].link.tail.link = null;
                        levelData.gameBoard[currentCellIndex].link = null;
                        break;
                    case CellType.SnakeTail:
                        levelData.gameBoard[currentCellIndex].cellType = (int)CellType.Blank;
                        levelData.gameBoard[currentCellIndex].link.head.cellType = (int)CellType.Blank;
                        levelData.gameBoard[currentCellIndex].link.head.link = null;
                        levelData.gameBoard[currentCellIndex].link = null;
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        sameBrushClick += 1;

        previousCell = levelData.gameBoard[currentCellIndex];
        //DebugGameBoard();
    } 

    void DebugGameBoard()
    {
        foreach (Cell cell in levelData.gameBoard)
        {
            //if(cell.link != null)
            //    Debug.Log(cell.cellId + ", " + cell.cellType + ", " + cell.link.head.cellId + " -> " + cell.link.tail?.cellId);
            //else
                Debug.Log(cell.cellId + ": " + (CellType)cell.cellType);
        }
        Debug.Log("***********************");
    }

    public static int GetLastLevelNum()
    {
        return Resources.LoadAll<LevelContainer>("Levels").Length;
    }

    private int GetLastLevel()
    {
        int lastLevel = GetLastLevelNum();
        if (lastLevel == 0) lastLevel = 1;
        return lastLevel;
    }

    void TestLevel()
    {

    }
    void SaveLevel(int levelNum)
    {
        ScriptableLevelManager.SaveLevel(levelPath, levelNum, levelData);
    }

    LevelData LoadLevel(int levelNum)
    {
        LevelData ld = null;
        ld = ScriptableLevelManager.LoadLevel(levelNum);
        return ld;
    }

    void PreviousLevel()
    {

    }
    void NextLevel()
    {

    }

    void AddLevel()
    {
        SaveLevel(levelNumber);
        levelNumber = GetLastLevel() + 1;
        Initialize();
        ScriptableLevelManager.CreateFileLevel(levelNumber, levelData);
        levelData = ScriptableLevelManager.LoadLevel(levelNumber);
        //Initialize();
        ClearLevel();
        SaveLevel(levelNumber);
    }

    void ClearLevel()
    {
        //levelData 
    }

    void RemoveLevel()
    {

    }
}
