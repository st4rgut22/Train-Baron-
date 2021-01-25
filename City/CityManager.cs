using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System;

public class CityManager : BoardManager
{
    //instantiate the cities.
    //maintain lists of allied / enemy cities
    //oversee routes between cities for to inform decision making

    public static List<City> city_list;
    public static City home_base;
    //public static Vector2Int home_base_location = new Vector2Int(3, 6); // location of city
    public static Vector2Int home_base_location = new Vector2Int(5, 6); // TODO: temporary to test structure
    public int city_id;
    public GameObject City;
    public static GameObject Activated_City;
    public static City Activated_City_Component;
    public Tilemap exit_north;
    public Tilemap exit_south;
    public Tilemap exit_west;
    public Tilemap exit_east;

    public GameObject city_tilemap_go;
    public static int total_people;

    // distances for train to travel before exiting the city (not before stopping)
    public static float exit_dest_west_east = 6;
    public static float exit_dest_north_south = 3;
    public static Dictionary<RouteManager.Orientation, float> orientation_to_rotation_map; // needed for proper rotation on boarding and unloading of trains
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

    public static Dictionary<Vector3Int, RouteManager.Orientation> station_track_boarding_map;
    public static Dictionary<Vector3Int, Dictionary<string, RouteManager.Orientation>> station_track_unloading_map;
    public static Dictionary<Vector3Int, RouteManager.Orientation[]> station_track_curve_map; // array index 0 is original orientation, 1 is final orientation

    public static Dictionary<string, int> building_count_dict = new Dictionary<string, int>(); // <building name, building count>
    public static List<int[]> city_plot_location = new List<int[]>() { new int[] { 0, 2 }, new int[] { 1, 2 }, new int[] { 2, 2 }, new int[] { 3, 2 },new int[]{4,2 }, new int[]{5,2 },
        new int[]{0,7 }, new int[]{0,8 }, new int[]{0,9 }, new int[]{3,9 }, new int[]{4,9 }, new int[]{5,9 }, new int[]{6,9 }, new int[]{10,1 }, new int[]{11,1 }, new int[]{12,1 },
        new int[]{13,1 }, new int[]{16,1 }, new int[]{16,2 }, new int[]{16,3 }, new int[]{16,9 }, new int[]{15,9 }, new int[]{14,9 }, new int[]{13,9 }, new int[]{12,9 }, new int[]{11,9 } };

    public static List<int[]>  boxcar_city_wait_tile = new List<int[]> { new int[] { 6, 1 }, new int[] { 12, 2 } , new int[] { 12, 3 } }; // ADD MORE 

    public int entrance_update_interval;

