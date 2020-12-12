using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class City : BoardManager
{
    public Tile business_tile;
    public Tile residential_tile;
    public Tile hospital_tile;
    public Tile lab_tile;
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
    public Tile bomb_boxcar_tile;
    public Tile supply_boxcar_tile;
    public Tile troop_boxcar_tile;

    public List<GameObject> train_list; // list of trains inside a city
    public GameObject[,] city_board; // contains location of vehicles within city
    public List<Station> station_list;

    //outside track comes first
    //TODO: change these 1s or 2s to outers or inners
    public static Vector2Int west_end_outer = new Vector2Int(6, 2);
    public static Vector2Int west_end_inner = new Vector2Int(6, 3);
    public static Vector3Int west_start_outer = new Vector3Int(-1, 1, 0);
    public static Vector3Int west_start_inner = new Vector3Int(-1, 3, 0);

    public static Vector2Int north_end_inner = new Vector2Int(6, 7);
    public static Vector2Int north_end_outer = new Vector2Int(6, 8);
    public static Vector3Int north_start_outer = new Vector3Int(1, 10, 0);
    public static Vector3Int north_start_inner = new Vector3Int(2, 10, 0);

    public static Vector2Int east_end_inner = new Vector2Int(10, 7); //wrong
    public static Vector2Int east_end_outer = new Vector2Int(10, 8);
    public static Vector3Int east_start_inner = new Vector3Int(17, 7, 0);
    public static Vector3Int east_start_outer = new Vector3Int(17, 9, 0);

    public static Vector2Int south_end_outer = new Vector2Int(10, 2);
    public static Vector2Int south_end_inner = new Vector2Int(10, 1);
    public static Vector3Int south_start_outer = new Vector3Int(15, -1, 0);
    public static Vector3Int south_start_inner = new Vector3Int(14, -1, 0);

    Station West_Station;
    Station North_Station;
    Station East_Station;
    Station South_Station;

    public GameObject Turn_Table;
    public GameObject Turn_Table_Circle;
    public GameObject turn_table;
    GameObject turn_table_circle;

    public GameObject Business;
    public GameObject Residential;
    public GameObject Hospital;
    public GameObject Lab;

    public GameObject city_tilemap_go;
    public Tilemap city_tilemap;

    public static Dictionary<Vector3Int, RouteManager.Orientation> station_track_boarding_map;
    public static Dictionary<Vector3Int, Dictionary<string, RouteManager.Orientation>> station_track_unloading_map;
    public static Dictionary<Vector3Int,RouteManager.Orientation[]> station_track_curve_map; // array index 0 is original orientation, 1 is final orientation
    public static Dictionary<string, RouteManager.Orientation> initial_person_face_map;

    public RouteManager.Orientation destination_orientation;

    public int prev_train_list_length = 0;

    public Dictionary<string, Building_Lot> building_map;
    string initial_building_lot;
    List<string> initial_building_lot_list;

    private void Awake()
    {
        //initial_building_lot_list = new List<string>() { "Building Lot South Outer", "Building Lot South Inner", "Building Lot North Outer", "Building Lot North Inner", "Building Lot West", "Building Lot East" };
        initial_building_lot_list = new List<string>() { "Building Lot South Inner" };
        base.Awake();
        city_tilemap_go = GameObject.Find("City Tilemap");
        city_tilemap = city_tilemap_go.GetComponent<Tilemap>();
        city_room_matrix = new Room[board_width, board_height];
        West_Station = new Station(west_start_outer, west_start_inner, RouteManager.Orientation.West, RouteManager.Orientation.South, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap2, RouteManager.shipyard_track_tilemap);
        North_Station = new Station(north_start_outer, north_start_inner, RouteManager.Orientation.North, RouteManager.Orientation.East, RouteManager.Orientation.South, RouteManager.shipyard_track_tilemap, RouteManager.shipyard_track_tilemap2);
        East_Station = new Station(east_start_outer, east_start_inner, RouteManager.Orientation.East, RouteManager.Orientation.South, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap2, RouteManager.shipyard_track_tilemap);
        South_Station = new Station(south_start_outer, south_start_inner, RouteManager.Orientation.South, RouteManager.Orientation.West, RouteManager.Orientation.North, RouteManager.shipyard_track_tilemap, RouteManager.shipyard_track_tilemap2);
        city_board = new GameObject[board_width, board_height]; // zero out the negative tile coordinates

        station_track_boarding_map = new Dictionary<Vector3Int, RouteManager.Orientation>() // the direction you leave building to board boxcar
        {
            { north_start_outer, RouteManager.Orientation.East },
            {north_start_inner, RouteManager.Orientation.South },
            {east_start_outer, RouteManager.Orientation.North },
            {east_start_inner, RouteManager.Orientation.South },
            {west_start_outer, RouteManager.Orientation.South },
            {west_start_inner, RouteManager.Orientation.North },
            {south_start_outer, RouteManager.Orientation.West },
            {south_start_inner, RouteManager.Orientation.North }
        };

        station_track_unloading_map = new Dictionary<Vector3Int, Dictionary<string, RouteManager.Orientation>>()
        {
            // a dictionary of orientations for person leaving a boxcar
            {
                north_start_outer,
                    new Dictionary<string, RouteManager.Orientation>(){
                        {
                            "hor", RouteManager.Orientation.South
                        },
                        {
                            "vert", RouteManager.Orientation.West
                        },
                        {
                            "NE", RouteManager.Orientation.West
                        }
                    }
            },
            {
                north_start_inner,
                    new Dictionary<string, RouteManager.Orientation>()
                    {
                        {
                            "hor", RouteManager.Orientation.North
                        },
                        {
                            "vert", RouteManager.Orientation.East
                        },
                        {
                            "NE", RouteManager.Orientation.East
                        }
                    }
            },
            {
                east_start_outer,
                    new Dictionary<string, RouteManager.Orientation>()
                    {
                        {
                            "hor", RouteManager.Orientation.South
                        },
                        {
                            "ES", RouteManager.Orientation.South
                        },
                        {
                            "vert", RouteManager.Orientation.East
                        }
                    }
            },
            {
                east_start_inner,
                    new Dictionary<string, RouteManager.Orientation>()
                    {
                        {
                            "hor", RouteManager.Orientation.North
                        }
                    }
            },
            {
                west_start_outer,
                    new Dictionary<string, RouteManager.Orientation>()
                    {
                        {
                            "hor", RouteManager.Orientation.North
                        },
                        {
                            "WN", RouteManager.Orientation.North 
                        },
                        {
                            "vert", RouteManager.Orientation.West
                        }
                    }
            },
            {
                west_start_inner,
                    new Dictionary<string, RouteManager.Orientation>()
                    {
                        {
                            "hor", RouteManager.Orientation.South
                        }
                    }
            },
            {
                south_start_outer,
                    new Dictionary<string, RouteManager.Orientation>()
                    {
                        {
                            "vert", RouteManager.Orientation.East
                        },
                        {
                            "hor", RouteManager.Orientation.North
                        },
                        {
                            "WS", RouteManager.Orientation.East
                        }
                    }
            },
            {
                south_start_inner,
                    new Dictionary<string, RouteManager.Orientation>()
                    {
                        {
                            "hor", RouteManager.Orientation.South
                        },
                        {
                            "vert", RouteManager.Orientation.West
                        },
                        {
                            "WS", RouteManager.Orientation.West
                        },
                        {
                            "NE", RouteManager.Orientation.South
                        }
                    }
            }
        };

        initial_person_face_map = new Dictionary<string, RouteManager.Orientation>()
        {
            {"Building Lot North Outer", RouteManager.Orientation.North },
            {"Building Lot North Inner", RouteManager.Orientation.West },
            {"Building Lot South Outer", RouteManager.Orientation.South },
            {"Building Lot South Inner", RouteManager.Orientation.East },
            {"Building Lot West", RouteManager.Orientation.West },
            {"Building Lot East", RouteManager.Orientation.East}
        };

        station_track_curve_map = new Dictionary<Vector3Int, RouteManager.Orientation[]>()
        {
            // a dictionary of orientations for person leaving a boxcar
            {
                north_start_outer, new RouteManager.Orientation[]{RouteManager.Orientation.North, RouteManager.Orientation.East}
            },
            {
                north_start_inner, new RouteManager.Orientation[]{RouteManager.Orientation.West, RouteManager.Orientation.South}
            },
            {
                east_start_outer, new RouteManager.Orientation[]{RouteManager.Orientation.East, RouteManager.Orientation.North}
            },
            {
                east_start_inner, new RouteManager.Orientation[]{RouteManager.Orientation.East, RouteManager.Orientation.South}
            },
            {
                west_start_outer,new RouteManager.Orientation[]{RouteManager.Orientation.West, RouteManager.Orientation.South}
            },
            {
                west_start_inner,new RouteManager.Orientation[]{RouteManager.Orientation.West, RouteManager.Orientation.North}
            },
            {
                south_start_outer, new RouteManager.Orientation[]{RouteManager.Orientation.South, RouteManager.Orientation.West}
            },
            {
                south_start_inner, new RouteManager.Orientation[]{RouteManager.Orientation.East, RouteManager.Orientation.North}
            }
        };

        building_map = new Dictionary<string, Building_Lot>()
        {
            { "Building Lot North Outer", new Building_Lot("Building Lot North Outer",new Vector2Int(0,7),4, RouteManager.Orientation.North, new List<Station_Track>{North_Station.outer_track }, -1.0f, 90f) },
            { "Building Lot North Inner", new Building_Lot("Building Lot North Inner",new Vector2Int(3,9),3, RouteManager.Orientation.East, new List<Station_Track>{North_Station.inner_track }, 0, -1.0f) },
            { "Building Lot East", new Building_Lot("Building Lot East",new Vector2Int(11,8),6, RouteManager.Orientation.East,new List<Station_Track>{East_Station.inner_track, East_Station.outer_track }, 180, 0) },
            { "Building Lot West", new Building_Lot("Building Lot West",new Vector2Int(0,2),6, RouteManager.Orientation.East, new List<Station_Track>{West_Station.inner_track, West_Station.outer_track }, 0, 180) },
            { "Building Lot South Inner", new Building_Lot("Building Lot South Inner",new Vector2Int(10,1),4, RouteManager.Orientation.East, new List<Station_Track>{South_Station.inner_track }, 180, -1.0f) },
            { "Building Lot South Outer", new Building_Lot("Building Lot South Outer",new Vector2Int(16,1),3, RouteManager.Orientation.North, new List<Station_Track>{South_Station.outer_track }, -1.0f, 270) }
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

    public void show_all_building_occupants(bool is_city_shown)
    {
        //iterate over all buildings, within each building iterate over rooms and deactivate as necessary
        foreach (Building building in city_building_list)
        {
            building.reveal_room(is_city_shown);
        }
    }

    public Building get_city_building()
    {
        GameObject building_object;
        if (city_type == "Business")
        {
            city_tile = business_tile;
            building_object = Instantiate(Business);
        }
        else if (city_type == "Residential")
        {
            city_tile = residential_tile;
            building_object = Instantiate(Residential);
        }
        else if (city_type == "Hospital")
        {
            city_tile = hospital_tile;
            building_object = Instantiate(Hospital);
        }
        else if (city_type == "Lab")
        {
            city_tile = lab_tile;
            building_object = Instantiate(Lab);
        }
        else
        {
            throw new Exception("not a valid tile");
        }
        return building_object.GetComponent<Building>(); // will this work? is a base class of the gameObject
    }

    public void expand_building(Building_Lot bl, Vector2Int selected_tile)
    {
        Building building = bl.building;
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
            building.spawn_room();
        }
        set_room_sprites();
        show_all_building_occupants(true);
    }

    public void unload_train(GameObject boxcar_go, Vector2Int room_position)
    {
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        boxcar.passenger_go.GetComponent<Person>().offset_map = RouteManager.offset_route_map[boxcar.station_track.start_location];
        boxcar.passenger_go.GetComponent<Person>().is_enter_home = true;
        Room room = city_room_matrix[room_position.x, room_position.y];
        string track_name = RouteManager.shipyard_track_tilemap.GetTile(boxcar.tile_position).name;
        print("unloading train from room  position " + room_position + " to " + boxcar.tile_position);
        RouteManager.Orientation exit_orientation = station_track_unloading_map[boxcar.station_track.start_location][track_name];
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
        //occupant.orientation = boxcar.station_track.board_orientation;
        StartCoroutine(GameObject.Find("PersonRouteManager").GetComponent<PersonRouteManager>().board_train(boxcar, room, occupant, boxcar.tile_position));
        // get the tile in direction of the track

        // then run shortest path algorithm with preference for tiles closer to the building
        // finally, board the train
    }

    public void set_room_sprites()
    {
        for (int i=0;i<city_room_matrix.GetLength(0);i++)
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

    public void initialize_city_tilemap()
    {
        foreach (string initial_building_lot in initial_building_lot_list)
        {
            Building_Lot first_building_lot = building_map[initial_building_lot];
            spawn_building(first_building_lot);
            show_all_building_occupants(false); // hide all doors when city is first created
            building_id += 1;
        }
    }

    public void spawn_building(Building_Lot building_lot)
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

    public List<GameObject> get_train_list()
    {
        return train_list;
    }

    public void show_turntable(bool state)
    {
        turn_table.GetComponent<SpriteRenderer>().enabled = state;
        turn_table_circle.GetComponent<SpriteRenderer>().enabled = state;
    }

    public void parking_validator()
    {
        // is available?
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
        for (int i = 0; i < TrackManager.parking_coord.GetLength(0); i++)
        {
            parking_spot = find_parking_spot(TrackManager.parking_coord[i,0], TrackManager.parking_coord[i,1], TrackManager.parking_coord[i,2]);
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

    public void place_boxcar_tile(GameObject boxcar_object, Vector2Int tile_pos)
    {
        Tilemap shipyard_inventory = GameManager.Shipyard_Inventory.GetComponent<Tilemap>();
        string boxcar_name = boxcar_object.name.Replace("(Clone)", "");
        if (boxcar_name == "bomb boxcar") place_tile(tile_pos, boxcar_object, bomb_boxcar_tile, shipyard_inventory);
        else if (boxcar_name == "supply boxcar") place_tile(tile_pos, boxcar_object, supply_boxcar_tile, shipyard_inventory);
        else if (boxcar_name == "troop boxcar") place_tile(tile_pos, boxcar_object, troop_boxcar_tile, shipyard_inventory);
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
                if (tile_pos.y==7) return East_Station.inner_track;
                else { return East_Station.outer_track; }
            }
        }
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
