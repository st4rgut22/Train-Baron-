using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class City : Structure
{
    public Tile wealthy_room_tile;
    public Tile poor_room_tile;
    public Tile diner_room_tile;
    public Tile factory_room_tile;
    public Tile entrance_room_tile;
    public Tile restaurant_room_tile;

    public Tile undeveloped_tile;

    public List<Building> city_building_list;
    public Room[,] city_room_matrix;
    public Tile city_tile;
    public int building_id;
    public GameObject city_icon; // icon viewable in game view
    public string city_type;
    public int city_id;
    public Vector2Int first_structure_location;

    // track city control as a function of supplies, troops, artillery
    Vector3Int tilemap_position;
    public Tile food_boxcar_tile;
    public Tile work_boxcar_tile;
    public Tile vacation_boxcar_tile;
    public Tile home_boxcar_tile;

    public List<GameObject> train_list; // list of trains inside a city
    public GameObject[,] city_board; // contains location of vehicles within city
    public List<Station> station_list;

    public GameObject Turn_Table;
    public GameObject Turn_Table_Circle;
    public GameObject turn_table;
    GameObject turn_table_circle;

    public GameObject Business;
    public GameObject Residential;
    public GameObject Wealthy;
    public GameObject Poor;
    public GameObject Restaurant;
    public GameObject Factory;
    public GameObject Pond;
    public GameObject Entrance;
    public GameObject Diner;

    public Station West_Station;
    public Station North_Station;
    public Station East_Station;
    public Station South_Station;

    public GameObject city_tilemap_go;
    public Tilemap city_tilemap;

    public RouteManager.Orientation destination_orientation;

    public int prev_train_list_length = 0;

    public static Dictionary<string, GameObject> building_map;
    List<string> initial_building_lot_list;
    public GameObject BuildingLot;

    private void Awake()
    {
        initial_building_lot_list = new List<string>() { "Building Lot South Outer", "Building Lot South Inner", "Building Lot North Outer", "Building Lot North Inner", "Building Lot West", "Building Lot East" };
        base.Awake();
        West_Station = new Station(CityManager.west_start_outer, CityManager.west_start_inner, RouteManager.Orientation.West, RouteManager.Orientation.South, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap2, RouteManager.shipyard_track_tilemap);
        North_Station = new Station(CityManager.north_start_outer, CityManager.north_start_inner, RouteManager.Orientation.North, RouteManager.Orientation.East, RouteManager.Orientation.South, RouteManager.shipyard_track_tilemap, RouteManager.shipyard_track_tilemap2);
        East_Station = new Station(CityManager.east_start_outer, CityManager.east_start_inner, RouteManager.Orientation.East, RouteManager.Orientation.South, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap2, RouteManager.shipyard_track_tilemap);
        South_Station = new Station(CityManager.south_start_outer, CityManager.south_start_inner, RouteManager.Orientation.South, RouteManager.Orientation.West, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap, RouteManager.shipyard_track_tilemap2);
        city_tilemap_go = GameManager.city_tilemap_go;
        city_tilemap = city_tilemap_go.GetComponent<Tilemap>();
        city_room_matrix = new Room[board_width, board_height];
        city_board = new GameObject[board_width, board_height]; // zero out the negative tile coordinates
        GameObject north_outer_bl = Instantiate(BuildingLot);
        GameObject north_inner_bl = Instantiate(BuildingLot);
        GameObject east_bl = Instantiate(BuildingLot);
        GameObject west_bl = Instantiate(BuildingLot);
        GameObject south_inner_bl = Instantiate(BuildingLot);
        GameObject south_outer_bl = Instantiate(BuildingLot);
        north_outer_bl.GetComponent<BuildingLot>().init_building_lot
            (
                "Building Lot North Outer",
                new Vector2Int(0, 7),
                4,
                RouteManager.Orientation.North,
                new List<Station_Track> { North_Station.outer_track },
                null,
                new Door_Prop(right_door_top_right, -90f, 90f)
            );
        north_inner_bl.GetComponent<BuildingLot>().init_building_lot
            (
                "Building Lot North Inner",
                new Vector2Int(3, 9),
                3,
                RouteManager.Orientation.East,
                new List<Station_Track> { North_Station.inner_track },
                new Door_Prop(left_door_top_left, 90f, 0f),
                null
            );
        east_bl.GetComponent<BuildingLot>().init_building_lot
            (
                "Building Lot East",
                new Vector2Int(11, 8),
                6,
                RouteManager.Orientation.East,
                new List<Station_Track> { East_Station.inner_track, East_Station.outer_track },
                new Door_Prop(left_door_top_left, 90f, 180f),
                new Door_Prop(right_door_top_right, -90f, 0f)
            );
        west_bl.GetComponent<BuildingLot>().init_building_lot
            (
                "Building Lot West",
                new Vector2Int(0, 2),
                6,
                RouteManager.Orientation.East,
                new List<Station_Track> { West_Station.inner_track, West_Station.outer_track },
                new Door_Prop(right_door_top_right, -90f, 0f),
                new Door_Prop(left_door_top_left, 90f, 180f)
            );
        south_inner_bl.GetComponent<BuildingLot>().init_building_lot
            (
                "Building Lot South Inner",
                new Vector2Int(10, 1),
                4,
                RouteManager.Orientation.East,
                new List<Station_Track> { South_Station.inner_track },
                new Door_Prop(left_door_top_left, 90f, 180f),
                null
            );
        south_outer_bl.GetComponent<BuildingLot>().init_building_lot
            (
                "Building Lot South Outer",
                new Vector2Int(16, 1),
                3,
                RouteManager.Orientation.North,
                new List<Station_Track> { South_Station.outer_track },
                null,
                new Door_Prop(right_door_top_right, -90f, 270f)
            );
        building_map = new Dictionary<string, GameObject>()
        {
            {"Building Lot North Outer", north_outer_bl },
            {"Building Lot North Inner", north_inner_bl },
            {"Building Lot East", east_bl },
            {"Building Lot West", west_bl },
            {"Building Lot South Inner", south_inner_bl },
            {"Building Lot South Outer", south_outer_bl }
        };
    }

    private void Start()
    {
        base.Start();
        initialize_city_tilemap(); // with the first structure location
        // must be a Gameobject for Start() Update() to run
        train_list = new List<GameObject>();
        turn_table = Instantiate(Turn_Table);
        turn_table.GetComponent<Turntable>().city = this;
        turn_table.GetComponent<SpriteRenderer>().enabled = false;

        turn_table_circle = Instantiate(Turn_Table_Circle);
        turn_table_circle.GetComponent<SpriteRenderer>().enabled = false;
        destination_orientation = RouteManager.Orientation.None;

        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        building_id = 1;
    }

    public Station_Track get_station_track(Vector2Int tile_pos)
    {
        if (tile_pos.y <= 4)
        {
            if (tile_pos.x < 7)
            {
                if (tile_pos.y == 1 || tile_pos.y == 2) return West_Station.outer_track;
                else { return West_Station.inner_track; }
            }
            else
            {
                if (tile_pos.y == 3 || tile_pos.x == 15) return South_Station.outer_track;
                else { return South_Station.inner_track; }
            }
        }
        else
        {
            if (tile_pos.x < 7)
            {
                if (tile_pos.y == 7 || tile_pos.x == 1) return North_Station.outer_track;
                else { return North_Station.inner_track; }
            }
            else
            {
                if (tile_pos.y == 7) return East_Station.inner_track;
                else { return East_Station.outer_track; }
            }
        }
    }

    private void Update()
    {
        //enable_train_for_screen(); causes lag
    }

    public bool is_selected_room_occupied(Vector2Int clicked_room_position, string lot_name)
    {
        Room room = city_room_matrix[clicked_room_position.x, clicked_room_position.y];
        if (room != null)
        {
            return room.occupied;
        }
        return false;
    }

    public Building get_city_building(Vector2Int building_id)
    {
        foreach (Building bldg in city_building_list)
        {
            if (building_id.Equals(bldg.offset_position))
            {
                return bldg;
            }
        }
        throw new Exception("Building should exist but doesn't");
    }

    public void show_all_building_occupants(bool is_city_shown)
    {
        //iterate over all buildings, within each building iterate over rooms and deactivate as necessary
        foreach (Building building in city_building_list)
        {
            building.reveal_building_rooms(is_city_shown);
        }
    }

    public Building get_city_building()
    {
        GameObject building_object;
        if (city_type == "Entrance")
        {
            city_tile = entrance_room_tile;
            building_object = Instantiate(Entrance);
        }
        else if (city_type == "Poor")
        {
            city_tile = poor_room_tile;
            building_object = Instantiate(Poor);
        }
        else if (city_type == "Wealthy")
        {
            city_tile = wealthy_room_tile;
            building_object = Instantiate(Wealthy);
        }
        else if (city_type == "Factory")
        {
            city_tile = factory_room_tile;
            building_object = Instantiate(Factory);
        }
        else if (city_type == "Diner")
        {
            city_tile = diner_room_tile;
            building_object = Instantiate(Diner);
        }
        else if (city_type == "Restaurant")
        {
            city_tile = restaurant_room_tile;
            building_object = Instantiate(Restaurant);
        }
        else
        {
            throw new Exception("not a valid tile");
        }
        return building_object.GetComponent<Building>(); 
    }

    public void expand_building(Building building, Vector2Int selected_tile)
    {
        int expansion_count = 0;
        if (selected_tile.x == building.offset_position.x && selected_tile.y > building.last_room_position.y)
        {
            expansion_count = selected_tile.y - building.last_room_position.y;
        }
        else if (selected_tile.y == building.offset_position.y && selected_tile.x > building.last_room_position.x)
        {
            expansion_count = selected_tile.x - building.last_room_position.x;
        }
        else
        {
            print("not a valid location for expansion");
        }
        expansion_count = Math.Min(expansion_count, building.max_capacity - building.current_capacity);
        print("expansion count is " + expansion_count);
        for (int e = 0; e < expansion_count; e++)
        {
            Room room = building.spawn_room();
            set_city_tile((Vector3Int)room.tile_position);
            GameManager.undeveloped_land.GetComponent<Tilemap>().SetTile((Vector3Int)room.tile_position, null);
            room.display_contents(true);
        }
    }

    public void unload_train(GameObject boxcar_go, Vector2Int room_position)
    {
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        boxcar.passenger_go.GetComponent<Person>().offset_map = RouteManager.offset_route_map[boxcar.station_track.start_location];
        boxcar.passenger_go.GetComponent<Person>().is_enter_home = true;
        Room room = city_room_matrix[room_position.x, room_position.y];
        string track_name = RouteManager.shipyard_track_tilemap.GetTile(boxcar.tile_position).name;
        print("unloading train from room  position " + room_position + " to " + boxcar.tile_position);
        RouteManager.Orientation exit_orientation = CityManager.station_track_unloading_map[boxcar.station_track.start_location][track_name];
        StartCoroutine(GameObject.Find("PersonRouteManager").GetComponent<PersonRouteManager>().unload_train(boxcar, room, exit_orientation));
    }

    public void board_train(GameObject boxcar_go, Vector2Int room_position)
    {
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        print("boarding train from room  position " + room_position + " to " + boxcar.tile_position);

        Room room = city_room_matrix[room_position.x, room_position.y];
        GameObject occupant_go = room.person_go; //todo: laster move the occupant to the room (first checkpoint). 
        Person occupant = occupant_go.GetComponent<Person>();
        boxcar.is_occupied = true;
        boxcar.passenger_go = occupant_go;
        occupant.is_board_boxcar = true;
        occupant.boxcar_go = boxcar_go;
        occupant.station_track = boxcar.station_track;
        occupant.offset_map = RouteManager.offset_route_map[boxcar.station_track.start_location];
        StartCoroutine(GameObject.Find("PersonRouteManager").GetComponent<PersonRouteManager>().board_train(boxcar, room, occupant_go, boxcar.tile_position));
    }

    public void set_all_room_sprites()
    {
        for (int i = 0; i < city_room_matrix.GetLength(0); i++)
        {
            for (int j = 0; j < city_room_matrix.GetLength(1); j++)
            {
                Vector3Int tile_position = new Vector3Int(i, j, 0);
                Room room = city_room_matrix[i, j];
                if (room != null)
                    city_tilemap.SetTile(tile_position, city_tile);
                else
                {
                    city_tilemap.SetTile(tile_position, null);
                }
            }
        }
    }

    public void set_city_tile(Vector3Int tile_position)
    {
        city_tilemap.SetTile(tile_position, city_tile);
    }

    public void initialize_city_tilemap()
    {
        foreach (string initial_building_lot in initial_building_lot_list)
        {
            GameObject first_building_lot_go = building_map[initial_building_lot];
            BuildingLot initial_bl = first_building_lot_go.GetComponent<BuildingLot>();
            spawn_building(initial_bl);
            show_all_building_occupants(false); // hide all doors when city is first created
            building_id += 1;
        }
    }

    public void spawn_building(BuildingLot building_lot)
    {
        Building building = get_city_building();
        building_lot.set_building(building);
        building.building_id = building_id;
        building.building_type = city_type;
        building.building_orientation = building_lot.orientation;
        building.offset_position = building_lot.origin_tile;
        building.max_capacity = building_lot.length;
        building.person_grid = new GameObject[building_lot.length];
        building.city = this;
        building.building_lot = building_lot;
        city_building_list.Add(building);
    }

    public void set_destination_track(RouteManager.Orientation orientation)
    {
        destination_orientation = orientation;
    }

    public Vector3Int get_location()
    {
        return tilemap_position;
    }

    public void show_turntable(bool state)
    {
        turn_table.GetComponent<SpriteRenderer>().enabled = state;
        turn_table_circle.GetComponent<SpriteRenderer>().enabled = state;
    }

    public bool is_parking_spot_available(Vector2Int tile_pos)
    {
        // first check if parking spot is valid, then check if is available
        bool is_valid = false;
        for (int i = 0; i < TrackManager.parking_coord.GetLength(0); i++)
        {
            int y = TrackManager.parking_coord[i, 0];
            int start_x = TrackManager.parking_coord[i, 1];
            int end_x = TrackManager.parking_coord[i, 2];
            if (y == tile_pos.y && tile_pos.x >= start_x && tile_pos.x <= end_x) is_valid = true;
        }
        if (is_valid && gameobject_board[tile_pos.x, tile_pos.y] == null) return true;
        else { return false; }
    }

    public Vector2Int find_parking_spot(int y, int start_x, int end_x)
    {
        // traverse row until an empty parking spot is found and return its location
        for (int i = start_x; i <= end_x; i++)
        {
            if (gameobject_board[i, y] == null)
            {
                return new Vector2Int(i, y);
            }
        }
        return new Vector2Int(-1, -1); // no parking spot available
    }

    public Vector2Int get_parking_spot()
    {
        Vector2Int parking_spot = BoardManager.invalid_tile;
        for (int i = 0; i < TrackManager.parking_coord.GetLength(0); i++)
        {
            parking_spot = find_parking_spot(TrackManager.parking_coord[i, 0], TrackManager.parking_coord[i, 1], TrackManager.parking_coord[i, 2]);
            if (!parking_spot.Equals(BoardManager.invalid_tile)) break;
        }
        return parking_spot;
    }

    public void show_all_undeveloped_plots(bool is_hide)
    {
        foreach (Building bldg in city_building_list)
        {
            foreach (GameObject room_go in bldg.roomba)
            {             
                if (room_go != null) // room exists
                {
                    Room room = room_go.GetComponent<Room>();
                    if (is_hide) GameManager.undeveloped_land.GetComponent<Tilemap>().SetTile((Vector3Int)room.tile_position, undeveloped_tile);
                    else { GameManager.undeveloped_land.GetComponent<Tilemap>().SetTile((Vector3Int)room.tile_position, null); }
                }
            }
        }
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

    public void place_boxcar_tile(GameObject boxcar_object, Vector2Int tile_pos)
    {
        Tilemap shipyard_inventory = GameManager.Shipyard_Inventory.GetComponent<Tilemap>();
        string boxcar_name = boxcar_object.name.Replace("(Clone)", "");
        if (boxcar_name == "food boxcar") place_tile(tile_pos, boxcar_object, food_boxcar_tile, shipyard_inventory);
        else if (boxcar_name == "work boxcar") place_tile(tile_pos, boxcar_object, work_boxcar_tile, shipyard_inventory);
        else if (boxcar_name == "vacation boxcar") place_tile(tile_pos, boxcar_object, vacation_boxcar_tile, shipyard_inventory);
        else if (boxcar_name == "home boxcar") place_tile(tile_pos, boxcar_object, home_boxcar_tile, shipyard_inventory);
        else
        {
            throw new Exception("not a valid boxcar to store");
        }
    }

    public void display_boxcar()
    {
        for (int i = 0; i < gameobject_board.GetLength(0); i++)
        {
            for (int j = 0; j < gameobject_board.GetLength(1); j++)
            {
                GameObject boxcar_object = gameobject_board[i, j];
                if (boxcar_object != null)
                {
                    Vector2Int tile_pos = new Vector2Int(i, j);
                    place_boxcar_tile(boxcar_object, tile_pos);
                }
            }
        }
    }

    public bool is_train_on_track(Vector2Int tile_pos, bool get_outer)
    {
        Station_Track station_track;
        if (tile_pos.y <= 4)
        {
            if (tile_pos.x < 7)
            {
                if (get_outer) station_track = West_Station.outer_track;
                else { station_track = West_Station.inner_track; }
            }
            else
            {
                if (get_outer) station_track = South_Station.outer_track;
                else { station_track = South_Station.inner_track; }
            }
        }
        else
        {
            if (tile_pos.x < 7)
            {
                if (get_outer) station_track = North_Station.outer_track;
                else { station_track = North_Station.inner_track; }
            }
            else
            {
                if (get_outer) station_track = East_Station.inner_track;
                else { station_track = East_Station.outer_track; }
            }
        }
        if (station_track.train != null) return true;
        return false;
    }

    public void delete_train(GameObject train_object)
    {
        remove_train_from_list(train_object, train_list);
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
            train_object.GetComponent<Train>().turn_on_train(true);
        }
        if (GameManager.game_menu_state) // this is not causing boxcar to disappear prematurely
            MovingObject.switch_sprite_renderer(train_object, false);
    }


    public void turn_turntable(GameObject train_object, RouteManager.Orientation orientation, bool depart_for_turntable = false)
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
            if (CityManager.Activated_City == gameObject)
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