using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Player : MonoBehaviour
{
    public int playerId;
    public Tile currentLocation;

    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ChangeLocation(Tile newLocation)
    {
        currentLocation = newLocation;
        transform.position = newLocation.transform.position;
    }

    public void SetColor(int playerId, Material myColor) {
        this.playerId = playerId;
        meshRenderer.material = myColor;
    }
}