    private void Awake()
    {
        base.Awake();
        city_list = new List<City>();
        total_people = 0;
        entrance_update_interval = 15;
        create_city((Vector3Int)home_base_location); // initialize train entrance
        home_base = get_city(home_base_location).GetComponent<City>();
        orientation_to_rotation_map = new Dictionary<RouteManager.Orientation, float>()
        {
            {RouteManager.Orientation.North, 90 },
            {RouteManager.Orientation.East, 0 },
            {RouteManager.Orientation.West, 180 },
            {RouteManager.Orientation.South, -90 }
        };
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
                            "NE", RouteManager.Orientation.North // west
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
                            "NE", RouteManager.Orientation.North
                        },
                        {
                            "WS", RouteManager.Orientation.North
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
                            "ES", RouteManager.Orientation.East
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
                            "WS", RouteManager.Orientation.North
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
                            "WS", RouteManager.Orientation.South
                        },
                        {
                            "NE", RouteManager.Orientation.South
                        }
                    }
            }
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
                west_start_outer,new RouteManager.Orientation[]{RouteManager.Orientation.East, RouteManager.Orientation.South}
            },
            {
                west_start_inner,new RouteManager.Orientation[]{RouteManager.Orientation.East, RouteManager.Orientation.North}
            },
            {
                south_start_outer, new RouteManager.Orientation[]{RouteManager.Orientation.South, RouteManager.Orientation.West}
            },
            {
                south_start_inner, new RouteManager.Orientation[]{RouteManager.Orientation.East, RouteManager.Orientation.North}
            }
        };
    }

    void Start()
    {
        base.Start();
        StartCoroutine(populate_entrance());
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static void increment_total_people()
    {
        total_people += 1;
        if (total_people >= GameManager.goal)
        {
            GameManager.end_level(true);
        }
    }

    public static GameObject get_vehicle_in_activated_city(List<Collider2D> colliders, string tag)
    {
        foreach (Collider2D collider in colliders)
        {
            MovingObject moving_go = collider.gameObject.GetComponent<MovingObject>();
            if (moving_go != null && moving_go.tag == tag)
            {
                if (moving_go.city == Activated_City_Component) return moving_go.gameObject;
            }
        }
        return null;
    }

    public static int get_building_count(string building_name)
    {
        if (building_count_dict.ContainsKey(building_name))
            return building_count_dict[building_name];
        else
        {
            return 0;
        }
    }

    public static void update_building_count(string building_name, int update_num)
    {
        if (building_count_dict.ContainsKey(building_name))
        {
            building_count_dict[building_name] += update_num;
        }
        else
        {
            building_count_dict[building_name] = 1;
        }
    }

    public static bool is_exit_route_shown(RouteManager.Orientation orientation)
    {
        //todo
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                return RouteManager.exit_north_tilemap.GetComponent<TilemapRenderer>().enabled;
            case RouteManager.Orientation.East:
                return RouteManager.exit_east_tilemap.GetComponent<TilemapRenderer>().enabled;
            case RouteManager.Orientation.West:
                return RouteManager.exit_west_tilemap.GetComponent<TilemapRenderer>().enabled;
            case RouteManager.Orientation.South:
                return RouteManager.exit_south_tilemap.GetComponent<TilemapRenderer>().enabled;
            default:
                return false;
        }
    }

    public bool hide_exit_route(RouteManager.Orientation orientation, City city, Tilemap exit_tilemap)
    {
        Vector3Int city_loc = city.get_location();
        Vector3Int loc;
        if (orientation == RouteManager.Orientation.North)
            loc = new Vector3Int(city_loc.x, city_loc.y + 1, city_loc.z);
        else if (orientation == RouteManager.Orientation.East)
            loc = new Vector3Int(city_loc.x + 1, city_loc.y, city_loc.z);
        else if (orientation == RouteManager.Orientation.West)
            loc = new Vector3Int(city_loc.x - 1, city_loc.y, city_loc.z);
        else if (orientation == RouteManager.Orientation.South)
            loc = new Vector3Int(city_loc.x, city_loc.y - 1, city_loc.z);
        else { throw new Exception("not a valid orientation"); }
        Tilemap track_tilemap = GameManager.track_manager.top_tilemap;
        Tile tile_type = (Tile)track_tilemap.GetTile(loc);
        if (tile_type == null) // todo: an additional check that exit track orientation is correct
        {
            exit_tilemap.GetComponent<TilemapRenderer>().enabled = false;
            return true;
        }
        else
        {
            if ((orientation == RouteManager.Orientation.South && (tile_type.name=="vert" || tile_type.name == "WN" || tile_type.name == "NE")) ||
                (orientation == RouteManager.Orientation.North && (tile_type.name == "vert" || tile_type.name == "WS" || tile_type.name == "ES")) ||
                (orientation == RouteManager.Orientation.East && (tile_type.name == "hor" || tile_type.name == "WN" || tile_type.name == "WS")) ||
                (orientation == RouteManager.Orientation.West && (tile_type.name == "hor" || tile_type.name == "NE" || tile_type.name == "ES")))
            {
                exit_tilemap.GetComponent<TilemapRenderer>().enabled = true;
                return false;
            }
            else
            {
                exit_tilemap.GetComponent<TilemapRenderer>().enabled = false;
                return true;
            }
            
        }
    }

    public static RouteManager.Orientation get_station_orientation(Vector2Int tile_pos)
    {
        City activated_city = Activated_City.GetComponent<City>();
        return activated_city.get_station_track(tile_pos).station.orientation;
    }

    public bool add_boxcar_to_station(string boxcar_type, Vector2Int tile_pos, Vector2Int boxcar_tile_pos)
    {
        // Activated City
        try
        {
            City activated_city = Activated_City.GetComponent<City>();
            GameObject train_object = activated_city.get_station_track(tile_pos).train;
            if (train_object != null && train_object.GetComponent<Train>().is_wait_for_turntable)
            {
                GameManager.vehicle_manager.add_boxcar_to_train(train_object.GetComponent<Train>(), boxcar_type);
                activated_city.remove_boxcar_from_inventory(boxcar_tile_pos); // after adding a boxcar to train, remove it from inventory
                return true;
            }
            else
            {
                return false;
            }
        } catch (NullReferenceException)
        {
            return false;
        }
    }

    public static bool is_tile_in_station(Vector2Int tile_pos)
    {
        // check if a boxcar is being placed on a valid station
        Tile shipyard_track_tile = (Tile)GameManager.Shipyard_Track.GetComponent<Tilemap>().GetTile((Vector3Int)tile_pos);
        Tile shipyard_track_2_tile = (Tile)GameManager.Shipyard_Track2.GetComponent<Tilemap>().GetTile((Vector3Int)tile_pos);
        if (shipyard_track_tile != null || shipyard_track_2_tile != null)
            return true;
        else
            return false;
    }

    public void set_destination_track(RouteManager.Orientation orientation)
    {
        Activated_City.GetComponent<City>().set_destination_track(orientation);
    }

    public void hide_shipyard_inventory()
    {
        Tilemap tilemap = GameManager.Shipyard_Inventory.GetComponent<Tilemap>();
        for (int i = 0; i < board_width; i++)
        {
            for (int j=0;j < board_height; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), null);
            }
        }
    }

    public IEnumerator populate_entrance()
    {
        while (true)
        {
            home_base.populate_entrance();
            yield return new WaitForSeconds(entrance_update_interval);
        }
    }

    public void set_activated_city(GameObject city_object=null)
    {
        if (city_object == null) // hide shipyard
        {
            city_tilemap_go.SetActive(false);
            PersonManager.show_notification(true);
            GameManager.city_menu_state = false;
            City activated_city = Activated_City.GetComponent<City>();
            activated_city.enable_train_for_screen();
            activated_city.show_all_building_occupants(false);
            activated_city.show_all_undeveloped_plots(true);
            activated_city.change_traffic_signal(false);
            hide_shipyard_inventory();
        }
        else // show shipyard
        {
            PersonManager.show_notification(false);
            city_tilemap_go.SetActive(true);
            // populate city tilemap
            GameManager.city_menu_state = true;
            City city = city_object.GetComponent<City>();
            city.set_all_room_sprites();
            city.show_all_building_occupants(true);
            city.show_all_undeveloped_plots(false);
            city.display_boxcar();
            //if (city.city_type == "Entrance")
            //    city.populate_entrance();
            hide_exit_route(RouteManager.Orientation.North, city, RouteManager.exit_north_tilemap);
            hide_exit_route(RouteManager.Orientation.East, city, RouteManager.exit_east_tilemap);
            hide_exit_route(RouteManager.Orientation.West, city, RouteManager.exit_west_tilemap);
            hide_exit_route(RouteManager.Orientation.South, city, RouteManager.exit_south_tilemap);
            city.change_traffic_signal(true);
        }
        if (city_object == null) Activated_City.GetComponent<City>().show_turntable(false);
        else { city_object.GetComponent<City>().show_turntable(true); }
        Activated_City = city_object;
        if (Activated_City == null) Activated_City_Component = null;
        else {
            Activated_City_Component = Activated_City.GetComponent<City>();
            Activated_City_Component.enable_train_for_screen(); // hide trains before setting activated city to null
            Activated_City_Component.apply_reputation();
        }
    }

    //public static RouteManager.Orientation set_orientation_of_new_train()
    //{
    //    // when a train is created, find which direction train should be in to pick up passengers
    //    RouteManager.Orientation train_orientation = home_base.get_orientation_of_open_track();
    //    print("orientation of new train is " + train_orientation);
    //    return train_orientation;
    //}

    public static float get_exit_dist(RouteManager.Orientation orientation)
    {
        if (orientation == RouteManager.Orientation.West || orientation == RouteManager.Orientation.East)
            return exit_dest_west_east;
        else if (orientation == RouteManager.Orientation.North || orientation == RouteManager.Orientation.South)
            return exit_dest_north_south;
        else
        {
            throw new Exception(orientation + " is not a valid exit destination");
        }
    }

    public GameObject get_city(Vector2Int city_location)
    {
        return gameobject_board[city_location.x, city_location.y];
    }

    public static List<GameObject> get_all_city_train()
    {
        List<GameObject> train_list = new List<GameObject>();
        foreach (City city in city_list)
        {
            train_list.AddRange(city.train_list);
        }
        return train_list;
    }

    public void create_city(Vector3Int cell_position)
    {
        // initialize board with stationary tiles eg cities
        Tilemap structure_tilemap = GameManager.Structure.GetComponent<Tilemap>();

        Tile structure_tile = (Tile)structure_tilemap.GetTile(cell_position);
        if (structure_tile != null)
        {
            string city_type = structure_tile.name;
            GameObject city = Instantiate(City);
            City city_component = city.GetComponent<City>();
            city_list.Add(city_component);
            city_component.set_location(cell_position);
            city_component.city_type = city_type;
            city_component.city_id = city_id;
            gameobject_board[cell_position.x, cell_position.y] = city;
            city_id++;
        }
    }
}
