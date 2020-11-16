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

    public Tilemap track_tilemap;

    private void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        gameobject_board = new GameObject[board_width, board_height];
        track_tilemap = GameObject.Find("Track Layer").GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void place_tile(Vector2Int tilemap_position, GameObject tile_object, Tile tile, bool display)
    {
        base.place_tile(tilemap_position, tile_object, tile, track_tilemap, display);
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
