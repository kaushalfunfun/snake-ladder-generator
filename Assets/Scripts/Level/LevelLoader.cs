using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SweetSugar.Scripts.System;
using TMPro;

[RequireComponent(typeof(UIManager))]
public class LevelLoader : MonoBehaviour
{
    public float turnDelay = 1;
    public int startLevel = 1;
    [SerializeField] int currentLevelId;
    LevelData currentLevel = null;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject board;
    [SerializeField] List<Tile> grid = new List<Tile>();
    [SerializeField] Player playerPrefab;
    [SerializeField] Material[] playerMaterials;
    [SerializeField] List<Player> players = new List<Player>();

    [SerializeField] int noOfPlayers = -1;
    int currentTurn;

    UIManager uIManager;

    private void Awake()
    {
        uIManager = GetComponent<UIManager>();
    }

    public void ResetPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Destroy(players[i].gameObject);
        }
        players = new List<Player>();
    }

    public void StartGame(int nop)
    {
        currentLevel = ScriptableLevelManager.LoadLevel(startLevel);
        if(currentLevel == null)
        {
            Debug.LogError("Could not find level with the name " + "Level_" + startLevel);
            return;
        }

        currentLevelId = startLevel;
        noOfPlayers = nop + 2;
        PopulateBoard();
        InitializeGame();
    }

    public void NextLevel()
    {
        currentLevelId++;
        currentLevel = ScriptableLevelManager.LoadLevel(currentLevelId);
        if (currentLevel == null)
        {
            Debug.LogError("Could not find level with the name " + "Level_" + currentLevelId);
            Debug.Log("GAME END!");
            return;
        }
        PopulateBoard();
        InitializeGame();
    }

    private void PopulateBoard()
    {
        ClearBoard();

        int dir = -1;
        int currentRowMax = currentLevel.gameBoard.maxCols * currentLevel.gameBoard.maxRows;

        for (int row = 0; row < currentLevel.gameBoard.maxRows; row++)
        {
            int flag = dir == 1 ? currentRowMax - (currentLevel.gameBoard.maxCols) : currentRowMax + 1;
            for (int col = 0; col < currentLevel.gameBoard.maxCols; col++)
            {
                int cellId = flag + dir;
                Tile newTile = Instantiate(tilePrefab).GetComponent<Tile>();
                newTile.name = cellId.ToString();
                newTile.transform.position = new Vector3(col, 0, -row);
                newTile.transform.parent = board.transform;
                newTile.cellId = cellId;
                newTile.cellType = currentLevel.gameBoard.grid[cellId - 1].cellType;
                if(currentLevel.gameBoard.grid[cellId - 1].link != -1)
                    newTile.link = currentLevel.gameBoard.grid[cellId - 1].link;
                newTile.SetTileNumber();
                grid.Add(newTile);
                flag = cellId;
            }
            dir = dir * -1;
            currentRowMax = currentRowMax - currentLevel.gameBoard.maxCols;
        }
        grid.Sort();
    }

    private void ClearBoard()
    {
        foreach(Tile t in grid)
        {
            Destroy(t.gameObject);
        }
        grid = new List<Tile>();
    }

    private void InitializeGame()
    {
        ResetPlayers();
        for (int i = 0; i < noOfPlayers; i++)
        {
            Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
            newPlayer.SetColor(i+1, playerMaterials[i]);
            newPlayer.ChangeLocation(grid[0]);
            newPlayer.transform.position = grid[0].transform.position;
            players.Add(newPlayer);
        }
        currentTurn = 0;
        uIManager.rollButton.interactable = true;
    }

    public void EvaluateRoll(int roll)
    {
        StartCoroutine("DieRolled", roll);
    }

    private IEnumerator DieRolled(int roll)
    {
        int currentCellId = players[currentTurn].currentLocation.cellId;
        int lastCellId = grid[grid.Count - 1].cellId;
        if(currentCellId + roll > lastCellId)
        {
            yield return new WaitForSeconds(turnDelay);
            ChangeTurn();
            yield break;
        }
        else if (currentCellId + roll == lastCellId) {
            Debug.Log("LEVEL END!");
            players[currentTurn].ChangeLocation(grid[grid.Count - 1]);
            yield return new WaitForSeconds(turnDelay);
            NextLevel();
            yield break;
        }
        Debug.Log((currentCellId + roll) - 1);

        Player currentPlayer = players[currentTurn];

        switch (currentPlayer.currentLocation.cellType)
        {
            case CellType.SnakeHead:
                currentPlayer.ChangeLocation(grid[(currentCellId + roll) - 1]);
                break;
            case CellType.LadderBottom:
                currentPlayer.ChangeLocation(grid[(currentCellId + roll) - 1]);
                break;
            default:
                currentPlayer.ChangeLocation(grid[(currentCellId + roll) - 1]);
                yield return new WaitForSeconds(turnDelay);
                ChangeTurn();
                yield break;
        }

        yield return new WaitForSeconds(turnDelay);

        Tile newLoc = grid[currentPlayer.currentLocation.link];
        currentPlayer.ChangeLocation(newLoc);
        ChangeTurn();
    }

    private void ChangeTurn()
    {
        if (currentTurn >= players.Count - 1)
            currentTurn = 0;
        else
            currentTurn++;
        uIManager.rollButton.interactable = true;
    }

    public Player GetCurrrentTurn()
    {
        if (players.Count == 0 || currentTurn < 0)
            return null;
        return players[currentTurn];
    }
}
