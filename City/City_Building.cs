using UnityEngine.Tilemaps;
using UnityEngine;

public class City_Building
{
    public Tile tile;
    public GameObject building_object;
    public City_Building(Tile tile, GameObject building_object)
    {
        this.tile = tile;
        this.building_object = building_object;
    }
}