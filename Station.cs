using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System;

public class Station_Track
{
    // tilemap for each station track, because some tracks may overlap
    public Tilemap tilemap;
    public Vector3Int start_location;
    public GameObject train;
    public Station station;
    public int inner;
    public RouteManager.Orientation board_orientation;
}

public class Station
{
    public RouteManager.Orientation orientation; // orientation to rotate the boxcars when initialized
    public Station_Track outer_track;
    public Station_Track inner_track;

    public void remove_train_from_station_track(GameObject train)
    {
        if (inner_track.train == train)
        {
            inner_track.train = null;
        }
        else if (outer_track.train == train)
        {
            outer_track.train = null;
        }
    }

    public Station_Track set_station_track(GameObject train)
    {
        if (outer_track.train == null)
        {
            outer_track.train = train;
            return outer_track;
        }
        else if (inner_track.train == null)
        {
            inner_track.train = train;
            return inner_track;
        }
        else { throw new Exception("track is not available"); }
    }

    public Station_Track get_station_track(GameObject train)
    {
        if (outer_track.train == train) return outer_track;
        else if (inner_track.train == train) return inner_track;
        else
        {
            throw new Exception("train is not in the station");
        }
    }

    public Station(Vector3Int outer_start, Vector3Int inner_start, RouteManager.Orientation orientation, RouteManager.Orientation outer_track_board_orientation, RouteManager.Orientation inner_track_board_orientation, Tilemap outer_track_tilemap, Tilemap inner_track_tilemap){
        this.orientation = orientation;

        outer_track = new Station_Track();
        outer_track.inner = 0;
        outer_track.tilemap = outer_track_tilemap;
        outer_track.start_location = outer_start;
        outer_track.train = null;
        outer_track.station = this;
        outer_track.board_orientation = outer_track_board_orientation;

        inner_track = new Station_Track();
        inner_track.inner = 1;
        inner_track.tilemap = inner_track_tilemap;
        inner_track.start_location = inner_start;
        inner_track.train = null;
        inner_track.station = this;
        inner_track.board_orientation = inner_track_board_orientation;
    }
}
