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
    public static Tilemap shipyard_track_tilemap2;
    public static float cell_width = .88f;

    public enum Orientation
    {
        None,
        North,
        East,
        West,
        South,
        ne_SteepCurve,  // one of the four steep curve tracks
        ne_LessSteepCurve,  // one of the four steep curve tracks
        nw_SteepCurve,
        nw_LessSteepCurve,
        se_SteepCurve,
        se_LessSteepCurve,
        sw_SteepCurve,
        sw_LessSteepCurve,
        Right_Angle,
        Steep_Angle,
        Less_Steep_Angle
    }

    private void Awake()
    {
        shipyard_track_tilemap = GameObject.Find("Shipyard Track").GetComponent<Tilemap>();
        shipyard_track_tilemap2 = GameObject.Find("Shipyard Track 2").GetComponent<Tilemap>();
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

    public static string get_destination_type(Vector3Int tile_coord, bool in_city)
    {
        if (in_city)
        {
            Tile track_tile = (Tile)track_tilemap.GetTile(tile_coord);
            Tile city_tile = (Tile)city_tilemap.GetTile(tile_coord);
            if (track_tile != null) return "track";
            if (city_tile != null) return "city";
        } else {
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

    public static Orientation is_curve_steep(Orientation orientation)
    {
        if (orientation == Orientation.ne_SteepCurve || orientation == Orientation.nw_SteepCurve
            || orientation == Orientation.sw_SteepCurve || orientation == Orientation.se_SteepCurve)
            return Orientation.Steep_Angle;
        else if (orientation == Orientation.ne_LessSteepCurve || orientation == Orientation.nw_LessSteepCurve
            || orientation == Orientation.sw_LessSteepCurve || orientation == Orientation.se_LessSteepCurve)
            return Orientation.Less_Steep_Angle;
        else
        {
            return Orientation.Right_Angle;
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

    public static Vector2 get_straight_final_dest(Orientation orientation, Vector2 tile_world_coord)
    {
        switch (orientation)
        {
            case Orientation.North:
                return new Vector2(tile_world_coord[0], tile_world_coord[1] + cell_width / 2);
            case Orientation.East:
                return new Vector2(tile_world_coord[0] + cell_width / 2, tile_world_coord[1]);
            case Orientation.West:
                return new Vector2(tile_world_coord[0] - cell_width / 2, tile_world_coord[1]);                
            case Orientation.South:
                return new Vector2(tile_world_coord[0], tile_world_coord[1] - cell_width / 2);
            default:
                throw new Exception("orientation not valid");
        }
    }

    public static Vector2Int get_straight_next_tile_pos(Orientation orientation, Vector2Int next_tilemap_pos)
    {
        switch (orientation)
        {
            case Orientation.North:
                return new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
            case Orientation.East:
                return new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
            case Orientation.West:
                return new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
            case Orientation.South:
                return new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y - 1);
            default:
                throw new Exception("orientation not valid");
        }
    }

    public static PositionPair get_next_tile_pos(Tilemap tilemap, Tile track_tile, MovingObject moving_thing, Vector3Int tile_coord)
    {
        Vector2Int next_tilemap_pos = new Vector2Int(tile_coord.x, tile_coord.y);
        Vector2 tile_world_coord = tilemap.GetCellCenterWorld(tile_coord);
        Vector2 final_cell_dest = moving_thing.transform.position; // end of track default destination ( dont move )
        try
        {
            string tile_name = track_tile.name;
            switch (tile_name)
            {
                //tricky curve tile updates. the train has already arrived in the tile so only adjust one coordinate
                case "ES":
                    if (moving_thing.orientation == Orientation.West)
                    {
                        moving_thing.final_orientation = Orientation.South;
                        final_cell_dest = get_straight_final_dest(Orientation.South, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y - 1);
                    }
                    else
                    {
                        moving_thing.final_orientation = Orientation.East;
                        final_cell_dest = get_straight_final_dest(Orientation.East, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
                    }
                    break;
                case "NE":
                    if (moving_thing.orientation == Orientation.South)
                    {
                        moving_thing.final_orientation = Orientation.East;
                        final_cell_dest = get_straight_final_dest(Orientation.East, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
                    }
                    else
                    {
                        moving_thing.final_orientation = Orientation.North;
                        final_cell_dest = get_straight_final_dest(Orientation.North, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
                    }
                    break;
                case "WN":
                    if (moving_thing.orientation == Orientation.East)
                    {
                        moving_thing.final_orientation = Orientation.North;
                        final_cell_dest = get_straight_final_dest(Orientation.North, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
                    }
                    else
                    {
                        moving_thing.final_orientation = Orientation.West;
                        final_cell_dest = get_straight_final_dest(Orientation.West, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
                    }

                    break;
                case "WS":
                    if (moving_thing.orientation == Orientation.East)
                    {
                        moving_thing.final_orientation = Orientation.South;
                        final_cell_dest = get_straight_final_dest(Orientation.South, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y - 1);
                    }
                    else
                    {
                        moving_thing.final_orientation = Orientation.West;
                        final_cell_dest = get_straight_final_dest(Orientation.West, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
                    }
                    break;
                case "vert":
                    final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                    next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
                    break;
                case "hor":
                    final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                    next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
                    break;
                case "ne_diag":
                    moving_thing.final_orientation = Orientation.ne_SteepCurve;
                    break;
                case "nw_diag":
                    moving_thing.final_orientation = Orientation.nw_SteepCurve;
                    break;
                case "se_diag":
                    moving_thing.final_orientation = Orientation.se_SteepCurve;
                    break;
                case "sw_diag":
                    moving_thing.final_orientation = Orientation.sw_SteepCurve;
                    break;
                case "less_diag_ne_turn":
                    moving_thing.final_orientation = Orientation.ne_LessSteepCurve;
                    break;
                case "less_diag_nw_turn":
                    moving_thing.final_orientation = Orientation.nw_LessSteepCurve;
                    break;
                case "less_diag_se_turn":
                    moving_thing.final_orientation = Orientation.se_LessSteepCurve;
                    break;
                case "less_diag_sw_turn":
                    moving_thing.final_orientation = Orientation.sw_LessSteepCurve;
                    break;
                default:
                    moving_thing.final_orientation = Orientation.None;
                    print("none of the track tiles matched"); // return current position
                    break;
            }
            if (tile_name == "ne_diag" || tile_name == "nw_diag" || tile_name == "se_diag" || tile_name == "less_diag_ne_turn" ||
                tile_name == "less_diag_nw_turn" || tile_name == "less_diag_se_turn" || tile_name == "less_diag_sw_turn")
            {
                final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
            }
        }
        catch (NullReferenceException e)
        {
            final_cell_dest = tile_world_coord;
            print("Vehicle has reached the end of the track. tilemap position of " + moving_thing.name + " is " + moving_thing.tile_position);
            print(e.Message);
        }
        return new PositionPair(final_cell_dest, next_tilemap_pos);
    }

    public static PositionPair get_initial_destination(MovingObject vehicle, Tilemap tilemap)
    {
        Orientation original_orientation = vehicle.orientation;
        Orientation original_final_orientation = vehicle.final_orientation;
        Tile track_tile = (Tile)tilemap.GetTile(vehicle.tile_position);
        string track_name = track_tile.name;
        vehicle.orientation = TrackManager.flip_straight_orientation(vehicle.orientation);
        PositionPair prev_pos_pair = get_next_tile_pos(tilemap, track_tile, vehicle, vehicle.tile_position); // opposite direction of train to get prev tile
        Vector3Int prev_tile_coord = (Vector3Int)prev_pos_pair.tile_dest_pos; 
        track_tile = (Tile)tilemap.GetTile(prev_tile_coord);
        track_name = track_tile.name;
        PositionPair pos_pair = get_next_tile_pos(tilemap, track_tile, vehicle, prev_tile_coord); // go in direction opposite of train
        TrackManager.set_opposite_direction(track_name, vehicle); // set direction same as train
        pos_pair.tile_dest_pos = prev_pos_pair.tile_dest_pos; // use previous tile, not the previous previous tile
        pos_pair.orientation = vehicle.orientation;
        vehicle.orientation = original_orientation; // restore original orientation
        vehicle.final_orientation = original_final_orientation;
        return pos_pair;
    }

    public static PositionPair get_destination(MovingObject moving_thing, Tilemap tilemap)
    {      
        Vector3Int tile_coord = new Vector3Int(moving_thing.tile_position[0], moving_thing.tile_position[1], 0);
        Tile track_tile = (Tile)tilemap.GetTile(tile_coord);
        Vector2 tile_world_coord = tilemap.GetCellCenterWorld(tile_coord);
        PositionPair pos_pair = get_next_tile_pos(tilemap, track_tile, moving_thing, tile_coord);
        if (!moving_thing.in_city) //not inside a city, so check if arrived at city
        {
            Tile city_tile = (Tile)city_tilemap.GetTile(tile_coord);
            if (city_tile != null) //check if arriving at city
            {
                City city = CityManager.gameobject_board[tile_coord.x, tile_coord.y].GetComponent<City>(); // check if city arrived at is not the same city we're leaving
                if (city != moving_thing.prev_city)
                {
                    pos_pair.abs_dest_pos = tile_world_coord; // destination is the center of the tile
                    moving_thing.prepare_to_arrive_at_city(city);
                }
            }
        }
        return pos_pair;
    }
}
