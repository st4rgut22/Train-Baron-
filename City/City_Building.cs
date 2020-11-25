using UnityEngine.Tilemaps;

public class City_Building
{
    public Tile tile;
    public Building building;
    public City_Building(Tile tile, Building building)
    {
        this.tile = tile;
        this.building = building;
    }
}