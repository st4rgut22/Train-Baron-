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
    public GameObject outer_door;
    public GameObject primary_door;

    public Building_Lot(string id, Vector2Int origin_tile, int length, RouteManager.Orientation orientation, List<Station_Track> station_track, GameObject outer_door, GameObject primary_door)
    {
        this.id = id;
        this.length = length;
        this.origin_tile = origin_tile;
        this.orientation = orientation;
        this.station_track_list = station_track;
        this.outer_door = outer_door;
        this.primary_door = primary_door;
        if (outer_door != null)
            this.outer_door.SetActive(false); // hide the doors until the buildings are spawned on the building lot
        if (primary_door != null)
            this.primary_door.SetActive(false); 
    }

    public void set_building(Building building)
    {
        this.building = building;
    }
}