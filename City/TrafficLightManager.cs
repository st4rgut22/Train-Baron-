using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TrafficLightManager : Simple_Moving_Object
{
    public Traffic_Light[,] traffic_light_matrix;

    public Tile green_light_tile;
    public Tile yellow_light_tile;
    public Tile red_light_tile;
    public Vector2Int north_light_tile_pos;
    public Vector2Int east_light_tile_pos;
    public Vector2Int west_light_tile_pos;
    public Vector2Int south_light_tile_pos;
    public bool change_traffic_signal_flag;
    public Traffic_Light_Loc north_light_loc;
    public Traffic_Light_Loc east_light_loc;
    public Traffic_Light_Loc west_light_loc;
    public Traffic_Light_Loc south_light_loc;

    public enum Traffic_Light
    {
        None, 
        Green,
        Yellow,
        Red
    }

    private void Awake()
    {
        north_light_tile_pos = new Vector2Int(7, 8);
        west_light_tile_pos = new Vector2Int(5, 4);
        east_light_tile_pos = new Vector2Int(11, 6);
        south_light_tile_pos = new Vector2Int(9, 2);
        in_tile = true; // dont enter update loop of parent
        change_traffic_signal_flag = false;
        traffic_light_matrix = new Traffic_Light[BoardManager.board_width, BoardManager.board_height];
    }

    // Start is called before the first frame update
    void Start()
    {
        north_light_loc = new Traffic_Light_Loc(north_light_tile_pos, GameManager.traffic_tilemap_offset_east, RouteManager.Orientation.North);
        east_light_loc = new Traffic_Light_Loc(east_light_tile_pos, GameManager.traffic_tilemap_offset_south, RouteManager.Orientation.East);
        west_light_loc = new Traffic_Light_Loc(west_light_tile_pos, GameManager.traffic_tilemap_offset_north, RouteManager.Orientation.West);
        south_light_loc = new Traffic_Light_Loc(south_light_tile_pos, GameManager.traffic_tilemap_offset_west, RouteManager.Orientation.South);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class Traffic_Light_Loc
    {
        public Vector3Int tile_pos;
        public Tilemap traffic_tilemap;
        public RouteManager.Orientation orientation;
        public Traffic_Light_Loc(Vector2Int tile_pos, Tilemap traffic_tilemap, RouteManager.Orientation orientation)
        {
            this.tile_pos = (Vector3Int)tile_pos;
            this.traffic_tilemap = traffic_tilemap;
            this.orientation = orientation;
        }
    }

    public bool is_end_of_track()
    {
        Tilemap next_tilemap = GameManager.track_manager.top_tilemap;
        if (next_tilemap.GetTile((Vector3Int)next_tilemap_position) == null)
            return true;
        else
            return false;
    }

    public Traffic_Light update_signal(Traffic_Light signal, Traffic_Light new_signal)
    {
        // return the traffic signal with higher prioirity
        if ((int)new_signal > (int)signal) return new_signal;
        else { return signal; }
    }

    public void set_signal_from_exit_route(Vector3Int city_tile_position, Traffic_Light_Loc traffic_light_loc)
    {
        bool exit_route_is_shown = CityManager.is_exit_route_shown(traffic_light_loc.orientation);
        Tilemap toggled_tilemap = GameManager.track_manager.top_tilemap;
        tile_position = (Vector3Int)RouteManager.get_depart_tile_position(traffic_light_loc.orientation, city_tile_position);
        if (!exit_route_is_shown) // no route exists, dont show signal
        {
            change_traffic_signal_tile(Traffic_Light.None, traffic_light_loc);
            return;
        }
        HashSet<Vector3Int> seen_track_tile_set = new HashSet<Vector3Int>(); // keep track of tracks to discover loop routes
        // used by traffic lights to warn of incoming trains etc.
        final_orientation = traffic_light_loc.orientation;
        Traffic_Light signal = Traffic_Light.Green;
        int tile_count = 0;
        while (true)
        {
            tile_count += 1;
            this.orientation = final_orientation; // updating the orientation at every new tile
            if (seen_track_tile_set.Contains(tile_position))
            {
                //print("yellow warning for orientation " + orientation + " this route contains loop");
                signal = update_signal(signal, Traffic_Light.Yellow); // warning, this route contains a loop
                break;
            }
            seen_track_tile_set.Add(tile_position);
            GameObject vehicle_go = VehicleManager.vehicle_board[tile_position.x + 1, tile_position.y + 1]; // remove offset
            if (!tile_position.Equals(city_tile_position) && vehicle_go != null)
            {
                //print("RED warning for orientation " + orientation + " vehicle " + vehicle_go.name + " found at " + tile_position);
                signal = update_signal(signal, Traffic_Light.Red); // is train enroute? . If YES return RED todoed
            }
            PositionPair position_pair;
            Vector2 no_offset = new Vector2(0, 0);
            position_pair = RouteManager.get_destination(this, toggled_tilemap, no_offset); // set the final orientation and destination
            next_tilemap_position = position_pair.tile_dest_pos; // discard abs pos information
            tile_position = new Vector3Int(next_tilemap_position.x, next_tilemap_position.y, 0);
            if (is_end_of_track())
            {
                GameObject city_object = GameManager.city_manager.get_city(next_tilemap_position);
                if (city_object == null) // end of track
                {
                    //print("yellow warning for orientation " + orientation + " this route does not end at a city");
                    signal = update_signal(signal, Traffic_Light.Yellow);
                    break;
                }
                else // is a city, find station
                {
                    // return green or red depending on whether another train station is full todoed
                    // return city_object;
                    City city = city_object.GetComponent<City>();
                    RouteManager.Orientation flipped_final_orientation = TrackManager.flip_straight_orientation(final_orientation);
                    Station incoming_station = city.get_station_by_orientation(flipped_final_orientation);
                    bool station_available = incoming_station.is_station_track_available();
                    if (station_available)
                    {
                        //print("GREEN for orientation " + orientation + " stations available GOOD TO GO");
                        signal = update_signal(signal, Traffic_Light.Green);
                    }
                    else
                    {
                        //print("RED warning for orientation " + orientation + " the destination city has no available tracks");
                        signal = update_signal(signal, Traffic_Light.Red);
                    } // station track is not available
                    break;
                }
            }
        }
        change_traffic_signal_tile(signal, traffic_light_loc);
    }

    public void change_traffic_signal_tile(Traffic_Light traffic_signal, Traffic_Light_Loc traffic_light_loc)
    {
        if (traffic_signal == Traffic_Light.Green)
        {            
            traffic_light_loc.traffic_tilemap.SetTile(traffic_light_loc.tile_pos, green_light_tile);
        }
        else if (traffic_signal == Traffic_Light.Yellow)
        {
            traffic_light_loc.traffic_tilemap.SetTile(traffic_light_loc.tile_pos, yellow_light_tile);
        }
        else if (traffic_signal == Traffic_Light.Red)
        {
            traffic_light_loc.traffic_tilemap.SetTile(traffic_light_loc.tile_pos, red_light_tile);
        }
        else if (traffic_signal == Traffic_Light.None)
        {
            traffic_light_loc.traffic_tilemap.SetTile(traffic_light_loc.tile_pos, null);
        }
        else { throw new Exception(traffic_signal + " is not a valid traffic light"); }
        traffic_light_matrix[traffic_light_loc.tile_pos.x, traffic_light_loc.tile_pos.y] = traffic_signal;
    }

    public void update_all_traffic_signal(Vector3Int city_position)
    {
        set_signal_from_exit_route(city_position, north_light_loc);
        set_signal_from_exit_route(city_position, east_light_loc);
        set_signal_from_exit_route(city_position, west_light_loc);
        set_signal_from_exit_route(city_position, south_light_loc);
    }

    public IEnumerator change_traffic_signal_coroutine(Vector3Int city_position) // only fire when City is Active
    {
        while (change_traffic_signal_flag)
        {
            update_all_traffic_signal(city_position);
            yield return new WaitForSeconds(1); // every second update traffic signals
        }
    }
}
