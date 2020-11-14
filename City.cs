using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class City : BoardManager
{
    // track city control as a function of supplies, troops, artillery
    Vector3Int tilemap_position;
    public Tile bomb_boxcar_tile;
    public Tile supply_boxcar_tile;
    public Tile troop_boxcar_tile;

    public List<GameObject> train_list; // list of trains inside a city
    public GameObject[,] city_board; // contains location of vehicles within city
    public List<Station> station_list;

    public static Vector2Int west_end_1 = new Vector2Int(4, 2);
    public static Vector2Int west_end_2 = new Vector2Int(4, 3);
    public static Vector3Int west_start_1 = new Vector3Int(0, 2, 0);
    public static Vector3Int west_start_2 = new Vector3Int(0, 3, 0);

    public static Vector2Int north_end_2 = new Vector2Int(4, 5);
    public static Vector2Int north_end_1 = new Vector2Int(4, 6);
    public static Vector3Int north_start_2 = new Vector3Int(1, 9, 0);
    public static Vector3Int north_start_1 = new Vector3Int(2, 9, 0);

    public static Vector2Int east_end_2 = new Vector2Int(10, 5); //wrong
    public static Vector2Int east_end_1 = new Vector2Int(10, 6);
    public static Vector3Int east_start_2 = new Vector3Int(16, 7, 0);
    public static Vector3Int east_start_1 = new Vector3Int(16, 8, 0);

    public static Vector2Int south_end_1 = new Vector2Int(10, 1);
    public static Vector2Int south_end_2 = new Vector2Int(10, 0);
    public static Vector3Int south_start_1 = new Vector3Int(14, 0, 0);
    public static Vector3Int south_start_2 = new Vector3Int(15, 0, 0);

    static Station West_Station;
    static Station North_Station;
    static Station East_Station;
    static Station South_Station;

    public GameObject Turn_Table;
    public GameObject Turn_Table_Circle;
    public GameObject turn_table;
    GameObject turn_table_circle;

    public RouteManager.Orientation destination_orientation;

    public int prev_train_list_length = 0;

    private void Start()
    {
        base.Start();
        // must be a Gameobject for Start() Update() to run
        train_list = new List<GameObject>();
        city_board = new GameObject[board_width, board_height]; // zero out the negative tile coordinates
        turn_table = Instantiate(Turn_Table);
        turn_table.GetComponent<Turntable>().city = this;
        turn_table.GetComponent<SpriteRenderer>().enabled = false;

        turn_table_circle = Instantiate(Turn_Table_Circle);
        turn_table_circle.GetComponent<SpriteRenderer>().enabled = false;
        destination_orientation = RouteManager.Orientation.None;

        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();

        West_Station = new Station(west_start_1, west_start_2, RouteManager.Orientation.West);
        North_Station = new Station(north_start_1, north_start_2, RouteManager.Orientation.North);
        East_Station = new Station(east_start_1, east_start_2,RouteManager.Orientation.East);
        South_Station = new Station(south_start_1, south_start_2, RouteManager.Orientation.South);
    }

    private void Update()
    {
        enable_train_for_screen();
    }

    public void set_destination_track(RouteManager.Orientation orientation)
    {
        destination_orientation = orientation;
    }

    public Vector3Int get_location()
    {
        return tilemap_position;
    }

    public List<GameObject> get_train_list()
    {
        return train_list;
    }

    public void show_turntable(bool state)
    {
        turn_table.GetComponent<SpriteRenderer>().enabled = state;
        turn_table_circle.GetComponent<SpriteRenderer>().enabled = state;
    }

    public Vector2Int find_parking_spot(int y, int start_x, int end_x)
    {
        // traverse row until an empty parking spot is found and return its location
        for (int i=start_x; i<=end_x; i++)
        {
            if (gameobject_board[i, y] == null)
            {
                return new Vector2Int(i, y);
            }
        }
        return new Vector2Int(-1,-1); // no parking spot available
    }

    public Vector2Int get_parking_spot()
    {
        Vector2Int parking_spot = BoardManager.invalid_tile;
        int[,] parking_coord = { { 4, 0, 5 }, { 4, 11, 16 }, { 6, 11, 16 }, { 6, 0, 4 } }; // y, x start, x end
        for (int i = 0; i < parking_coord.Length; i++)
        {
            parking_spot = find_parking_spot(parking_coord[i,0],parking_coord[i,1],parking_coord[i,2]);
            if (!parking_spot.Equals(BoardManager.invalid_tile)) break;
        }
        return parking_spot;
    }

    public void add_boxcar_to_tilemap(GameObject boxcar_object)
    {
        // called when displaying a city
        Vector2Int parking_spot = get_parking_spot();
        gameobject_board[parking_spot.x, parking_spot.y] = boxcar_object;
    }

    public void remove_boxcar_from_inventory(Vector2Int tile_pos)
    {
        Tilemap shipyard_inventory = GameManager.Shipyard_Inventory.GetComponent<Tilemap>();
        shipyard_inventory.SetTile((Vector3Int)tile_pos, null);
        gameobject_board[tile_pos.x, tile_pos.y] = null;
    }

    public void display_boxcar(bool display)
    {
        Tilemap shipyard_inventory = GameManager.Shipyard_Inventory.GetComponent<Tilemap>();
        for (int i = 0; i < gameobject_board.GetLength(0); i++)
        {
            for (int j = 0; j < gameobject_board.GetLength(1); j++)
            {
                GameObject boxcar_object = gameobject_board[i, j];
                if (boxcar_object != null)
                {
                    Vector2Int tile_pos = new Vector2Int(i, j);
                    string boxcar_name = boxcar_object.name.Replace("(Clone)", "");
                    if (boxcar_name == "bomb boxcar") place_tile(tile_pos, boxcar_object, bomb_boxcar_tile, shipyard_inventory, display);
                    else if (boxcar_name == "supply boxcar") place_tile(tile_pos, boxcar_object, supply_boxcar_tile, shipyard_inventory, display);
                    else if (boxcar_name == "troop boxcar") place_tile(tile_pos, boxcar_object, troop_boxcar_tile, shipyard_inventory, display);
                    else
                    {
                        throw new Exception("not a valid boxcar to store");
                    }
                }
            }
        }
    }

    public Station_Track get_station_track(Vector2Int tile_pos)
    {
        Station_Track station_track = null;
        if (tile_pos.y < 4)
        {
            if (tile_pos.x < 7)
            {
                if (tile_pos.y == 2) return West_Station.outer_track;
                else { return West_Station.inner_track; }
            }
            else
            {
                if (tile_pos.y == 2 || tile_pos.x == 14) return South_Station.inner_track;
                else { return South_Station.outer_track; }
            }
        }
        else
        {
            if (tile_pos.x < 7)
            {
                if (tile_pos.y == 7 || tile_pos.x == 2) return North_Station.outer_track;
                else { return North_Station.inner_track; }
            }
            else
            {
                if (tile_pos.y==7) return East_Station.inner_track;
                else { return East_Station.outer_track; }
            }
        }
        return station_track;
    }

    public void delete_train(GameObject train_object)
    {
        remove_train_from_list(train_object, train_list);
        //TODO: repeat line below for when train exits game view. Remove from list and hide
        if (CityManager.Activated_City == gameObject) MovingObject.switch_sprite_renderer(train_object, false);
        if (GameManager.game_menu_state) MovingObject.switch_sprite_renderer(train_object, true);
        game_manager.train_list.Add(train_object);
    }

    public void delete_boxcar(GameObject boxcar_object)
    {   // delete boxcar after it has left the city (delayed from train)
        if (CityManager.Activated_City == gameObject) MovingObject.switch_sprite_renderer(boxcar_object, false);
        if (GameManager.game_menu_state) MovingObject.switch_sprite_renderer(boxcar_object, true);
    }

    public void add_boxcar(GameObject boxcar_object)
    {   // add boxcar after it has entered the city (delayed from train)
        if (CityManager.Activated_City == gameObject) MovingObject.switch_sprite_renderer(boxcar_object, true);
        if (GameManager.game_menu_state) MovingObject.switch_sprite_renderer(boxcar_object, false); 
    }

    public void add_train_to_list(GameObject train_object)
    {
        // add train to the city
        train_list.Add(train_object); 
        remove_train_from_list(train_object, game_manager.train_list); // remove train from the game manager list
        if (CityManager.Activated_City == gameObject) // city screen is on, containing the relvant vehicle
        {
            MovingObject.switch_sprite_renderer(train_object, true);
            train_object.GetComponent<Train>().turn_on_train(true);
        }
        if (GameManager.game_menu_state) // this is not causing boxcar to disappear prematurely
            MovingObject.switch_sprite_renderer(train_object, false);
    }


    public void turn_turntable(GameObject train_object, RouteManager.Orientation orientation, bool depart_for_turntable=false)
    {
        Turntable t = turn_table.GetComponent<Turntable>();
        StartCoroutine(t.turn_turntable(train_object, orientation, depart_for_turntable));
    }

    public Station_Track add_train_to_station(GameObject train_object, RouteManager.Orientation orientation)
    {
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                return South_Station.set_station_track(train_object);
            case RouteManager.Orientation.East:
                return West_Station.set_station_track(train_object);
            case RouteManager.Orientation.West:
                return East_Station.set_station_track(train_object);
            case RouteManager.Orientation.South:
                return North_Station.set_station_track(train_object);
            default:
                return null;
        }
    }

    public void remove_train_from_station(GameObject train_object)
    {
        South_Station.remove_train_from_station_track(train_object);
        West_Station.remove_train_from_station_track(train_object);
        East_Station.remove_train_from_station_track(train_object);
        North_Station.remove_train_from_station_track(train_object);
    }

    public void remove_train_from_list(GameObject train_object, List<GameObject> train_list)
    {
        // remove a train that has departed the city
        Train train = train_object.GetComponent<Train>();
        int id = train.get_id();
        int trains_removed = 0;
        for (int t = 0; t < train_list.Count; t++)
        {
            int train_id = train_list[t].GetComponent<Train>().get_id();
            if (train_id == id)
            {
                train_list.RemoveAt(t);
                trains_removed++;
            }
        }
    }

    public void set_location(Vector3Int position)
    {
        tilemap_position = position;
    }

    public void enable_train_for_screen()
    {
        //update renderer when the screen changes
        if (GameManager.city_menu_state != GameManager.prev_city_menu_state) // change game view
        {
            // if city view open, only update if city matches activated city. If city closed, hide trains 
            if (CityManager.Activated_City==gameObject)
            {
                foreach (GameObject train in train_list) // hide or show trains depending on whether I'm in a game view
                {
                    train.GetComponent<Train>().turn_on_train(GameManager.city_menu_state);
                }
                GameManager.prev_city_menu_state = GameManager.city_menu_state;
            }
        }
    }

}
