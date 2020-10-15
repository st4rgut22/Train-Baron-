using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class RouteManager : MonoBehaviour
{
    // all functions for routing moving objects

    public static Tilemap track_tilemap;
    public static Tilemap city_tilemap;

    public enum Orientation
    {
        North,
        East,
        West,
        South
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject track_layer = GameObject.FindGameObjectsWithTag("track_layer")[0];
        GameObject city_layer = GameObject.FindGameObjectsWithTag("city_layer")[0];
        if (track_layer != null && city_layer != null)
        {
            city_tilemap = city_layer.GetComponent<Tilemap>();
            track_tilemap = track_layer.GetComponent<Tilemap>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Dictionary<string, bool> get_exit_track(Vector3Int tilemap_position)
    {
        // return a dictionary of routes bordering a tilemap location
        Dictionary<string, bool> exit_map = new Dictionary<string, bool>(); // key is orientation, value is true if route exists
        Tile north_tile = (Tile)track_tilemap.GetTile(new Vector3Int(tilemap_position.x, tilemap_position.y + 1, tilemap_position.z));
        Tile south_tile = (Tile)track_tilemap.GetTile(new Vector3Int(tilemap_position.x, tilemap_position.y - 1, tilemap_position.z));
        Tile west_tile = (Tile)track_tilemap.GetTile(new Vector3Int(tilemap_position.x - 1, tilemap_position.y, tilemap_position.z));
        Tile east_tile = (Tile)track_tilemap.GetTile(new Vector3Int(tilemap_position.x + 1, tilemap_position.y, tilemap_position.z));
        if (north_tile != null) exit_map["N"] = true;
        else { exit_map["N"] = false; }
        if (south_tile != null) exit_map["S"] = true;
        else { exit_map["S"] = false; }
        if (west_tile != null) exit_map["W"] = true;
        else { exit_map["W"] = false; }
        if (east_tile != null) exit_map["E"] = true;
        else { exit_map["E"] = false; }
        return exit_map;
    }

    public static string get_destination_type(Vector3Int tile_coord)
    {
        Tile track_tile = (Tile)track_tilemap.GetTile(tile_coord);
        Tile city_tile = (Tile)city_tilemap.GetTile(tile_coord);
        if (track_tile != null)
            return "track";
        if (city_tile != null)
            return "city";
        return ""; // not a destination
    }

    public static Vector3 get_spawn_location(Vector3Int tilemap_location, Orientation orientation)
    {
        // get the edge of the tile opposite its orientation. Starting here rather than the center of the tile allows
        // the gap between train and boxcars
        Vector3 tile_world_coord = track_tilemap.GetCellCenterWorld(tilemap_location);
        switch (orientation)
        {
            case Orientation.East:
                tile_world_coord.x -= .5f;
                break;
            case Orientation.West:
                tile_world_coord.x += .5f;
                break;
            case Orientation.North:
                tile_world_coord.y -= .5f;
                break;
            case Orientation.South:
                tile_world_coord.y += .5f;
                break;
            default:
                print("train orientation is not set. cannot set boxcar position");
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
                tile_world_coord.x += .5f;
                break;
            case Orientation.West:
                tile_world_coord.x -= .5f;
                break;
            case Orientation.North:
                tile_world_coord.y += .5f;
                break;
            case Orientation.South:
                tile_world_coord.y -= .5f;
                break;
            default:
                print("train orientation is not set. cannot set boxcar position");
                break;
        }
        return tile_world_coord;
    }


    public static Vector2 get_destination(MovingObject moving_thing)
    {
        Vector3Int tile_coord = new Vector3Int(moving_thing.tile_position[0], moving_thing.tile_position[1], 0);
        Tile track_tile = (Tile)track_tilemap.GetTile(tile_coord);
        Tile city_tile = (Tile)city_tilemap.GetTile(tile_coord);
        Vector2 tile_world_coord = track_tilemap.GetCellCenterWorld(tile_coord);
        Vector2 final_cell_dest = moving_thing.transform.position;
        try
        {
            if (city_tile != null)
            {   
                print("train sees city " + city_tile.name + " final destination is " + tile_world_coord);
                final_cell_dest = tile_world_coord; // destination is the center of the tile
                moving_thing.set_motion(false);
                moving_thing.arrive_at_city(); // when arrive at city, set flag to true 
            }
            string tile_name = track_tile.name;
            switch (tile_name)
            {
                case "ES":
                    if (moving_thing.orientation == Orientation.West)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - .5f);
                        moving_thing.final_orientation = Orientation.South;
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + 0.5f, tile_world_coord[1]);
                        moving_thing.final_orientation = Orientation.East;
                    }
                    break;
                case "NE":
                    if (moving_thing.orientation == Orientation.South)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + 0.5f, tile_world_coord[1]);
                        moving_thing.final_orientation = Orientation.East;
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + 0.5f);
                        moving_thing.final_orientation = Orientation.North;
                    }
                    break;
                case "WN":
                    if (moving_thing.orientation == Orientation.East)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + 0.5f);
                        moving_thing.final_orientation = Orientation.North;
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] - 0.5f, tile_world_coord[1]);
                        moving_thing.final_orientation = Orientation.West;
                    }

                    break;
                case "WS":
                    if (moving_thing.orientation == Orientation.East)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - 0.5f);
                        moving_thing.final_orientation = Orientation.South;
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] - 0.5f, tile_world_coord[1]);
                        moving_thing.final_orientation = Orientation.West;
                    }
                    break;
                case "vert":
                    if (moving_thing.orientation == Orientation.North)
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + .5f);
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - .5f);
                    }
                    break;
                case "hor":
                    if (moving_thing.orientation == Orientation.West)
                        final_cell_dest = new Vector2(tile_world_coord[0] - .5f, tile_world_coord[1]);
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + .5f, tile_world_coord[1]);
                    }
                    break;
                default:
                    print("none of the track tiles matched"); // return current position
                    break;
            }
        }
        catch (NullReferenceException e)
        {
            print("Train has reached the end of the track.");
            print(e.Message);
        }
        //print("FINAL cell destination " + final_cell_dest);
        return final_cell_dest;
    }
}
