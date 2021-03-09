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
    public static List<Vector2> offset_right_angle_inner_curve = new List<Vector2> { p0/2, p1/2, p2/2, p3/2 };
    public static List<Vector2> offset_right_angle_outer_curve = new List<Vector2> { p0*1.5f, p1*1.5f, p2*1.5f, p3*1.5f };


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

    public List<Tile>[,] track_grid = new List<Tile>[board_width, board_height]; // store tracks added to a particular cell
    public static int[,] toggle_count_grid = new int[board_width, board_height];//store the toggle count, initially 0 (first track in the list). off tiles are grey, on tiles are color

    public GameObject top_tilemap_go;
    public GameObject bottom_tilemap_go_1;
    public GameObject bottom_tilemap_go_2;
    public GameObject bottom_tilemap_go_3;
    public GameObject bottom_tilemap_go_4;
    public GameObject bottom_tilemap_go_5;
    public GameObject[] bottom_tilemap_go_list;

    public Tilemap top_tilemap;
    public Tilemap bottom_tilemap_1;
    public Tilemap bottom_tilemap_2;
    public Tilemap bottom_tilemap_3;
    public Tilemap bottom_tilemap_4;
    public Tilemap bottom_tilemap_5;
    public Tilemap[] bottom_tilemap_list;

    public static int[,] parking_coord = { { 4, 0, 4 }, { 4, 11, 16 }, { 6, 12, 16 }, { 6, 0, 5 } }; // y, x start, x end  used by inventory pusher
    public static Dictionary<RouteManager.Orientation, List<int>> parking_coord_map;
    public static Dictionary<RouteManager.Orientation, List<List<int[]>>> unloading_coord_map; // positions of all unloading coordinates
    public static Dictionary<RouteManager.Orientation, List<List<int[]>>> add_to_train_coord_map;
    public static Dictionary<RouteManager.Orientation, List<int[]>> exit_track_map; // positions of all exit tiles

    public static Dictionary<string, int> track_count_dict = new Dictionary<string, int>(); // <track name, building count>

    public static TrackManager instance;
    private void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            instance = this;
            initialize();
            DontDestroyOnLoad(transform.gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        //initialize_track_layer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void initialize()
    {
        track_count_dict = new Dictionary<string, int>();
        parking_coord_map = new Dictionary<RouteManager.Orientation, List<int>> // parking boxcars
        { // {{coordinates of loading tiles for outer track},{coordinates of loading tiles for inner track}}
            { RouteManager.Orientation.North, new List<int>{6,0,5} },
            { RouteManager.Orientation.East, new List<int>{6,12,16} },
            { RouteManager.Orientation.West, new List<int> {4,0,4} },
            { RouteManager.Orientation.South, new List<int> {4,11,16} }
        };

        unloading_coord_map = new Dictionary<RouteManager.Orientation, List<List<int[]>>>();

        List<int[]> unload_north_outer = new List<int[]>() { new int[] { 2, 8 }, new int[] { 3, 8 }, new int[] { 4, 8 }, new int[] { 5, 8 } };
        List<int[]> unload_north_inner = new List<int[]>() { new int[] { 2, 8 }, new int[] { 3, 8 }, new int[] { 4, 8 }, new int[] { 5, 8 } };
        List<int[]> unload_east_outer = new List<int[]>() { new int[] { 14, 8 }, new int[] { 13, 8 }, new int[] { 12, 8 }, new int[] { 11, 8 } };
        List<int[]> unload_east_inner = new List<int[]>() { new int[] { 14, 8 }, new int[] { 13, 8 }, new int[] { 12, 8 }, new int[] { 11, 8 } };
        List<int[]> unload_west_outer = new List<int[]>() { new int[] { 2, 2 }, new int[] { 3, 2 }, new int[] { 4, 2 }, new int[] { 5, 2 } };
        List<int[]> unload_west_inner = new List<int[]>() { new int[] { 2, 2 }, new int[] { 3, 2 }, new int[] { 4, 2 }, new int[] { 5, 2 } };
        List<int[]> unload_south_outer = new List<int[]> { new int[] { 11, 2 }, new int[] { 12, 2 }, new int[] { 13, 2 }, new int[] { 14, 2 } };
        List<int[]> unload_south_inner = new List<int[]> { new int[] { 11, 2 }, new int[] { 12, 2 }, new int[] { 13, 2 }, new int[] { 14, 2 } };
        unloading_coord_map[RouteManager.Orientation.North] = new List<List<int[]>>() { unload_north_outer, unload_north_inner };
        unloading_coord_map[RouteManager.Orientation.East] = new List<List<int[]>>() { unload_east_outer, unload_east_inner };
        unloading_coord_map[RouteManager.Orientation.West] = new List<List<int[]>>() { unload_west_outer, unload_west_inner };
        unloading_coord_map[RouteManager.Orientation.South] = new List<List<int[]>>() { unload_south_outer, unload_south_inner };

        List<int[]> west_outer_track = new List<int[]> { new int[] { -1, 1 }, new int[] { 0, 1 }, new int[] { 1, 1 }, new int[] { 2, 1 }, new int[] { 3, 1 }, new int[] { 4, 1 }, new int[] { 5, 1 }, new int[] { 6, 1 }, new int[] { 6, 2 } };
        List<int[]> west_inner_track = new List<int[]> { new int[] { -1, 3 }, new int[] { 0, 3 }, new int[] { 1, 3 }, new int[] { 2, 3 }, new int[] { 3, 3 }, new int[] { 4, 3 }, new int[] { 5, 3 }, new int[] { 6, 3 } };
        List<int[]> east_outer_track = new List<int[]> { new int[] { 17, 9 }, new int[] { 16, 9 }, new int[] { 15, 9 }, new int[] { 14, 9 }, new int[] { 13, 9 }, new int[] { 12, 9 }, new int[] { 11, 9 }, new int[] { 10, 9 }, new int[] { 10, 8 } };
        List<int[]> east_inner_track = new List<int[]> { new int[] { 17, 7 }, new int[] { 16, 7 }, new int[] { 15, 7 }, new int[] { 14, 7 }, new int[] { 13, 7 }, new int[] { 12, 7 }, new int[] { 11, 7 }, new int[] { 10, 7 } };
        List<int[]> north_outer_track = new List<int[]> { new int[] { 1, 9 }, new int[] { 1, 8 }, new int[] { 1, 7 }, new int[] { 2, 7 }, new int[] { 3, 7 }, new int[] { 4, 7 }, new int[] { 5, 7 }, new int[] { 6, 7 } };
        List<int[]> north_inner_track = new List<int[]> { new int[] { 2, 9 }, new int[] { 3, 9 }, new int[] { 4, 9 }, new int[] { 5, 9 }, new int[] { 6, 9 }, new int[] { 6, 8 } };
        List<int[]> south_outer_track = new List<int[]> { new int[] { 10, 3 }, new int[] { 11, 3 }, new int[] { 12, 3 }, new int[] { 13, 3 }, new int[] { 14, 3 }, new int[] { 15, 3 }, new int[] { 15, 2 }, new int[] { 15, 1 }, new int[] { 15, 0 } };
        List<int[]> south_inner_track = new List<int[]> { new int[] { 10, 2 }, new int[] { 10, 1 }, new int[] { 11, 1 }, new int[] { 12, 1 }, new int[] { 13, 1 }, new int[] { 14, 1 }, new int[] { 14, 0 } };
        add_to_train_coord_map = new Dictionary<RouteManager.Orientation, List<List<int[]>>>();
        add_to_train_coord_map[RouteManager.Orientation.West] = new List<List<int[]>>() { west_outer_track, west_inner_track };
        add_to_train_coord_map[RouteManager.Orientation.North] = new List<List<int[]>>() { north_outer_track, north_inner_track };
        add_to_train_coord_map[RouteManager.Orientation.East] = new List<List<int[]>>() { east_outer_track, east_inner_track };
        add_to_train_coord_map[RouteManager.Orientation.South] = new List<List<int[]>>() { south_outer_track, south_inner_track };

        exit_track_map = new Dictionary<RouteManager.Orientation, List<int[]>>
        { // {{coordinates of loading tiles for outer track},{coordinates of loading tiles for inner track}}
            { RouteManager.Orientation.West, new List<int[]> { new int[] { -1, 5 }, new int[] { 0,5 }, new int[] { 1,5 }, new int[]{2,5 }, new int[]{3,5 }, new int[]{4,5 }, new int[]{5,5 }}},
            { RouteManager.Orientation.East, new List<int[]> { new int[] { 11,5 }, new int[] { 12,5 }, new int[]{13,5 }, new int[]{14,5 }, new int[]{15,5 }, new int[]{16,5 }}},
            { RouteManager.Orientation.South, new List<int[]> { new int[] { 8,0 }, new int[] { 8,1 }, new int[]{8,2 }} },
            { RouteManager.Orientation.North, new List<int[]> { new int[] { 8,8 }, new int[] { 8,9 }} }
        };

        bottom_tilemap_list = new Tilemap[] { bottom_tilemap_1, bottom_tilemap_2, bottom_tilemap_3, bottom_tilemap_4, bottom_tilemap_5 };
        bottom_tilemap_go_list = new GameObject[] { bottom_tilemap_go_1, bottom_tilemap_go_2, bottom_tilemap_go_3, bottom_tilemap_go_4, bottom_tilemap_go_5 };

        gameobject_board = new GameObject[board_width, board_height];
        for (int i = 0; i < board_width; i++)
        {
            for (int j = 0; j < board_height; j++)
            {
                track_grid[i, j] = new List<Tile>();
                top_tilemap.SetTile(new Vector3Int(i, j, 0), null); // remove all tracks to start new scene
            }
        }
    }

    public static bool is_location_in_list(List<int[]> loc_list, Vector2Int loc)
    {
        for (int i = 0; i < loc_list.Count; i++)
        {
            int[] loc_pair = loc_list[i];
            if (loc.x == loc_pair[0] && loc.y == loc_pair[1]) return true;
        }
        return false;
    }

    public static Station_Track get_station_from_location(Vector2Int location, City city)
    {
        foreach (KeyValuePair<RouteManager.Orientation, List<List<int[]>>> entry in add_to_train_coord_map)
        {
            List<int[]> outer_location_list = entry.Value[0];
            List<int[]> inner_location_list = entry.Value[1];
            if (is_location_in_list(outer_location_list, location))
            {
                bool is_outer = true;
                return city.get_station_track(is_outer, entry.Key);
            }
            if (is_location_in_list(inner_location_list, location))
            {
                bool is_outer = false;
                return city.get_station_track(is_outer, entry.Key);
            }
        }
        return null; // not a valid station location. do nothing
    }


    //public void initialize_track_layer()
    //{
    //    // add tracks that pre-exist to the grid
    //    for (int i = 0; i < board_width; i++)
    //    {
    //        for (int j = 0; j < board_height; j++)
    //        {
    //            Vector3Int tile_pos = new Vector3Int(i, j, 0);
    //            Tile tile = (Tile) top_tilemap.GetTile(tile_pos);
    //            if (tile != null)
    //                track_grid[i, j].Add(tile);
    //        }
    //    }
    //}

    public static bool is_track_a_path(RouteManager.Orientation prior_orientation, string next_track_tile, string cur_track_tile)
    {
        // check if train can traverse a placed tile
        // get orientation AFTER moving current tile, not before
        RouteManager.Orientation orientation = TrackManager.get_direction_from_orientation(cur_track_tile, prior_orientation);
        if (orientation == RouteManager.Orientation.North)
            if (next_track_tile == "WS" || next_track_tile == "ES" || next_track_tile == "vert")
                return true;
        if (orientation == RouteManager.Orientation.East)
            if (next_track_tile == "WN" || next_track_tile == "WS" || next_track_tile == "hor")
                return true;
        if (orientation == RouteManager.Orientation.West)
            if (next_track_tile == "NE" || next_track_tile == "ES" || next_track_tile == "hor")
                return true;
        if (orientation == RouteManager.Orientation.South)
            if (next_track_tile == "WN" || next_track_tile == "NE" || next_track_tile == "vert")
                return true;
        return false;
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
        if (tile_name == "vert_track_gray" || tile_name=="vert") return vert_tile_gray;
        else if (tile_name == "hor_track_gray" || tile_name=="hor") return hor_tile_gray;
        else if (tile_name == "curve_NE_gray" || tile_name=="NE") return NE_tile_gray;
        else if (tile_name == "curve_ES_gray" || tile_name=="ES") return ES_tile_gray;
        else if (tile_name == "curve_WS_gray" || tile_name=="WS") return WS_tile_gray;
        else if (tile_name == "curve_WN_gray" || tile_name=="WN") return WN_tile_gray;
        else
            throw new Exception("cant place track tile because existing tile is unknown: " + tile_name);
    }

    public void toggle_on_train_track(Vector2Int tilemap_position)
    {
        List<Tile> track_list = track_grid[tilemap_position.x, tilemap_position.y];
        // click on tile to toggle tracks. called from GameManager
        int toggle_count = toggle_count_grid[tilemap_position.x, tilemap_position.y];
        toggle_count = (toggle_count + 1) % track_list.Count; // increment toggle count to activate the next track tile
        toggle_count_grid[tilemap_position.x, tilemap_position.y] = toggle_count;
        int bottom_tilemap_idx = 0;
        for (int i = 0; i < track_list.Count; i++) // inactivate all track tiles, except for the one toggled on
        {
            Tile tile = track_list[i];
            string tile_name = tile.name;
            //track_tilemap.SetTile((Vector3Int)tilemap_position, toggled_track);

            // todo: set tile belonging to the tilemap first in sort order

            // inactivate the tracks that are not toggled on
            if (i != toggle_count) // not an activated track
            {
                Tile toggled_track = inactivate_track_tile(tile_name);
                Tilemap track_tilemap = bottom_tilemap_list[bottom_tilemap_idx];
                track_tilemap.SetTile((Vector3Int)tilemap_position, toggled_track);
                bottom_tilemap_idx += 1;
            }
            else
            {
                top_tilemap.SetTile((Vector3Int)tilemap_position, tile);
            }
        }
    }

    public void place_tile(Vector2Int tilemap_position, Tile tile, bool display)
    {
        //add to appropriate track tilemap
        List<Tile> track_list = track_grid[tilemap_position.x, tilemap_position.y];
        int track_count = track_list.Count;
        if (track_count < 6)
        {
            foreach (Tile track_tile in track_list)
            {
                if (tile == track_tile)
                    return;
            }
            track_list.Add(tile);
            track_count = track_list.Count;
            if (track_count > 1)
            {
                Tilemap track_tilemap = bottom_tilemap_list[track_count - 2];
                // activate tile (default) if it is the first tile in the cell, otherwise toggle off
                tile = inactivate_track_tile(tile.name); // toggle off, gray out tile
                track_tilemap.SetTile((Vector3Int)tilemap_position, tile);
            } else
            {
                top_tilemap.SetTile((Vector3Int)tilemap_position, tile);
            }

        }
    }

    public static RouteManager.Orientation enter_boxcar_orientation(Vector2 boxcar_pos, Vector2 person_pos)
    {
        Vector2 delta_position = person_pos - boxcar_pos;
        if (Mathf.Abs(delta_position.x) > Mathf.Abs(delta_position.y))
        { // horizontal movement
            if (delta_position.x > 0)
                return RouteManager.Orientation.West;
            else
                return RouteManager.Orientation.East;
        }
        else
        {
            if (delta_position.y > 0)
                return RouteManager.Orientation.South;
            else
                return RouteManager.Orientation.North;
        }
    }

    public static float get_orientation_angle(RouteManager.Orientation orientation)
    {
        float final_angle = -1;
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                final_angle = 90;
                break;
            case RouteManager.Orientation.East:
                final_angle = 0;// random_algos.degree_to_radian(-90);
                break;
            case RouteManager.Orientation.West:
                final_angle = 180;// random_algos.degree_to_radian(90);
                break;
            case RouteManager.Orientation.South:
                final_angle = 270;// random_algos.degree_to_radian(180);
                break;
        }
        return final_angle;
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

    public static RouteManager.Orientation get_steep_orientation(string track_name)
    {
        if (track_name == "less_diag_ne_turn")
        {
            return RouteManager.Orientation.ne_LessSteepCurve;
        }
        else if (track_name == "less_diag_nw_turn")
        {
            return RouteManager.Orientation.nw_LessSteepCurve;
        }
        else if (track_name == "less_diag_se_turn")
        {
            return RouteManager.Orientation.se_LessSteepCurve;
        }
        else if (track_name == "less_diag_sw_turn")  
        {
            return RouteManager.Orientation.sw_LessSteepCurve;
        }
        else if (track_name == "ne_diag")
        {
            return RouteManager.Orientation.ne_SteepCurve;
        }
        else if (track_name == "se_diag")
        {
            return RouteManager.Orientation.se_SteepCurve;
        }
        else if (track_name == "nw_diag")
        {
            return RouteManager.Orientation.nw_SteepCurve;
        }
        else if (track_name == "sw_diag")
        {
            return RouteManager.Orientation.sw_SteepCurve;
        }
        else
        {
            return RouteManager.Orientation.None;
        }

    }

    public static bool is_curve_steep(string track_name)
    {
        if (track_name == "less_diag_ne_turn" || track_name == "less_diag_nw_turn" || track_name == "less_diag_se_turn" || track_name == "less_diag_sw_turn" ||
            track_name == "ne_diag" || track_name == "se_diag" || track_name == "nw_diag" || track_name == "sw_diag")
            return true;
        else
        {
            return false;
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

    public static RouteManager.Orientation get_x_axis_orientation(Vector3 origin, Vector3 destination)
    {
        if (destination.x > origin.x)
        {
            return RouteManager.Orientation.East;
        }
        else if (destination.x < origin.x)
        {
            return RouteManager.Orientation.West;
        }
        else
        {
            throw new Exception("get start orientation notn aligned with track");
        }
    }

    public static RouteManager.Orientation get_y_axis_orientation(Vector3 origin, Vector3 destination)
    {
        if (destination.y > origin.y)
        {
            return RouteManager.Orientation.North;
        }
        else if (destination.y < origin.y)
        {
            return RouteManager.Orientation.South;
        }
        else
        {
            throw new Exception("get start orientation notn aligned with track");
        }
    }

    public static RouteManager.Orientation get_start_orientation(string track_tile_name, Vector3 track_location, Vector3 destination, Boxcar boxcar)
    {
        // boarding train: home -> boxcar
        // when a train is instantiated, its orientation must match direction of trac
        // get orientation relative to city
        RouteManager.Orientation station_orientation = boxcar.station_track.station.orientation;
        int inner_track = boxcar.station_track.inner; // 0 if false, 1 if true
        if (track_tile_name == "vert")
        {
            return get_y_axis_orientation(track_location, destination);
        }

        else if (track_tile_name == "hor")
        {
            return get_x_axis_orientation(track_location, destination);
        }
        else if (track_tile_name == "NE")
        {
            if (station_orientation == RouteManager.Orientation.South) return RouteManager.Orientation.East;
            else
            {
                if (inner_track == 0) return get_y_axis_orientation(track_location, destination); // RouteManager.Orientation.West; // unloading!
                else { return get_x_axis_orientation(track_location, destination); } // unloading or boarding
            }
        }
        else if (track_tile_name == "ES")
        {
            return get_x_axis_orientation(track_location, destination);
        }
        else if (track_tile_name == "WS")
        {
            if (station_orientation == RouteManager.Orientation.North)
            {
                return get_x_axis_orientation(track_location, destination);
            }
            else // SOUTH STATION
            {
                if (inner_track == 0) return get_x_axis_orientation(track_location, destination);
                else { return RouteManager.Orientation.North; } // unloading
            }
        }
        else if (track_tile_name == "WN")
        {
            return get_x_axis_orientation(track_location, destination);
        }
        else
        {
            throw new Exception("no start orientation found");
        }
    }

    public static RouteManager.Orientation get_direction_from_orientation(string track_name, RouteManager.Orientation orientation)
    {
        switch (track_name)
        {
            case "ES":
                if (orientation == RouteManager.Orientation.West)
                {
                    return RouteManager.Orientation.South;
                }
                else
                {
                    return RouteManager.Orientation.East;
                }
            case "NE":
                if (orientation == RouteManager.Orientation.West)
                {
                    return RouteManager.Orientation.North;
                }
                else
                {
                    return RouteManager.Orientation.East;
                }
            case "WN":
                if (orientation == RouteManager.Orientation.East)
                {
                    return RouteManager.Orientation.North;
                }
                else
                {
                    return RouteManager.Orientation.West;
                }
            case "WS":
                if (orientation == RouteManager.Orientation.East)
                {
                    return RouteManager.Orientation.South;
                }
                else
                {
                    return RouteManager.Orientation.West;
                }
            default:
                return orientation;
        }
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

    public static double get_final_rotation(float current_angle, Vector2 cur_position, Vector2 final_position)
    {
        double delta_y = final_position.y - cur_position.y;
        double delta_x = final_position.x - cur_position.x;
        double rot = Math.Atan2(delta_y, delta_x);
        return rot;
    }

    public static float get_rotation(float cur_angle, RouteManager.Orientation orientation)
    {
        float final_angle = get_rotation(RouteManager.Orientation.East, orientation); // East is aligned with x axis
        return final_angle - cur_angle;
    }

    public static float get_rotation(RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
    {
        if (orientation == RouteManager.Orientation.West && final_orientation == RouteManager.Orientation.East ||
            orientation == RouteManager.Orientation.East && final_orientation == RouteManager.Orientation.West ||
            orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.South ||
            orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.North)
            return 180f;
        // get angle of moving object along track based on start and end orientation for  RIGHT ANGLE track
        else if (orientation == final_orientation)
        {
            return 0;
        }
        else if (orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.East ||
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

    public static int get_track_count(string track_name)
    {
        if (track_count_dict.ContainsKey(track_name))
            return track_count_dict[track_name];
        else
        {
            return 0;
        }
    }

    public static void update_track_count(string track_name, int update_num)
    {
        if (track_count_dict.ContainsKey(track_name))
        {
            track_count_dict[track_name] += update_num;
        }
        else
        {
            track_count_dict[track_name] = 1;
        }
    }
}
