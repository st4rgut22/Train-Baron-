using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class RouteManager : MonoBehaviour
{
    // all functions for routing moving objects


    public static GameObject Track_Layer;
    public static Tilemap track_tilemap;

    public static Tilemap city_tilemap;
    public static Tilemap shipyard_track_tilemap;
    public static Tilemap shipyard_track_tilemap2;
    public static Tilemap exit_north_tilemap;
    public static Tilemap exit_south_tilemap;
    public static Tilemap exit_east_tilemap;
    public static Tilemap exit_west_tilemap;
    public static float cell_width = .88f;

    public static Vector2 offset_right = new Vector2(cell_width / 4, 0);
    public static Vector2 offset_left = new Vector2(-cell_width / 4, 0);
    public static Vector2 offset_up = new Vector2(0, cell_width / 4);
    public static Vector2 offset_down = new Vector2(0, -cell_width / 4);
    public static Vector2 offset_diag_ne = new Vector2(cell_width / 4, cell_width / 4);
    public static Vector2 offset_diag_sw = new Vector2(-cell_width / 4, -cell_width / 4);
    public static Vector2 offset_diag_se = new Vector2(-cell_width / 4, cell_width / 4);
    public static Vector2 no_offset = new Vector2(0, 0);
    public static Dictionary<Vector3Int, Dictionary<string, Vector2>> offset_route_map;

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
        Track_Layer = GameObject.Find("Top Track Layer");
        track_tilemap = Track_Layer.GetComponent<Tilemap>();
        exit_north_tilemap = GameObject.Find("Shipyard Track Exit North").GetComponent<Tilemap>();
        exit_south_tilemap = GameObject.Find("Shipyard Track Exit South").GetComponent<Tilemap>();
        exit_west_tilemap = GameObject.Find("Shipyard Track Exit West").GetComponent<Tilemap>();
        exit_east_tilemap = GameObject.Find("Shipyard Track Exit East").GetComponent<Tilemap>();
        shipyard_track_tilemap = GameObject.Find("Shipyard Track").GetComponent<Tilemap>();
        shipyard_track_tilemap2 = GameObject.Find("Shipyard Track 2").GetComponent<Tilemap>();
        city_tilemap = GameObject.Find("Structure").GetComponent<Tilemap>();

        Dictionary<string, Vector2> poopies = new Dictionary<string, Vector2>()
        {
            {
                "poop", new Vector2(-1, -1)
            }
        };



        offset_route_map = new Dictionary<Vector3Int, Dictionary<string, Vector2>>()
        {
            // a dictionary of offsets for person leave offset calculations
            {
                City.north_start_1,
                    new Dictionary<string, Vector2>(){
                        {
                            "hor", offset_down
                        },
                        {
                            "vert", offset_left
                        },
                        {
                            "NE", no_offset
                        }
                    }
            },
            {
                City.north_start_2,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_up
                        }
                    }
            },
            {
                City.east_start_1,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_down
                        },
                        {
                            "NE", no_offset
                        }
                    }
            },
            {
                City.east_start_2,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_up
                        }
                    }
            },
            {
                City.west_start_1,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_up
                        },
                        {
                            "WN", no_offset
                        }
                    }
            },
            {
                City.west_start_2,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_down
                        }
                    }
            },
            {
                City.south_start_1,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "vert", offset_right
                        },
                        {
                            "WS", no_offset
                        }
                    }
            },
            {
                City.south_start_2,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_down
                        },
                        {
                            "vert", offset_left
                        },
                        {
                            "NE", no_offset
                        },
                        {
                            "WS", no_offset
                        }
                    }
            }
        };
    }

