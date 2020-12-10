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

    public static City home_base;
    public static Vector2Int home_base_location = new Vector2Int(3, 6); // location of city
    public int city_id;
    public GameObject City;
    public static GameObject Activated_City;
    public static City Activated_City_Component;
    public Tilemap exit_north;
    public Tilemap exit_south;
    public Tilemap exit_west;
    public Tilemap exit_east;

    public GameObject city_tilemap_go;

    // distances for train to travel before exiting the city (not before stopping)
    public static float exit_dest_west_east = 6;
    public static float exit_dest_north_south = 3;
    public static Dictionary<string, List<string>> cargo_to_structure;
    public static Dictionary<RouteManager.Orientation, float> orientation_to_rotation_map; // needed for proper rotation on boarding and unloading of trains

    private void Awake()
    {
        base.Awake();
        create_cities(); // instantiate cities and save their positions
        home_base = get_city(home_base_location).GetComponent<City>();
        orientation_to_rotation_map = new Dictionary<RouteManager.Orientation, float>()
        {
            {RouteManager.Orientation.North, 90 },
            {RouteManager.Orientation.East, 0 },
            {RouteManager.Orientation.West, 180 },
            {RouteManager.Orientation.South, -90 }
        };
    }

    void Start()
    {
        base.Start();
        cargo_to_structure = new Dictionary<string, List<string>>
        //todo replace keys with names of actual cargo (by changing boxcar names?)
        { // {{coordinates of loading tiles for outer track},{coordinates of loading tiles for inner track}}
            { "bomb boxcar",new List<string>(){ "Business","Hospital","Lab","Residential"} }, //people
            { "troop boxcar",new List<string>(){ "Hospital"} }, // medicine
            { "supply boxcar",new List<string>(){ "Business","Hospital","Lab","Residential"} }, // food
        };

    }

    // Update is called once per frame
    void Update()
    {
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
        else {
            exit_tilemap.GetComponent<TilemapRenderer>().enabled = true;
            return false;
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
            if (train_object != null)
            {
                GameManager.vehicle_manager.add_boxcar_to_train(train_object.GetComponent<Train>(), boxcar_type);
                activated_city.remove_boxcar_from_inventory(boxcar_tile_pos); // after adding a boxcar to train, remove it from inventory
                return true;
            }
            else
            {
                //print("no train found in this track");
                return false;
            }
        } catch (NullReferenceException)
        {
            //print("no train available  to add boxcar to at tile position " + tile_pos);
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

    //public static void expand_city(PointerEventData eventData)
    //{
    //    RaycastHit2D selected_object = GameManager.get_object_at_cursor(eventData.pressPosition);
    //    Activated_City_Component.expand_building(selected_object.collider.name);
    //}

    public void set_activated_city(GameObject city_object=null)
    {
        if (city_object == null) // hide shipyard
        {
            city_tilemap_go.SetActive(false);
            GameManager.city_menu_state = false;
            Activated_City.GetComponent<City>().enable_train_for_screen(); // hide trains before setting activated city to null
            Activated_City.GetComponent<City>().show_all_building_occupants(false);
            hide_shipyard_inventory();
        }
        else // show shipyard
        {
            city_tilemap_go.SetActive(true);
            // populate city tilemap
            GameManager.city_menu_state = true;
            City city = city_object.GetComponent<City>();
            city.set_room_sprites();
            city.show_all_building_occupants(true);
            city.display_boxcar();
            hide_exit_route(RouteManager.Orientation.North, city, RouteManager.exit_north_tilemap);
            hide_exit_route(RouteManager.Orientation.East, city, RouteManager.exit_east_tilemap);
            hide_exit_route(RouteManager.Orientation.West, city, RouteManager.exit_west_tilemap);
            hide_exit_route(RouteManager.Orientation.South, city, RouteManager.exit_south_tilemap);

        }
        if (city_object == null) Activated_City.GetComponent<City>().show_turntable(false);
        else { city_object.GetComponent<City>().show_turntable(true); }
        Activated_City = city_object;
        if (Activated_City == null) Activated_City_Component = null;
        else {
            Activated_City_Component = Activated_City.GetComponent<City>();
            Activated_City_Component.enable_train_for_screen(); // hide trains before setting activated city to null
        }
    }

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

    
    public void create_cities()
    {
        // initialize board with stationary tiles eg cities
        Tilemap structure_tilemap = GameManager.Structure.GetComponent<Tilemap>();
        for (int r = 0; r < board_width; r++)
        {
            for (int c = 0; c < board_height; c++)
            {
                Vector3Int cell_position = new Vector3Int(r, c, 0);
                Tile structure_tile = (Tile)structure_tilemap.GetTile(cell_position);
                if (structure_tile != null)
                {
                    string city_type = structure_tile.name;
                    GameObject city = Instantiate(City);
                    city.GetComponent<City>().set_location(cell_position);
                    city.GetComponent<City>().city_type = city_type;
                    city.GetComponent<City>().city_id = city_id;
                    gameobject_board[r, c] = city;
                    city_id++;
                }
            }
        }
    }
}
