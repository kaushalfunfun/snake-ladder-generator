using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region CellRelated
public enum CellType
{
    Blank,
    SnakeHead,
    SnakeTail,
    LadderTop,
    LadderBottom
}

[Serializable]
public class Link
{
    public Cell top, bottom;

    public Link(Cell t, Cell b)
    {
        top = t;
        bottom = b;
    }

    public Link DeepCopy()
    {
        var other = (Link)MemberwiseClone();
        return other;
    }
}

[Serializable]
public class Cell
{
    public int cellId;
    public CellType cellType;
    [SerializeField] public int link = -1;

    public Cell()
    {
        link = -1;
    }

    public Cell(int _cellId, CellType ct, int l=-1)
    {
        cellId = _cellId;
        cellType = ct;
        link = l;
    }

    public Cell DeepCopy()
    {
        var other = (Cell)MemberwiseClone();
        //other.link = -1;
        //if (link != null)
        //    other.link = new Cell(this.link.cellId, this.link.cellType);
        other.cellType = cellType;
        return other;
    }

    public void ChangeCellType(CellType newType)
    {
        this.cellType = newType;
    }
}
#endregion

[Serializable]
public class GameBoard
{
    public int maxRows;
    public int maxCols;
    public Cell[] grid;

    public GameBoard (int maxRows, int maxCols)
    {
        this.maxCols = maxCols;
        this.maxRows = maxRows;

        grid = new Cell[maxRows * maxCols];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new Cell();
        }
    }

    public GameBoard(int maxRows, int maxCols, Cell[] grid)
    {
        this.maxRows = maxRows;
        this.maxCols = maxCols;
        this.grid = grid;
    }

    public GameBoard DeepCopy()
    {
        var other = (GameBoard)MemberwiseClone();
        other.grid = new Cell[maxRows * maxCols];
        for (int i = 0; i < other.grid.Length; i++)
        {
            other.grid[i] = grid[i].DeepCopy();
        }
        return other;
    }
}
[Serializable]
public class LevelData
{
    public string levelName;
    public int levelId;
    public GameBoard gameBoard;

    public LevelData (int levelId)
    {
        this.levelId = levelId;
        this.levelName = "Level_" + levelId;
    }

    public LevelData (int levelId, string levelName, GameBoard gameBoard)
    {
        this.levelName = levelName;
        this.levelId = levelId;
        this.gameBoard = gameBoard;
    }
    
    public LevelData DeepCopy()
    {
        var other = (LevelData)MemberwiseClone();
        other.gameBoard = gameBoard.DeepCopy();
        return other;
    }
}