// Start is called before the first frame update
void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static Vector2 get_straight_walking_position(Vector2Int cp_position, Orientation orientation)
    {
        Vector2 middle_pos = track_tilemap.GetCellCenterWorld((Vector3Int)cp_position);
        // todo: incorporate rotation here!
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                middle_pos.y -= cell_width / 4;
                break;
            case RouteManager.Orientation.East:
                middle_pos.x -= cell_width / 4;
                break;
            case RouteManager.Orientation.West:
                middle_pos.x += cell_width / 4;
                break;
            case RouteManager.Orientation.South:
                middle_pos.y += cell_width / 4;
                break;
        }
        return middle_pos;
    }

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
                //print("train orientation is not set. cannot set boxcar position");
                break;
        }
        // Get the center of the city where the vehicle is instantiated
        return tile_world_coord;
    }

    public static Checkpoint get_exit_home_checkpoint(RouteManager.Orientation begin_orientation, Boxcar boxcar, Room room)
    {
        Vector3Int train_start_location = boxcar.train.station_track.start_location; // id of track
        RouteManager.Orientation exit_orientation = City.station_track_boarding_map[train_start_location];
        Vector2Int doorstep_position = RouteManager.get_straight_next_tile_pos(exit_orientation, room.tile_position);
        return new Checkpoint(begin_orientation, exit_orientation, doorstep_position);
    }

    public IEnumerator board_train(Boxcar boxcar, Room room, Person occupant, Vector3Int destination)
    {
        List<Checkpoint> board_train_checkpoints = new List<Checkpoint>();
        List<float> rotation_list = new List<float>();
        Checkpoint exit_home_cp = get_exit_home_checkpoint(occupant.orientation, boxcar, room);
        board_train_checkpoints.Add(exit_home_cp);
        print("EXIT HOME rotation from " + occupant.orientation + " to " + exit_home_cp.end_orientation + " is " + exit_home_cp.rotation + " with final tile position " + exit_home_cp.tile_position);
        string track_name = shipyard_track_tilemap.GetTile((Vector3Int)exit_home_cp.tile_position).name;
        RouteManager.Orientation align_track_orientation = TrackManager.get_start_orientation(track_name, (Vector3Int) exit_home_cp.tile_position, (Vector3Int) boxcar.tile_position);
        Checkpoint face_track_cp = new Checkpoint(exit_home_cp.end_orientation, align_track_orientation, exit_home_cp.tile_position); // dont move just rotate
        occupant.final_dest_tile_pos = boxcar.tile_position;
        print("ALIGN TRACK rotation from " + exit_home_cp.end_orientation + " to " + align_track_orientation + " is " + face_track_cp.rotation);
        yield return StartCoroutine(occupant.move_checkpoints(board_train_checkpoints));
        print("start follow track sequence");
        occupant.in_tile = false; // allow person to follow the track to the destination boxcar
                                  // if all goes well then boarding is all that's left
        occupant.final_orientation = occupant.orientation;
    }

    public IEnumerator step_on_boxcar(GameObject person_go, GameObject boxcar_go)
    {
        Person person = person_go.GetComponent<Person>();
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        Vector2 boxcar_position = boxcar_go.transform.position;
        Orientation final_orientation = TrackManager.enter_boxcar_orientation(boxcar_position, person_go.transform.position);
        print("enter boxcar cp with person orientation " + person.orientation + " final orientation " + final_orientation + " boxcar tile pos " + boxcar.tile_position);
        Checkpoint step_on_boxcar_cp = new Checkpoint(person.orientation, final_orientation, (Vector2Int)boxcar.tile_position);
        List<Checkpoint> board_train_checkpoints = new List<Checkpoint>() { step_on_boxcar_cp };
        //yield return StartCoroutine(person.move_checkpoints(board_train_checkpoints));
        yield return StartCoroutine(person.straight_move(person_go.transform.position, boxcar_go.transform.position));
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

    public static Vector2 get_straight_final_dest(Orientation orientation, Vector2 tile_world_coord)
    {
        //destination absolute position is offset from center of destination tile
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

    public static Vector2Int get_straight_next_tile_pos_multiple(Orientation orientation, Vector2Int next_tilemap_pos, int times)
    {
        Vector2Int next_pos = next_tilemap_pos;
        for (int i=0; i < times; i++)
        {
            next_pos = get_straight_next_tile_pos(orientation, next_pos);
        }
        return next_pos;
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

    public static PositionPair get_next_tile_pos(Tilemap tilemap, Tile track_tile, Simple_Moving_Object moving_thing, Vector3Int tile_coord, Vector2 offset)
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
                    else if (moving_thing.orientation == Orientation.North || moving_thing.orientation == Orientation.East) // 2nd condition is for opposite direction when placing boxcars using flipped orientation
                    {
                        moving_thing.final_orientation = Orientation.East;
                        final_cell_dest = get_straight_final_dest(Orientation.East, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "NE":
                    if (moving_thing.orientation == Orientation.South || moving_thing.orientation == Orientation.East)
                    {
                        moving_thing.final_orientation = Orientation.East;
                        final_cell_dest = get_straight_final_dest(Orientation.East, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
                    }
                    else if (moving_thing.orientation == Orientation.West)
                    {
                        moving_thing.final_orientation = Orientation.North;
                        final_cell_dest = get_straight_final_dest(Orientation.North, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "WN":
                    if (moving_thing.orientation == Orientation.East)
                    {
                        moving_thing.final_orientation = Orientation.North;
                        final_cell_dest = get_straight_final_dest(Orientation.North, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
                    }
                    else if (moving_thing.orientation == Orientation.South || moving_thing.orientation == Orientation.West)
                    {
                        moving_thing.final_orientation = Orientation.West;
                        final_cell_dest = get_straight_final_dest(Orientation.West, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "WS":
                    if (moving_thing.orientation == Orientation.East)
                    {
                        moving_thing.final_orientation = Orientation.South;
                        final_cell_dest = get_straight_final_dest(Orientation.South, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y - 1);
                    }
                    else if (moving_thing.orientation == Orientation.North || moving_thing.orientation == Orientation.West)
                    {
                        moving_thing.final_orientation = Orientation.West;
                        final_cell_dest = get_straight_final_dest(Orientation.West, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "vert":
                    if (moving_thing.orientation == Orientation.North || moving_thing.orientation == Orientation.South)
                    {
                        final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                        next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "hor":
                    print(moving_thing.name + " orientation is " +  moving_thing.orientation);
                    if (moving_thing.orientation == Orientation.East || moving_thing.orientation == Orientation.West)
                    {
                        final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                        next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
                    }
                    else { throw new NullReferenceException(); }
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
                    //print("none of the track tiles matched"); // return current position
                    break;
            }
            if (tile_name == "ne_diag" || tile_name == "nw_diag" || tile_name == "se_diag" || tile_name == "sw_diag" || tile_name == "less_diag_ne_turn" ||
                tile_name == "less_diag_nw_turn" || tile_name == "less_diag_se_turn" || tile_name == "less_diag_sw_turn")
            {
                final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
            }
        }
        catch (NullReferenceException e)
        {
            final_cell_dest = tile_world_coord;
            //print("Vehicle Should not reach end of track due to look ahead. tilemap " + tilemap + " position of " + moving_thing.name + " is " + moving_thing.tile_position);
            print(e.Message);
        }
        print("final cell dest without offset is " + final_cell_dest + " + with offset is " + (final_cell_dest+offset));

        final_cell_dest += offset;
        return new PositionPair(final_cell_dest, next_tilemap_pos);
    }

    public static PositionPair get_initial_destination(MovingObject vehicle, Tilemap tilemap)
    {
        Orientation original_orientation = vehicle.orientation;
        Orientation original_final_orientation = vehicle.final_orientation;
        Tile track_tile = (Tile)tilemap.GetTile(vehicle.tile_position);
        string track_name = track_tile.name;
        vehicle.orientation = TrackManager.flip_straight_orientation(vehicle.orientation);
        PositionPair prev_pos_pair = get_next_tile_pos(tilemap, track_tile, vehicle, vehicle.tile_position, new Vector2(0,0)); // opposite direction of train to get prev tile
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

    public static PositionPair get_destination(Simple_Moving_Object moving_thing, Tilemap tilemap, Vector2 offset)
    {
        // modify by offset for a person boarding a train (so hes not standing on the tracks)
        Vector3Int tile_coord = new Vector3Int(moving_thing.tile_position[0], moving_thing.tile_position[1], 0);
        Tile track_tile = (Tile)tilemap.GetTile(tile_coord);
        Vector2 tile_world_coord = tilemap.GetCellCenterWorld(tile_coord);
        PositionPair pos_pair = get_next_tile_pos(tilemap, track_tile, moving_thing, tile_coord, offset);
        if (!moving_thing.in_city) //not inside a city, so check if arrived at city
        {
            Tile city_tile = (Tile)city_tilemap.GetTile(tile_coord);
            if (city_tile != null) //check if arriving at city
            {
                City city = GameManager.city_manager.gameobject_board[tile_coord.x, tile_coord.y].GetComponent<City>(); // check if city arrived at is not the same city we're leaving
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
