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
    public static Tilemap shipyard_track_tilemap;
    static float cell_width = .88f;

    public enum Orientation
    {
        None,
        North,
        East,
        West,
        South
    }

    private void Awake()
    {
        shipyard_track_tilemap = GameObject.Find("Shipyard Track").GetComponent<Tilemap>();
    }

    // Start is called before the first frame update
    void Start()
    {
        track_tilemap = GameObject.Find("Track Layer").GetComponent<Tilemap>();
        city_tilemap = GameObject.Find("Structure").GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {

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
                print("train orientation is not set. cannot set boxcar position");
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

    public static Orientation get_start_orientation(Vector3Int track_location, City departure_city)
    {
        // when a train is instantiated, its orientation must match direction of track
        Vector3Int depart_city_location = departure_city.get_location();

        Tile track_tile = (Tile)track_tilemap.GetTile(track_location);
        if (track_tile != null)
        {
            // get orientation relative to city
            string tile_name = track_tile.name.Replace("(Clone)", "");
            bool city_next_to_track = is_city_adjacent_to_track(track_location, depart_city_location, tile_name);
            if (!city_next_to_track) return Orientation.None; // track must be adjacent to city
            switch (tile_name)
            {
                case "ES":
                    if (depart_city_location.x > track_location.x)
                    {
                        return Orientation.West;
                    }
                    else if (depart_city_location.y < track_location.y)
                    {
                        return Orientation.North;
                    }
                    break;
                case "NE":
                    if (depart_city_location.y > track_location.y)
                    {
                        return Orientation.South;
                    }
                    else if (depart_city_location.x > track_location.x)
                    {
                        return Orientation.West;
                    }
                    break;
                case "WN":
                    if (depart_city_location.y > track_location.y)
                    {
                        return Orientation.South;
                    }
                    else if (depart_city_location.x < track_location.x)
                    {
                        return Orientation.East;
                    }
                    break;
                case "WS":
                    if (depart_city_location.x < track_location.x)
                    {
                        return Orientation.East;
                    }
                    else if (depart_city_location.y < track_location.y)
                    {
                        return Orientation.North;
                    }
                    break;
                case "vert":
                    if (depart_city_location.y > track_location.y)
                    {
                        return Orientation.South;
                    }
                    else {
                        return Orientation.North;
                    }
                case "hor":
                    if (depart_city_location.x > track_location.x)
                    {
                        return Orientation.West;
                    } else
                    {
                        return Orientation.East;
                    }
                default:
                    return Orientation.None;
            }
        }
        return Orientation.None;
    }

    public static PositionPair get_destination(MovingObject moving_thing)
    {
        
        Vector3Int tile_coord = new Vector3Int(moving_thing.tile_position[0], moving_thing.tile_position[1], 0);
        Tile track_tile = (Tile)track_tilemap.GetTile(tile_coord);
        Vector2 tile_world_coord = track_tilemap.GetCellCenterWorld(tile_coord);
        Vector2 final_cell_dest = moving_thing.transform.position;
        Vector2Int next_tilemap_pos = new Vector2Int(tile_coord.x, tile_coord.y);
        try
        {
            if (!moving_thing.in_city)
            {
                Tile city_tile = (Tile)city_tilemap.GetTile(tile_coord);
                if (city_tile != null)
                {
                    print("train sees city " + city_tile.name + " final destination is " + tile_world_coord);
                    final_cell_dest = tile_world_coord; // destination is the center of the tile
                    moving_thing.set_motion(false);
                    moving_thing.arrive_at_city(); // when arrive at city, set flag to true 
                }
            } else
            {
                track_tile = (Tile)shipyard_track_tilemap.GetTile(tile_coord);
            }
            string tile_name = track_tile.name;
            print("Tile name is " + tile_name);
            switch (tile_name)
            {
                case "ES":
                    if (moving_thing.orientation == Orientation.West)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - cell_width / 2);
                        moving_thing.final_orientation = Orientation.South;
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y - 1);
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + cell_width / 2, tile_world_coord[1]);
                        moving_thing.final_orientation = Orientation.East;
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y + 1);
                    }
                    break;
                case "NE":
                    if (moving_thing.orientation == Orientation.South)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + cell_width / 2, tile_world_coord[1]);
                        moving_thing.final_orientation = Orientation.East;
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y - 1);
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + cell_width / 2);
                        moving_thing.final_orientation = Orientation.North;
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y + 1);
                    }
                    break;
                case "WN":
                    if (moving_thing.orientation == Orientation.East)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + cell_width / 2);
                        moving_thing.final_orientation = Orientation.North;
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y + 1);
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] - cell_width / 2, tile_world_coord[1]);
                        moving_thing.final_orientation = Orientation.West;
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y - 1);
                    }

                    break;
                case "WS":
                    if (moving_thing.orientation == Orientation.East)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - cell_width / 2);
                        moving_thing.final_orientation = Orientation.South;
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y - 1);
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] - cell_width / 2, tile_world_coord[1]);
                        moving_thing.final_orientation = Orientation.West;
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y + 1);
                    }
                    break;
                case "vert":
                    if (moving_thing.orientation == Orientation.North)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + cell_width / 2);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - cell_width / 2);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y - 1);
                    }
                    break;
                case "hor":
                    if (moving_thing.orientation == Orientation.West)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] - cell_width / 2, tile_world_coord[1]);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
                    }                   
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + cell_width / 2, tile_world_coord[1]);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
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
        return new PositionPair(final_cell_dest, next_tilemap_pos);
    }
}
