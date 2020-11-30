﻿using UnityEngine.Tilemaps;
using UnityEngine;

public class Building_Lot
{
    public Building building;
    public Vector2Int origin_tile;
    public int length;
    public RouteManager.Orientation orientation;

    public Building_Lot(Vector2Int origin_tile, int length, RouteManager.Orientation orientation)
    {
        this.length = length;
        this.origin_tile = origin_tile;
        this.orientation = orientation;
    }

    public void set_building(Building building)
    {
        this.building = building;
    }
}