using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class Tile : MonoBehaviour, IComparable<Tile>
{
    public int cellId;
    public CellType cellType;
    public int link;

    [SerializeField]private TextMeshProUGUI cellIdDisplay;
    
    public void SetTileNumber()
    {
        if(cellIdDisplay == null)
        {
            Debug.LogError("Set tile number display!");
            return;
        }
        cellIdDisplay.text = cellId.ToString();
    }

    public int CompareTo(Tile comparePart)
    {
        // A null value means that this object is greater.
        if (comparePart == null)
            return 1;

        else
            return this.cellId.CompareTo(comparePart.cellId);
    }
}
