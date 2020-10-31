using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TrackManager : BoardManager
{
    //responsible for placing tracks on tilemaps, switching tracks, etc.

    Track[,] track_board;
    public static int hor_count = 0;
    public static int wn_count = 0;
    public static int vert_count = 0;
    public static int ne_count = 0;
    public static int ws_count = 0;
    public static int es_count = 0;

    //bezier vertices for a SE turn
    static Vector2 p0 = new Vector2(.44f, .44f);
    static Vector2 p1 = new Vector2(.2f, .435f);
    static Vector2 p2 = new Vector2(0, .25f);
    static Vector2 p3 = new Vector2(0, 0);
    public static List<Vector2> right_angle_curve = new List<Vector2> { p0, p1, p2, p3 };

    static Vector2 q0 = new Vector2(.22f, .66f);
    static Vector2 q1 = new Vector2(0, 0.53f);
    static Vector2 q2 = new Vector2(0, .404f);
    static Vector2 q3 = new Vector2(0, 0);
    public static List<Vector2> steep_curve = new List<Vector2> { q0, q1, q2, q3 };

    class Track 
    {
        public Track(string name)
        {
            string track_name = name;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        set_tilemap("track_layer");
        track_board = new Track[board_width, board_height];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static float get_steep_angle_rotation(RouteManager.Orientation orientation)
    {
        switch (orientation)
        {
            case RouteManager.Orientation.ne_SteepCurve:
                return -45f;
            case RouteManager.Orientation.nw_SteepCurve:
                return 45;
            case RouteManager.Orientation.se_SteepCurve:
                return 45f;
            case RouteManager.Orientation.sw_SteepCurve:
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

    public void place_tile(Vector3Int tilemap_position, string tile_name, Tile tile)
    {
        print("place tile " + tile_name + " at position " + tilemap_position);
        tilemap.SetTile(tilemap_position, tile);
        if (tile_name != "train(Clone)" && tile_name!="boxcar(Clone") // if track is clicked, add it to track map
            track_board[tilemap_position.x, tilemap_position.y] = new Track(tile_name);
    }



}
