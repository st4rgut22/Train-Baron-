﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System;

public class Station_Track
{
    public Tilemap tilemap;
    public Vector3Int start_location;
    public GameObject train;
}

public class Station
{
    Station_Track outer_track;
    Station_Track inner_track;

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

    public Station(Vector3Int outer_start, Vector3Int inner_start){
        outer_track = new Station_Track();
        outer_track.tilemap = RouteManager.shipyard_track_tilemap2;
        outer_track.start_location = outer_start;
        outer_track.train = null;

        inner_track = new Station_Track();
        inner_track.tilemap = RouteManager.shipyard_track_tilemap;
        inner_track.start_location = inner_start;
        inner_track.train = null;
    }
}
