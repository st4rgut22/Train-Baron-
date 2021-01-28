using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class City : Structure
{
    public Tile wealthy_room_tile;
    public Tile poor_room_tile;
    public Tile diner_room_tile;
    public Tile factory_room_tile;
    public Tile entrance_room_tile;
    public Tile restaurant_room_tile;
    public Tile business_room_tile;

    public Tile undeveloped_tile;

    public List<Building> city_building_list;
    public Room[,] city_room_matrix;
    public Tile city_tile;
    public int building_id;
    public GameObject city_icon; // icon viewable in game view
    public string city_type;
    public int city_id;
    public Vector2Int first_structure_location;
    public int total_people;

    // track city control as a function of supplies, troops, artillery
    public Vector3Int tilemap_position;
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
    public GameObject Mansion;
    public GameObject Apartment;
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

    
    public int prev_train_list_length = 0;

    public int reputation = 0;
    public int max_reputation = 100;
    public int min_reputation = 0;
    public int last_checked_reputation;
    public const int reputation_per_lot = 5;
    public int unapplied_reputation_count;

    public int total_review_count;
    public int total_star;

    public static Dictionary<string, GameObject> building_map;
    List<string> initial_building_lot_list;
    public GameObject BuildingLot;
    public GameObject TrafficLightManager;
    public GameObject Traffic_Light_Manager_Instance;
    public TrafficLightManager traffic_manager;
    public Inventory_Item[,] inventory_item_board;

    //public List<GameObject> person_in_transit; //people unloading from trains

    public class Inventory_Item
    {
        public string name;
        public Inventory_Item(string name)
        {
            this.name = name;
        }
    }

    private void Awake()
    {
        base.Awake();
        Traffic_Light_Manager_Instance = Instantiate(TrafficLightManager);
        traffic_manager = Traffic_Light_Manager_Instance.GetComponent<TrafficLightManager>();
        Traffic_Light_Manager_Instance.transform.parent = gameObject.transform; // only activate when this city is activated
        unapplied_reputation_count = 0;
        total_review_count = 0;
        last_checked_reputation = reputation;
        initial_building_lot_list = new List<string>() { "Building Lot West", "Building Lot North Outer", "Building Lot North Inner", "Building Lot South Outer", "Building Lot South Inner", "Building Lot East"};
        West_Station = new Station(CityManager.west_start_outer, CityManager.west_start_inner, RouteManager.Orientation.West, RouteManager.Orientation.South, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap2, RouteManager.shipyard_track_tilemap);
        North_Station = new Station(CityManager.north_start_outer, CityManager.north_start_inner, RouteManager.Orientation.North, RouteManager.Orientation.East, RouteManager.Orientation.South, RouteManager.shipyard_track_tilemap, RouteManager.shipyard_track_tilemap2);
        East_Station = new Station(CityManager.east_start_outer, CityManager.east_start_inner, RouteManager.Orientation.East, RouteManager.Orientation.South, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap2, RouteManager.shipyard_track_tilemap);
        South_Station = new Station(CityManager.south_start_outer, CityManager.south_start_inner, RouteManager.Orientation.South, RouteManager.Orientation.West, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap, RouteManager.shipyard_track_tilemap2);
        city_tilemap_go = GameManager.city_tilemap_go;
        city_tilemap = city_tilemap_go.GetComponent<Tilemap>();
        city_room_matrix = new Room[board_width, board_height];
        city_board = new GameObject[board_width, board_height]; // zero out the negative tile coordinates
        inventory_item_board = new Inventory_Item[board_width, board_height];
        GameObject north_outer_bl = Instantiate(BuildingLot);
        GameObject north_inner_bl = Instantiate(BuildingLot);
        GameObject east_bl = Instantiate(BuildingLot);
        GameObject west_bl = Instantiate(BuildingLot);
        GameObject south_inner_bl = Instantiate(BuildingLot);
        GameObject south_outer_bl = Instantiate(BuildingLot);
        start_reputation = PersonManager.reputation; // 0

        north_outer_bl.GetComponent<BuildingLot>().init_building_lot
            (
                "Building Lot North Outer",
                new Vector2Int(0, 7),
                4,
                RouteManager.Orientation.North,
                RouteManager.Orientation.South,
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
                RouteManager.Orientation.South,
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
                RouteManager.Orientation.West,
                new List<Station_Track> { East_Station.inner_track, East_Station.outer_track },
                new Door_Prop(left_door_top_left, 90f, 180f),
                new Door_Prop(right_door_top_right, -90f, 0f)
            ); ;
        west_bl.GetComponent<BuildingLot>().init_building_lot
            (
                "Building Lot West",
                new Vector2Int(0, 2),
                6,
                RouteManager.Orientation.East,
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
                RouteManager.Orientation.North,
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
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        building_id = 1;
    }

    public void change_traffic_signal(bool is_signal_on)
    {
        traffic_manager.change_traffic_signal_flag = is_signal_on;
        if (is_signal_on) StartCoroutine(traffic_manager.change_traffic_signal_coroutine(tilemap_position));
    }

    public Texture get_star_image_from_reputation()
    {
        PersonManager pm = GameManager.person_manager.GetComponent<PersonManager>();
        if (total_review_count == 0) return pm.zero_star_texture;
        int texture_id = (int)(total_star / total_review_count);
        print("total stars is " + total_star + " total reviews is " + total_review_count);
        return pm.get_star_texture(texture_id);
    }

    public void populate_entrance()
    {
        //use delta reputation to populate as many rooms as possible
        int people_to_add = PersonManager.reputation - start_reputation;
        int total_vacancy_count = get_total_occupant_count();
        if (total_vacancy_count < people_to_add) people_to_add = total_vacancy_count;
        start_reputation = PersonManager.reputation;
        print("people to add " + people_to_add);
        for (int i = 0; i < people_to_add; i++)
        {
            foreach (Building bldg in city_building_list)
            {
                foreach (GameObject room_go in bldg.roomba)
                {
                    if (room_go != null )
                    {
                        Room room = room_go.GetComponent<Room>();
                        if (!room.has_person)
                        {
                            Person person = room.spawn_person();
                            CityManager.increment_total_people();
                            total_people += 1;
                            if (CityManager.Activated_City_Component == this) person.initialize_egghead(true, true); // if entrance is activated
                            else { person.initialize_egghead(false, false); }
                            break;
                        }
                    }
                }
            }
        }
    }

    public int get_total_occupant_count()
    {
        int city_occupant_count = 0;
        foreach (Building bldg in city_building_list)
        {
            city_occupant_count += bldg.get_vacancy_count();
        }
        return city_occupant_count;
    }

    public void change_star_count(int star_count)
    {
        total_star += star_count;
    }

    public void change_reputation(int reputation_change)
    {
        total_review_count += 1;
        reputation += reputation_change;
        reputation = Mathf.Min(reputation, max_reputation);
        reputation = Mathf.Max(reputation, min_reputation);
        print("reputation of city is " + reputation);
    }

    public Station get_station_by_orientation(RouteManager.Orientation orientation)
    {
        switch (orientation)
        {
            case RouteManager.Orientation.East:
                return East_Station;
            case RouteManager.Orientation.West:
                return West_Station;
            case RouteManager.Orientation.North:
                return North_Station;
            case RouteManager.Orientation.South:
                return South_Station;
        }
        throw new Exception("not a valid orientation");
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
        
    }

    public RouteManager.Orientation get_orientation_of_open_track()
    {
        foreach (Building bldg in city_building_list)
        {
            foreach (GameObject room_go in bldg.roomba)
            {
                if (room_go != null)
                {
                    Room room = room_go.GetComponent<Room>();
                    if (room.person_go_instance != null)
                    {
                        return bldg.building_lot.train_orientation;
                    }
                }
            }
        }
        return RouteManager.Orientation.None; // all the rooms are full
    }

    public Room get_selected_room(Vector2Int clicked_room_position)
    {
        return city_room_matrix[clicked_room_position.x, clicked_room_position.y];
    }

    public bool is_selected_room_occupied(Room room)
    {
        if (room != null)
        {
            return room.booked;
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
        else if (city_type == "Apartment")
        {
            city_tile = poor_room_tile;
            building_object = Instantiate(Apartment);
        }
        else if (city_type == "Mansion")
        {
            city_tile = wealthy_room_tile;
            building_object = Instantiate(Mansion);
        }
        else if (city_type == "business")
        {
            city_tile = business_room_tile;
            building_object = Instantiate(Business);
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
            throw new Exception("city type " + city_type + " not a valid tile");
        }
        return building_object.GetComponent<Building>(); 
    }

    public void unload_train(GameObject boxcar_go, Vector2Int room_position)
    {
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        Person person = boxcar.passenger_go.GetComponent<Person>();
        person.offset_map = RouteManager.offset_route_map[boxcar.station_track.start_location];
        person.is_enter_home = true;
        Room room = city_room_matrix[room_position.x, room_position.y];
        room.booked = true; // set true even tho person hasnt arrived to lock in this room and lock out others
        room.person_go_instance = boxcar.passenger_go;
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
        GameObject occupant_go = room.person_go_instance; //todo: laster move the occupant to the room (first checkpoint). 
        Person occupant = occupant_go.GetComponent<Person>();
        occupant.is_selected = false;
        boxcar.is_occupied = true; // prevent another pserson from boarding the same boxcar
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
        if (is_valid && inventory_item_board[tile_pos.x, tile_pos.y] == null) return true;
        else { return false; }
    }

    public int remove_lot(int affected_lot)
    {
        int remove_lot_count = 0;
        foreach (Building bldg in city_building_list)
        {
            while (bldg.current_capacity > 0 && affected_lot < remove_lot_count)
            {
                bldg.remove_last_room();
                remove_lot_count += 1;
            }
        }
        return affected_lot - remove_lot_count;
    }

    public int add_lot(int affected_lot)
    {
        int added_lot_count = 0;
        foreach (Building bldg in city_building_list)
        {
            while(bldg.current_capacity < bldg.max_capacity && added_lot_count < affected_lot)
            {
                bldg.spawn_room();
                added_lot_count += 1;
            }
        }
        return affected_lot - added_lot_count; // rollover lots to be applied to reputation since bldg capacity is filled
    }

    public void apply_reputation()
    {
        // change number of lots 
        int delta_reputation = reputation + unapplied_reputation_count - last_checked_reputation;
        print("delta reputation is " + delta_reputation);
        int lot_affected = Math.Abs(delta_reputation); //TESTING UNDO!! TODOED / reputation_per_lot;
        int leftover_reputation = delta_reputation % reputation_per_lot;
        int rollover_reputation = 0; // rollover lots are how many lots over the max capacity should be affected. Applied when more lots are available
        if (delta_reputation < 0)
        {
            int underwater_rollover_lot = remove_lot(lot_affected);
            rollover_reputation = underwater_rollover_lot * -reputation_per_lot;
            print("underwater lots is " + underwater_rollover_lot);
        }
        else if (delta_reputation > 0){
            int excess_rollover_lot = add_lot(lot_affected);
            rollover_reputation = excess_rollover_lot * reputation_per_lot;
            print("excess lots is " + excess_rollover_lot);
        }
        if (this == CityManager.Activated_City_Component)
        {
            show_all_undeveloped_plots(false);
            set_all_room_sprites();
        }
        reputation = Mathf.Max(0, reputation);
        reputation = Mathf.Min(100, reputation);
        print("reputation is now " + reputation);
        unapplied_reputation_count = leftover_reputation + rollover_reputation;
        if (reputation != last_checked_reputation)
        {
            GameManager.reputation_text_go.GetComponent<Text>().text = "Reputation: " + reputation;
        }
        last_checked_reputation = reputation;
        GameManager.star_review_image_go.GetComponent<RawImage>().texture = get_star_image_from_reputation();
    }

    public Vector2Int find_parking_spot(int y, int start_x, int end_x)
    {
        // traverse row until an empty parking spot is found and return its location
        for (int i = start_x; i <= end_x; i++)
        {
            if (inventory_item_board[i, y] == null)
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

    public void add_boxcar_to_tilemap(string boxcar_type)
    {
        // called when displaying a city
        Inventory_Item boxcar_item = new Inventory_Item(boxcar_type);
        Vector2Int parking_spot = get_parking_spot();
        inventory_item_board[parking_spot.x, parking_spot.y] = boxcar_item;
    }

    public void remove_boxcar_from_inventory(Vector2Int tile_pos)
    {
        Tilemap shipyard_inventory = GameManager.Shipyard_Inventory.GetComponent<Tilemap>();
        shipyard_inventory.SetTile((Vector3Int)tile_pos, null);
        inventory_item_board[tile_pos.x, tile_pos.y] = null;
    }

    public void place_boxcar_tile(string boxcar_name, Vector3Int tile_pos)
    {
        Tilemap shipyard_inventory = GameManager.Shipyard_Inventory.GetComponent<Tilemap>();
        if (boxcar_name == "food boxcar") shipyard_inventory.SetTile(tile_pos, food_boxcar_tile); 
        else if (boxcar_name == "work boxcar") shipyard_inventory.SetTile(tile_pos, work_boxcar_tile); 
        else if (boxcar_name == "vacation boxcar") shipyard_inventory.SetTile(tile_pos, vacation_boxcar_tile); 
        else if (boxcar_name == "home boxcar") shipyard_inventory.SetTile(tile_pos, home_boxcar_tile);
        else
        {
            throw new Exception("not a valid boxcar to store: " + boxcar_name);
        }
    }

    public void display_boxcar()
    {
        for (int i = 0; i < inventory_item_board.GetLength(0); i++)
        {
            for (int j = 0; j < inventory_item_board.GetLength(1); j++)
            {
                Inventory_Item boxcar_object = inventory_item_board[i, j];
                if (boxcar_object != null)
                {
                    Vector2Int tile_pos = new Vector2Int(i, j);
                    place_boxcar_tile(boxcar_object.name, (Vector3Int)tile_pos);
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
        GameManager.train_list.Add(train_object);
    }

    public void add_train_to_list(GameObject train_object)
    {
        // add train to the city
        train_list.Add(train_object);
        remove_train_from_list(train_object, GameManager.train_list); // remove train from the game manager list
        if (CityManager.Activated_City == gameObject) // city screen is on, containing the relvant vehicle
        {
            train_object.GetComponent<Train>().turn_on_train(true);
        }
        if (GameManager.game_menu_state) // in city AND game menu state
            StartCoroutine(train_object.GetComponent<Train>().switch_on_vehicle(false));
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