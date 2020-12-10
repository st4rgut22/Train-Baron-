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
    public static float offset_amount = .1f;

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
                // no offset for the curves, because this is already applied in bezier movement
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
                        },
                        {
                            "vert", offset_left
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

    public static Vector2 get_straight_walking_position(Vector2 middle_pos, Orientation orientation)
    {
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

    public static Vector2 offset_direction_of_travel(Orientation orientation)
    {
        // prevent running into walls and whatnot
        Vector2 offset = new Vector2(0, 0);
        switch (orientation)
        {
            case Orientation.North:
                offset.y = -offset_amount;
                break;
            case Orientation.East:
                offset.x = -offset_amount;
                break;
            case Orientation.West:
                offset.x = offset_amount;
                break;
            case Orientation.South:
                offset.y += offset_amount;
                break;
        }
        return offset;
    }

    public static Checkpoint get_exit_home_checkpoint(Orientation orientation, Transform transform, Orientation exit_orientation, Boxcar boxcar, Room room, Vector2 door_position)
    {
        // rotate to face door, then exit in straight path
        Vector2Int doorstep_position = get_straight_next_tile_pos(exit_orientation, room.tile_position);
        Vector2 middle_pos = track_tilemap.GetCellCenterWorld((Vector3Int)doorstep_position);
        Vector2 offset = get_doorstep_offset(exit_orientation, door_position, middle_pos);
        middle_pos += offset;
        Vector2 exit_home_dest = get_straight_walking_position(middle_pos, exit_orientation);
        float rotation = CityManager.orientation_to_rotation_map[orientation]; // current rotation. eg EAST equals degree 0
        return new Checkpoint(transform.position, rotation, exit_home_dest, doorstep_position); //(angle, exit_orientation, exit_home_dest, doorstep_position);
    }

    public static Vector2 get_doorstep_offset(Orientation exit_home_orientation, Vector2 door_position, Vector2 dest_tile_center)
    {
        Vector2 offset = new Vector2(0, 0);
        if (exit_home_orientation == Orientation.North || exit_home_orientation == Orientation.South)
        {
            if (door_position.x < dest_tile_center.x)
                offset.x = -cell_width / 4;
            else { offset.x = cell_width / 4;  }           
        }
        else
        {
            if (door_position.y < dest_tile_center.y)
                offset.y = -cell_width / 4;
            else { offset.y = cell_width / 4;  }
        }
        return offset;
    }

    public static Checkpoint get_find_door_checkpoint(Orientation begin_orientation, Orientation exit_home_orientation, Vector2 position, Vector2 door_position, Vector2Int tile_position)
    {
        Orientation final_orientation = Orientation.None;
        Vector2 final_position = new Vector2(-1, -1);
        if (exit_home_orientation == Orientation.West || exit_home_orientation == Orientation.East)
        {
            if (position.y < door_position.y)
                final_orientation = Orientation.North;
            else 
                final_orientation = Orientation.South;
            final_position = new Vector2(position.x, door_position.y);
        }
        if (exit_home_orientation == Orientation.North || exit_home_orientation == Orientation.South)
        {
            if (position.x < door_position.x) final_orientation = Orientation.East;
            else { final_orientation = Orientation.West; }
            final_position = new Vector2(door_position.x, position.y);
        }
        Vector2 offset = offset_direction_of_travel(final_orientation);
        final_position += offset;
        if (final_orientation == Orientation.None || final_position.Equals(new Vector2(-1, -1))) throw new Exception("bad value in find door cp");
        print("final position of find door cp is " + final_position + " final orientation is " + final_orientation);
        return new Checkpoint(begin_orientation, final_orientation, final_position, tile_position);
    }

    public static GameObject get_exit_door(Boxcar boxcar, Room room)
    {
        // get door that exits to the boxcar
        GameObject door_parent;
        int is_inner = boxcar.station_track.inner;
        if (is_inner == 0) door_parent = room.bl_door;
        else { door_parent = room.br_door; }
        return door_parent.transform.GetChild(0).gameObject;
    }

    public IEnumerator unload_train(Boxcar boxcar, Room room, Orientation exit_orientation)
    {
        List<Checkpoint> go_home_checkpoints = new List<Checkpoint>();
        GameObject person_go = boxcar.passenger_go;
        boxcar.is_occupied = false;
        person_go.transform.parent = null;
        Person person = person_go.GetComponent<Person>();
        room.unlocked_door = get_exit_door(boxcar, room);
        // the final dest tile is offset from the person's route, so find the offset using city map
        Orientation home_orientation = City.station_track_boarding_map[boxcar.station_track.start_location];
        person.room = room; // assign room to person
        person.final_dest_tile_pos = (Vector3Int) get_straight_next_tile_pos(home_orientation,room.tile_position);//(Vector3Int) room.tile_position;
        person.enter_home_orientation = TrackManager.flip_straight_orientation(home_orientation); // reverse direction of exiting the home
        Orientation flip_exit_orientation = TrackManager.flip_straight_orientation(exit_orientation); // need to flip because offset is in the opposite direction the person is traveling
        Vector2 exit_boxcar_dest = get_straight_walking_position(boxcar.transform.position, flip_exit_orientation);
        Checkpoint exit_boxcar_checkpoint = new Checkpoint(person.orientation, exit_orientation, exit_boxcar_dest, (Vector2Int)boxcar.tile_position);
        string track_name = shipyard_track_tilemap.GetTile(boxcar.tile_position).name;
        go_home_checkpoints.Add(exit_boxcar_checkpoint);
        Orientation exit_home_orientation = City.station_track_boarding_map[boxcar.station_track.start_location];
        Vector2Int doorstep_position = get_straight_next_tile_pos(exit_home_orientation, room.tile_position);
        Vector2 room_abs_position = track_tilemap.GetCellCenterWorld((Vector3Int)doorstep_position); //one tile offset from home in the center. Because boxcars always stop at tile edges, so center tells us which direction person should exit boxcar
        Orientation align_track_orientation = TrackManager.get_start_orientation(track_name, boxcar.transform.position, room_abs_position, exit_boxcar_checkpoint.end_orientation);
        Checkpoint face_track_cp = new Checkpoint(exit_boxcar_checkpoint.end_orientation, align_track_orientation, exit_boxcar_checkpoint.dest_pos, exit_boxcar_checkpoint.tile_position); // dont move just rotate
        go_home_checkpoints.Add(face_track_cp);
        yield return StartCoroutine(person.move_checkpoints(go_home_checkpoints));
        boxcar.passenger_go = null;
        person.boxcar_go = null;
        //person.in_tile = false;
        person.is_exit_boxcar = true; // set off follow track back home sequence
    }

    public IEnumerator board_train(Boxcar boxcar, Room room, Person occupant, Vector3Int destination)
    {
        GameObject door = get_exit_door(boxcar, room);
        Vector2 door_location = door.transform.position;
        room.unlocked_door = door;
        // 2 checkpoints. In first go to doorstep and rotate accordingly. In the next only rotate to face track direction
        List<Checkpoint> board_train_checkpoints = new List<Checkpoint>();
        Vector3Int train_start_location = boxcar.train.station_track.start_location; // id of track
        Orientation exit_orientation = City.station_track_boarding_map[train_start_location];
        Checkpoint exit_home_cp = get_exit_home_checkpoint(occupant.orientation, occupant.transform, exit_orientation,  boxcar, room, door_location);
        Checkpoint face_track_cp = new Checkpoint(exit_home_cp.final_angle, exit_home_cp.dest_pos, exit_orientation);
        string track_name = shipyard_track_tilemap.GetTile((Vector3Int)exit_home_cp.tile_position).name;
        Orientation align_track_orientation = TrackManager.get_start_orientation(track_name, exit_home_cp.dest_pos, boxcar.transform.position, exit_home_cp.end_orientation);
        Checkpoint align_track_cp = new Checkpoint(face_track_cp.end_orientation, align_track_orientation, exit_home_cp.dest_pos, exit_home_cp.tile_position); // dont move just rotate
        occupant.final_dest_tile_pos = boxcar.tile_position;
        //board_train_checkpoints.Add(face_door_cp);
        board_train_checkpoints.Add(exit_home_cp);
        board_train_checkpoints.Add(face_track_cp);
        board_train_checkpoints.Add(align_track_cp);
        yield return StartCoroutine(occupant.move_checkpoints(board_train_checkpoints));
        print("start follow track sequence");
        occupant.in_tile = false; // allow person to follow the track to the destination boxcar
                                  // if all goes well then boarding is all that's left
        occupant.exit_home();
        occupant.final_orientation = occupant.orientation;
    }

    public IEnumerator step_on_boxcar(GameObject person_go, GameObject boxcar_go)
    {
        Person person = person_go.GetComponent<Person>();
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        Vector2 boxcar_position = boxcar_go.transform.position;
        Orientation final_orientation = TrackManager.enter_boxcar_orientation(boxcar_position, person_go.transform.position);
        print("enter boxcar cp with person orientation " + person.orientation + " final orientation " + final_orientation + " boxcar tile pos " + boxcar.tile_position);
        Checkpoint step_on_boxcar_cp = new Checkpoint(person.orientation, final_orientation, boxcar.transform.position, (Vector2Int)boxcar.tile_position);
        List<Checkpoint> board_train_checkpoints = new List<Checkpoint>() { step_on_boxcar_cp };
        yield return StartCoroutine(person.move_checkpoints(board_train_checkpoints));
        person.transform.parent = boxcar.transform; // make person a passenger of boxcar
    }

    public IEnumerator enter_home(GameObject person_go, Room room)
    {
        List<Checkpoint> enter_home_checkpoints = new List<Checkpoint>();
        Person person = person_go.GetComponent<Person>();
        Vector2 enter_door_location = room.unlocked_door.transform.position;
        Vector2 room_position = track_tilemap.GetCellCenterWorld((Vector3Int)room.tile_position);
        Vector3 center_doorstep_pos = person.transform.position + person.transform.up * .5f;
        Checkpoint center_cp = new Checkpoint(person.orientation, person.orientation, center_doorstep_pos, (Vector2Int)person.tile_position);
        float center_cp_angle = CityManager.orientation_to_rotation_map[person.orientation];
        Checkpoint enter_door_cp = new Checkpoint(center_cp.dest_pos, center_cp_angle, enter_door_location, room.tile_position);
        Checkpoint enter_home_cp = new Checkpoint(enter_door_cp.dest_pos, enter_door_cp.final_angle, room_position, room.tile_position);
        Checkpoint resting_cp = new Checkpoint(enter_home_cp.final_angle, enter_home_cp.dest_pos, room.building.building_orientation);
        enter_home_checkpoints.Add(center_cp);
        enter_home_checkpoints.Add(enter_door_cp);
        enter_home_checkpoints.Add(enter_home_cp);
        enter_home_checkpoints.Add(resting_cp); // rotate person so facing same direction as building
        yield return StartCoroutine(person.move_checkpoints(enter_home_checkpoints));
        room.occupied = true;
        room.person_go = person_go;
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
        //print("final cell dest without offset is " + final_cell_dest + " + with offset is " + (final_cell_dest+offset));

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
