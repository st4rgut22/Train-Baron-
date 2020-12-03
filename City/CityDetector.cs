﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Tilemaps;
public class CityDetector : EventDetector
{
    public Tilemap offset_hint_tilemap_north;
    public Tilemap offset_hint_tilemap_east;
    public Tilemap offset_hint_tilemap_west;
    public Tilemap offset_hint_tilemap_south;
    public Dictionary<Vector2Int, Tilemap> boxcar_tile_tracker = new Dictionary<Vector2Int, Tilemap>();
    public List<Offset_Tile> offset_tile_list;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public class Offset_Tile
    {
        float start_x;
        float end_x;
        float start_y;
        float end_y;
        public Boxcar boxcar;
        public GameObject boxcar_go;
        Vector2 boxcar_position;
        public Offset_Tile(GameObject boxcar_go)
        {
            this.boxcar_go = boxcar_go;
            this.boxcar = boxcar_go.GetComponent<Boxcar>();
            boxcar_position = boxcar.transform.position;
            float offset = RouteManager.cell_width / 2;
            start_x = boxcar_position.x - offset;
            end_x = boxcar_position.x + offset;
            start_y = boxcar_position.y - offset;
            end_y = boxcar_position.y + offset;
        }
        public bool is_in_boxcar_tile(Vector2 position)
        {
            // is click position in box surrounding the boxcar?
            if (position.x <= end_x && position.x >= start_x && position.y >= start_y && position.y <= end_y)
                return true;
            else
                return false;
        }
        public float get_distance(Vector2 position)
        {
            return Vector2.Distance(position, boxcar_position);
        }
    }

    public GameObject is_boxcar_tile_clicked(Vector2 click_position)
    {
        Offset_Tile closest_offset_tile = null;
        float closest_distance_to_boxcar = Mathf.Infinity;
        for (int i=0; i < offset_tile_list.Count; i++)
        {
            Offset_Tile offset_tile = offset_tile_list[i];
            if (offset_tile.is_in_boxcar_tile(click_position))
            {
                float distance_to_boxcar = offset_tile.get_distance(click_position);
                if (distance_to_boxcar < closest_distance_to_boxcar)
                {
                    closest_distance_to_boxcar = distance_to_boxcar;
                    closest_offset_tile = offset_tile;
                }
            }
        }
        if (closest_offset_tile != null)
            return closest_offset_tile.boxcar_go;
        else
            return null;
    }

    public bool is_track_contain_valid_boxcar(bool is_outer, Station station, string building_type)
    {
        Station_Track adjacent_station_track;
        if (is_outer) adjacent_station_track = station.outer_track;
        else { adjacent_station_track = station.inner_track; }
        GameObject train_object = adjacent_station_track.train;
        if (train_object != null)
        {
            Train train = train_object.GetComponent<Train>();
            List<GameObject>boxcar_list = train.boxcar_squad;
            foreach (GameObject boxcar_object in boxcar_list)
            {
                Boxcar boxcar = boxcar_object.GetComponent<Boxcar>();
                List<string> valid_structure = CityManager.cargo_to_structure[boxcar.boxcar_type];
                if (valid_structure.Contains(building_type))
                    return true;
            }
        }
        return false;
    }

    public Tilemap boxcar_orientation_to_offset_tilemap(RouteManager.Orientation orientation)
    {
        //convert boxcar orientation to tilemaps to be used for marking the boxcar tiles, which are offset from the original tilemap by half a cell
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                return offset_hint_tilemap_north;
            case RouteManager.Orientation.East:
                return offset_hint_tilemap_east;
            case RouteManager.Orientation.West:
                return offset_hint_tilemap_west;
            case RouteManager.Orientation.South:
                return offset_hint_tilemap_south;
            default:
                return null;
        }
    }

    public List<int[]> filter_available_boxcar(List<int[]> possible_boxcar_location)
    {
        int available_boxcar = 0;
        offset_tile_list = new List<Offset_Tile>();
        List<int[]> available_boxcar_list = new List<int[]>();
        GameObject[,] city_board = CityManager.Activated_City_Component.city_board;
        foreach (int[] boxcar_loc in possible_boxcar_location)
        {
            try
            {
                GameObject boxcar_go = city_board[boxcar_loc[0], boxcar_loc[1]];
                if (boxcar_go != null && boxcar_go.tag == "boxcar")
                {
                    Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
                    if (!boxcar.is_occupied)
                    {
                        Tilemap boxcar_tilemap = boxcar_orientation_to_offset_tilemap(boxcar.orientation);
                        offset_tile_list.Add(new Offset_Tile(boxcar_go));
                        boxcar_tile_tracker[new Vector2Int(boxcar_loc[0], boxcar_loc[1])] = boxcar_tilemap;
                        available_boxcar_list.Add(boxcar_loc);
                        available_boxcar++;
                    }
                }
            }
            catch (IndexOutOfRangeException) { print("eligible boxcar location " + boxcar_loc + " is not suitable for boxcar placement"); };
          
        }
        print("available boxcar tally is " + available_boxcar);
        return available_boxcar_list;
    }

    public void click_city(PointerEventData eventData)
    {
        List<List<int[]>> city_action_coord = new List<List<int[]>>();
        print("set a city hint in frame " + Time.frameCount);
        // get station
        // get adjacent boarding track
        // if train is on track, then search for appropriate boxcars on track
        List<string> train_hint_list = new List<string>();
        Vector2Int selected_tile = GameManager.get_selected_tile(eventData.position);
        // get room, and check if it is occupied before boarding
        bool is_room_occupied = CityManager.Activated_City_Component.is_selected_room_occupied(selected_tile, gameObject.name);
        string building_type = CityManager.Activated_City_Component.city_type;
        int id = CityManager.Activated_City_Component.city_id;
        if (is_room_occupied)
        {
            print("room is occupied");
            Station cb_station = CityManager.Activated_City_Component.get_station_track(selected_tile).station;
            RouteManager.Orientation orientation = cb_station.orientation;
            bool[] outer_inner_arr = TrackManager.is_city_building_inner(selected_tile, orientation); // 0 means outer, 1 means inner
            bool is_outer_track_valid = false;
            bool is_inner_track_valid = false;
            if (outer_inner_arr[0]) is_outer_track_valid = is_track_contain_valid_boxcar(true, cb_station, building_type);
            if (outer_inner_arr[1]) is_inner_track_valid = is_track_contain_valid_boxcar(false, cb_station, building_type);
            if (is_outer_track_valid || is_inner_track_valid)
            {
                List<int[]> inner_outer_coord = new List<int[]>();
                if (is_outer_track_valid)
                {
                    List<int[]> available_outer_track_vehicle = filter_available_boxcar(TrackManager.add_to_train_coord_map[orientation][0]);
                    inner_outer_coord.AddRange(available_outer_track_vehicle);
                }
                if (is_inner_track_valid)
                {
                    List<int[]> available_inner_track_vehicle = filter_available_boxcar(TrackManager.add_to_train_coord_map[orientation][1]);
                    inner_outer_coord.AddRange(available_inner_track_vehicle);
                }
                city_action_coord.Add(inner_outer_coord);
                train_hint_list.Add("board");
                GameObject.Find("GameManager").GetComponent<GameManager>().mark_tile_as_eligible(city_action_coord, train_hint_list, gameObject, true);
            }
        }
    }
}