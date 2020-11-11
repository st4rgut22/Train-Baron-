using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionPair
{
    public Vector2 abs_dest_pos;
    public Vector2Int tile_dest_pos;
    public RouteManager.Orientation orientation; // used to initialize orientation of new boxcars
    public PositionPair(Vector2 abs_dest_pos, Vector2Int tile_dest_pos)
    {
        this.tile_dest_pos = tile_dest_pos;
        this.abs_dest_pos = abs_dest_pos;
    }
}