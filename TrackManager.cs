using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TrackManager : BoardManager
{
    //responsible for placing tracks on tilemaps, switching tracks, etc.

    public static int hor_count = 0;
    public static int wn_count = 0;
    public static int vert_count = 0;
    public static int ne_count = 0;
    public static int ws_count = 0;
    public static int es_count = 0;

    static Vector2 p0 = new Vector2(.44f, .44f);
    static Vector2 p1 = new Vector2(.2f, .435f);
    static Vector2 p2 = new Vector2(0, .25f);
    static Vector2 p3 = new Vector2(0, 0);
    public static List<Vector2> right_angle_curve = new List<Vector2> { p0, p1, p2, p3 };

    //bezier vertices for a Less Steep SE turn
    static Vector2 r0 = new Vector2(.7f, .25f);
    static Vector2 r1 = new Vector2(.42f, 0.023f);
    static Vector2 r2 = new Vector2(.2f, 0f);
    static Vector2 r3 = new Vector2(0, 0);
    public static List<Vector2> less_steep_curve = new List<Vector2> { r0, r1, r2, r3 };

    //bezier vertices for a SE turn
    static Vector2 q0 = new Vector2(.22f, .66f);
    static Vector2 q1 = new Vector2(0, 0.53f);
    static Vector2 q2 = new Vector2(0, .404f);
    static Vector2 q3 = new Vector2(0, 0);
    public static List<Vector2> steep_curve = new List<Vector2> { q0, q1, q2, q3 };

    public Tile ES_tile;
    public Tile NE_tile;
    public Tile WN_tile;
    public Tile hor_tile;
    public Tile WS_tile;
    public Tile vert_tile;
    public Tile ES_tile_gray;
    public Tile NE_tile_gray;
    public Tile WN_tile_gray;
    public Tile hor_tile_gray;
    public Tile WS_tile_gray;
    public Tile vert_tile_gray;

    const int max_track_per_cell = 3;

    public List<GameObject>[,] track_grid = new List<GameObject>[board_width, board_height]; // store tracks added to a particular cell
    public static int[,] toggle_count_grid = new int[board_width, board_height];//store the toggle count, initially 0 (first track in the list). off tiles are grey, on tiles are color
    private void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        gameobject_board = new GameObject[board_width, board_height];
        for (int i = 0; i < board_width; i++)
        {
            for (int j=0; j< board_height; j++)
            {
                track_grid[i, j] = new List<GameObject>();                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Tilemap get_toggled_tilemap(Vector2Int tilemap_position)
    {
        // TODO: when vehicle gets next position, return the current next tile
        int toggle_count = toggle_count_grid[tilemap_position.x, tilemap_position.y];
        if (toggle_count == 0) return RouteManager.track_tilemap;
        else if (toggle_count == 1) return RouteManager.track_tilemap_2;
        else if (toggle_count == 2) return RouteManager.track_tilemap_3;
        else { throw new Exception("toggle count is not between 0 and 2"); }
    }

    public Tile activate_track_tile(string tile_name)
    {
        // if track is on, turn off and vise versa
        if (tile_name == "vert_track_gray") return vert_tile;
        else if (tile_name == "hor_track_gray") return hor_tile;
        else if (tile_name == "curve_NE_gray") return NE_tile;
        else if (tile_name == "curve_ES_gray") return ES_tile;
        else if (tile_name == "curve_WS_gray") return WS_tile;
        else if (tile_name == "curve_WN_gray") return WN_tile;
        else
            throw new Exception("cant place track tile because existing tile is unknown: " + tile_name);
    }

    public Tile inactivate_track_tile(string tile_name)
    {
        // if track is on, turn off and vise versa
        if (tile_name == "vert_track_gray" || tile_name=="vert_track") return vert_tile_gray;
        else if (tile_name == "hor_track_gray" || tile_name=="hor") return hor_tile_gray;
        else if (tile_name == "curve_NE_gray" || tile_name=="NE") return NE_tile_gray;
        else if (tile_name == "curve_ES_gray" || tile_name=="ES") return ES_tile_gray;
        else if (tile_name == "curve_WS_gray" || tile_name=="WS") return WS_tile_gray;
        else if (tile_name == "curve_WN_gray" || tile_name=="WN") return WN_tile_gray;
        else
            throw new Exception("cant place track tile because existing tile is unknown: " + tile_name);
    }

    public Tilemap get_tilemap(int index)
    {
        if (index == 0) return RouteManager.track_tilemap;
        else if (index == 1) return RouteManager.track_tilemap_2;
        else if (index == 2) return RouteManager.track_tilemap_3;
        return null;
    }

    public void toggle_on_train_track(Vector2Int tilemap_position)
    {
        List<GameObject> track_list = track_grid[tilemap_position.x, tilemap_position.y];
        // click on tile to toggle tracks. called from GameManager
        int toggle_count = toggle_count_grid[tilemap_position.x, tilemap_position.y];
        toggle_count_grid[tilemap_position.x, tilemap_position.y] = (toggle_count + 1) % track_list.Count;
        print("toggle count " + toggle_count);
        Tilemap track_tilemap;
        for (int i = 0; i < track_list.Count; i++) // inactivate all track tiles, except for the one toggled on
        {
            track_tilemap = get_tilemap(i);
            Tile tile = (Tile)track_tilemap.GetTile((Vector3Int)tilemap_position);
            string tile_name = tile.name;
            Tile toggled_track = inactivate_track_tile(tile_name);
            track_tilemap.SetTile((Vector3Int)tilemap_position, toggled_track);
            // inactivate the tracks that are not toggled on
            if (i == toggle_count)
            {
                toggled_track = activate_track_tile(toggled_track.name);
                track_tilemap.SetTile((Vector3Int)tilemap_position, toggled_track);
            }
        }
    }

    public void place_tile(Vector2Int tilemap_position, GameObject tile_object, Tile tile, bool display)
    {
        //add to appropriate track tilemap
        List<GameObject> track_list = track_grid[tilemap_position.x, tilemap_position.y];
        int track_count = track_list.Count;
        track_list.Add(tile_object);
        Tilemap track_tilemap = get_tilemap(track_count);
        if (track_tilemap == null)
        {
            print("max track limit reached for this cell ");
            return;
        }
        // activate tile (default) if it is the first tile in the cell, otherwise toggle off
        if (track_count != 0) tile = inactivate_track_tile(tile.name); // toggle off, gray out tile
        base.place_tile(tilemap_position, tile_object, tile, track_tilemap);
    }

    public static RouteManager.Orientation flip_straight_orientation(RouteManager.Orientation orientation)
    {
        switch (orientation)
        {
            case RouteManager.Orientation.East:
                return RouteManager.Orientation.West;
            case RouteManager.Orientation.West:
                return RouteManager.Orientation.East;
            case RouteManager.Orientation.North:
                return RouteManager.Orientation.South;
            case RouteManager.Orientation.South:
                return RouteManager.Orientation.North;
            default:
                return RouteManager.Orientation.None;
        }
    }

    public static RouteManager.Orientation is_curve_steep(RouteManager.Orientation orientation)
    {
        if (orientation == RouteManager.Orientation.ne_SteepCurve || orientation == RouteManager.Orientation.nw_SteepCurve
            || orientation == RouteManager.Orientation.sw_SteepCurve || orientation == RouteManager.Orientation.se_SteepCurve)
            return RouteManager.Orientation.Steep_Angle;
        else if (orientation == RouteManager.Orientation.ne_LessSteepCurve || orientation == RouteManager.Orientation.nw_LessSteepCurve
            || orientation == RouteManager.Orientation.sw_LessSteepCurve || orientation == RouteManager.Orientation.se_LessSteepCurve)
            return RouteManager.Orientation.Less_Steep_Angle;
        else
        {
            return RouteManager.Orientation.Right_Angle;
        }
    }

    public static string is_track_tile_exit(Vector2Int tile_pos)
    {
        // check if a track tile in the shipyard view is an exit route
        int x_pos = tile_pos.x;
        int y_pos = tile_pos.y;
        if (y_pos == 5 && x_pos >= 0 && x_pos <= 5) return "Shipyard Track Exit West";
        else if (y_pos == 5 && x_pos >= 11 && x_pos <= 16) return "Shipyard Track Exit East";
        else if (x_pos == 8 && y_pos >= 0 && y_pos <= 2) return "Shipyard Track Exit South";
        else if (x_pos == 8 && y_pos >= 8 && y_pos <= 9) return "Shipyard Track Exit North";
        else
        {
            return null;
        }
    }

    public static float get_exit_track_rotation(string exit_track_type)
    {
        // rotate from the NORTH orientation
        if (exit_track_type == "Shipyard Track Exit West")
            return 90f;
        else if (exit_track_type == "Shipyard Track Exit East")
            return -90f;
        else if (exit_track_type == "Shipyard Track Exit South")
            return 180f;
        else { return 0f;  }
    }


    public static void set_opposite_direction(string track_name, MovingObject vehicle)
    {
        switch (track_name)
        {
            //tricky curve tile updates. the train has already arrived in the tile so only adjust one coordinate
            case "ES":
                if (vehicle.orientation == RouteManager.Orientation.West)
                {
                    vehicle.orientation = RouteManager.Orientation.North;
                }
                else
                {
                    vehicle.orientation = RouteManager.Orientation.West;
                }
                return;
            case "NE":
                if (vehicle.orientation==RouteManager.Orientation.West)
                {
                    vehicle.orientation = RouteManager.Orientation.South;
                }
                else
                {
                    vehicle.orientation = RouteManager.Orientation.West;
                }
                return;
            case "WN":
                if (vehicle.orientation == RouteManager.Orientation.East)
                {
                    vehicle.orientation = RouteManager.Orientation.South;
                }
                else
                {
                    vehicle.orientation = RouteManager.Orientation.East;
                }
                return;
            case "WS":
                if (vehicle.orientation == RouteManager.Orientation.East)
                {
                    vehicle.orientation = RouteManager.Orientation.North;
                }
                else
                {
                    vehicle.orientation = RouteManager.Orientation.East;
                }
                return;
            case "vert":
                vehicle.orientation = flip_straight_orientation(vehicle.orientation);
                return;
            case "hor":
                vehicle.orientation = flip_straight_orientation(vehicle.orientation);
                return;
            case "less_diag_ne_turn":
                vehicle.orientation = RouteManager.Orientation.West;
                return;
            case "less_diag_nw_turn":
                vehicle.orientation = RouteManager.Orientation.East;
                return;
            case "less_diag_se_turn":
                vehicle.orientation = RouteManager.Orientation.West;
                return;
            case "less_diag_sw_turn":
                vehicle.orientation = RouteManager.Orientation.East;
                return;
            case "ne_diag":
                vehicle.orientation = RouteManager.Orientation.North;
                break;
            case "nw_diag":
                vehicle.orientation = RouteManager.Orientation.North;
                break;
            case "se_diag":
                vehicle.orientation = RouteManager.Orientation.South;
                break;
            case "sw_diag":
                vehicle.orientation = RouteManager.Orientation.South;
                break;
            default:
                throw new Exception("none of the track tiles matched");
        }
    }

    public static float get_steep_angle_rotation(RouteManager.Orientation orientation)
    {
        switch (orientation)
        {
            case RouteManager.Orientation.ne_SteepCurve:
                return 45f;
            case RouteManager.Orientation.nw_SteepCurve:
                return -45;
            case RouteManager.Orientation.se_SteepCurve:
                return -45f;
            case RouteManager.Orientation.sw_SteepCurve:
                return 45f;
            case RouteManager.Orientation.ne_LessSteepCurve:
                return -45f;
            case RouteManager.Orientation.nw_LessSteepCurve:
                return 45;
            case RouteManager.Orientation.se_LessSteepCurve:
                return 45f;
            case RouteManager.Orientation.sw_LessSteepCurve:
                return -45f;
            default:
                throw new Exception("not a valid steep track");
        }
    }

    public static float get_right_angle_rotation(RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
    {
        // get angle of moving object along track based on start and end orientation for  RIGHT ANGLE track
        if (orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.East ||
            orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.West ||
            orientation == RouteManager.Orientation.West && final_orientation == RouteManager.Orientation.North ||
            orientation == RouteManager.Orientation.East && final_orientation == RouteManager.Orientation.South)
        {
            return -90f; // clockwise
        }
        else
        {
            return 90f; // counterclockwise
        }
    }

    public static void add_track(string track_type)
    {
        switch (track_type)
        {
            case "vert_desc":
                vert_count++;
                break;
            case "hor_desc":
                hor_count++;
                break;
            case "wn_desc":
                wn_count++;
                break;
            case "ne_desc":
                ne_count++;
                break;
            case "ws_desc":
                ws_count++;
                break;
            case "es_desc":
                es_count++;
                break;
            default:
                break;
        }
    }
}
