﻿using System.Collections;
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
    public RouteManager.Orientation orientation;

    public void add_boxcar_to_train(GameObject boxcar)
    {
        VehicleManager.create_boxcar(train.GetComponent<Train>(), boxcar);
    }
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

    public int orientation_to_idx()
    {
        // used to access wait tile list
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                return 0;
            case RouteManager.Orientation.East:
                return 1;
            case RouteManager.Orientation.West:
                return 2;
            case RouteManager.Orientation.South:
                return 3;
            default:
                throw new Exception("not a valid orientation");
        }
    }

    public bool is_station_track_available()
    {
        if (outer_track.train == null || inner_track.train == null) return true;
        else
        {
            return false;
        }
    }

    public Station_Track set_station_track(GameObject train)
    {
        // place train in an available track. if none is available just return the outer track
        if (outer_track.train == null)
        {
            //outer_track.train = train;
            return outer_track;
        }
        else if (inner_track.train == null) // TODOED just for testing uncomment later
        {
            //inner_track.train = train;
            return inner_track;
        }
        else {  
            // dont station track's train
            return outer_track;
        }
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

    public Station(Vector3Int outer_start, Vector3Int inner_start, RouteManager.Orientation orientation, Tilemap outer_track_tilemap, Tilemap inner_track_tilemap){
        this.orientation = orientation;

        outer_track = new Station_Track();
        outer_track.inner = 0;
        outer_track.tilemap = outer_track_tilemap;
        outer_track.start_location = outer_start;
        outer_track.train = null;
        outer_track.station = this;

        inner_track = new Station_Track();
        inner_track.inner = 1;
        inner_track.tilemap = inner_track_tilemap;
        inner_track.start_location = inner_start;
        inner_track.train = null;
        inner_track.station = this;
    }
}
