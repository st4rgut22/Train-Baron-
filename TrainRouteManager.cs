﻿using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrainRouteManager : RouteManager
{

    public static Orientation get_destination_track_orientation(string exit_track_name)
    {
        if (exit_track_name == "north exit")
            return Orientation.North;
        else if (exit_track_name == "east exit")
            return Orientation.East;
        else if (exit_track_name == "west exit")
            return Orientation.West;
        else if (exit_track_name == "south exit")
            return Orientation.South;
        else
        {
            return Orientation.None;
        }
    }

    public static void set_destination_track(string exit_track_name)
    {
        if (exit_track_name == "Shipyard Track Exit North")
            GameManager.city_manager.set_destination_track(Orientation.North);
        else if (exit_track_name == "Shipyard Track Exit South")
            GameManager.city_manager.set_destination_track(Orientation.South);
        else if (exit_track_name == "Shipyard Track Exit West")
            GameManager.city_manager.set_destination_track(Orientation.West);
        else if (exit_track_name == "Shipyard Track Exit East")
            GameManager.city_manager.set_destination_track(Orientation.East);
    }

    public static string get_destination_type(Vector3Int tile_coord, bool in_city)
    {
        if (in_city)
        {
            Tile track_tile = (Tile)track_tilemap.GetTile(tile_coord);
            Tile city_tile = (Tile)city_tilemap.GetTile(tile_coord);
            if (track_tile != null) return "track";
            if (city_tile != null) return "city";
        }
        else
        {
            Tile shipyard_tile = (Tile)shipyard_track_tilemap.GetTile(tile_coord);
            if (shipyard_tile != null) return "track";
        }
        return ""; // cannot go to this tile
    }

    public static Vector3 get_spawn_location(Vector3Int tilemap_location, Orientation orientation)
    {
        Vector3 tile_world_coord = track_tilemap.GetCellCenterWorld(tilemap_location);
        switch (orientation)
        {
            case Orientation.East:
                tile_world_coord.x -= cell_width / 2;
                break;
            case Orientation.West:
                tile_world_coord.x += cell_width / 2;
                break;
            case Orientation.North:
                tile_world_coord.y -= cell_width / 2;
                break;
            case Orientation.South:
                tile_world_coord.y += cell_width / 2;
                break;
            default:
                //print("train orientation is not set. cannot set boxcar position");
                break;
        }
        // Get the center of the city where the vehicle is instantiated
        return tile_world_coord;
    }

    public static Vector3 get_city_boundary_location(Vector3Int tile_position, Orientation orientation)
    {
        // get edge of city matching orientation fo the vehicle, the first destination for the vehicle
        Vector3 tile_world_coord = track_tilemap.GetCellCenterWorld(tile_position);
        switch (orientation)
        {
            case Orientation.East:
                tile_world_coord.x += cell_width / 2;
                break;
            case Orientation.West:
                tile_world_coord.x -= cell_width / 2;
                break;
            case Orientation.North:
                tile_world_coord.y += cell_width / 2;
                break;
            case Orientation.South:
                tile_world_coord.y -= cell_width / 2;
                break;
            default:
                throw new Exception("train orientation should be set upon leaving city");
                break;
        }
        return tile_world_coord;
    }

    public static bool is_city_adjacent_to_track(Vector3Int track_location, Vector3Int city_location, string trackname)
    {
        Vector2Int city_location_2d = new Vector2Int(city_location.x, city_location.y);
        if (city_location_2d.Equals(new Vector2Int(track_location.x + 1, track_location.y)) || city_location_2d.Equals(new Vector2Int(track_location.x - 1, track_location.y))
            || city_location_2d.Equals(new Vector2Int(track_location.x, track_location.y + 1)) || city_location_2d.Equals(new Vector2Int(track_location.x, track_location.y - 1)))
            return true;
        else
        {
            return false;
        }
    }

    public static Vector2Int get_depart_tile_position(Orientation orientation, Vector3Int tile_coord)
    {
        switch (orientation)
        {
            case Orientation.North:
                return new Vector2Int(tile_coord.x, tile_coord.y + 1);
            case Orientation.East:
                return new Vector2Int(tile_coord.x + 1, tile_coord.y);
            case Orientation.West:
                return new Vector2Int(tile_coord.x - 1, tile_coord.y);
            case Orientation.South:
                return new Vector2Int(tile_coord.x, tile_coord.y - 1);
            default:
                return new Vector2Int(tile_coord.x, tile_coord.y);
        }
    }

    public static PositionPair get_initial_destination(MovingObject vehicle, Tilemap tilemap)
    {
        Orientation original_orientation = vehicle.orientation;
        Orientation original_final_orientation = vehicle.final_orientation;
        Tile track_tile = (Tile)tilemap.GetTile(vehicle.tile_position);
        string track_name = track_tile.name;
        vehicle.orientation = TrackManager.flip_straight_orientation(vehicle.orientation);
        PositionPair prev_pos_pair = get_next_tile_pos(tilemap, track_tile, vehicle, vehicle.tile_position, new Vector2(0, 0)); // opposite direction of train to get prev tile
        Vector3Int prev_tile_coord = (Vector3Int)prev_pos_pair.tile_dest_pos;
        track_tile = (Tile)tilemap.GetTile(prev_tile_coord);
        track_name = track_tile.name;
        PositionPair pos_pair = get_next_tile_pos(tilemap, track_tile, vehicle, prev_tile_coord, new Vector2(0, 0)); // go in direction opposite of train
        TrackManager.set_opposite_direction(track_name, vehicle); // set direction same as train
        //print("final direction is " + vehicle.orientation);
        pos_pair.tile_dest_pos = prev_pos_pair.tile_dest_pos; // use previous tile, not the previous previous tile
        pos_pair.orientation = vehicle.orientation;
        vehicle.orientation = original_orientation; // restore original orientation
        vehicle.final_orientation = original_final_orientation;
        return pos_pair;
    }
}