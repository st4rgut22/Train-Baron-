using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections.Generic;

public class Building_Lot
{
    public Building building;
    public Vector2Int origin_tile;
    public int length;
    public RouteManager.Orientation orientation;
    public List<Station_Track> station_track_list;
    public RouteManager.Orientation person_orientation;
    public float door_1_rotation;
    public float door_2_rotation;

    public Building_Lot(Vector2Int origin_tile, int length, RouteManager.Orientation orientation, List<Station_Track> station_track, float door_1_rot = -1.0f, float door_2_rot=-1.0f)
    {
        this.length = length;
        this.origin_tile = origin_tile;
        this.orientation = orientation;
        this.station_track_list = station_track;
        this.door_1_rotation = door_1_rot;
        this.door_2_rotation = door_2_rot;
    }

    public void set_building(Building building)
    {
        this.building = building;
    }
}