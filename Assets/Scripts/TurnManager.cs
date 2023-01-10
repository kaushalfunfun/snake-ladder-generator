using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] Player[] players;
    

    public void ResetGame()
    {
        for (int i = 0; i < players.Length; i++)
        {
            Destroy(players[i].gameObject);
        }
    }
}
