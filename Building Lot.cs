using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections.Generic;

public class Building_Lot
{
    public string id; // used for movement
    public Building building;
    public Vector2Int origin_tile;
    public int length;
    public RouteManager.Orientation orientation;
    public List<Station_Track> station_track_list;
    public RouteManager.Orientation person_orientation;
    public float outer_door_rotation;
    public float primary_door_rotation;

    public Building_Lot(string id, Vector2Int origin_tile, int length, RouteManager.Orientation orientation, List<Station_Track> station_track, float outer_door_rotation = -1.0f, float primary_door_rotation = -1.0f)
    {
        this.id = id;
        this.length = length;
        this.origin_tile = origin_tile;
        this.orientation = orientation;
        this.station_track_list = station_track;
        this.outer_door_rotation = outer_door_rotation;
        this.primary_door_rotation = primary_door_rotation;
    }

    public void set_building(Building building)
    {
        this.building = building;
    }
}