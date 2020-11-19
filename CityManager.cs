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

    public GameObject City;
    public static GameObject Activated_City;
    public static City Activated_City_Component;
    public Tilemap exit_north;
    public Tilemap exit_south;
    public Tilemap exit_west;
    public Tilemap exit_east;
    static City cit;

    // distances for train to travel before exiting the city (not before stopping)
    public static float exit_dest_west_east = 6;
    public static float exit_dest_north_south = 3;

    private void Awake()
    {
        base.Awake();
        create_cities(); // instantiate cities and save their positions
        home_base = get_city(home_base_location).GetComponent<City>();
    }

    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static RouteManager.Orientation get_station_orientation(Vector2Int tile_pos)
    {
        City activated_city = Activated_City.GetComponent<City>();
        return activated_city.get_station_track(tile_pos).station.orientation;
    }

    public bool add_boxcar_to_station(string boxcar_type, Vector2Int tile_pos, Vector2Int boxcar_tile_pos)
    {
        // Activated City
        City activated_city = Activated_City.GetComponent<City>();
        GameObject train_object = activated_city.get_station_track(tile_pos).train;
        print("train id is " + train_object.GetComponent<Train>().get_id());
        if (train_object != null)
        {
            GameManager.vehicle_manager.add_boxcar_to_train(train_object.GetComponent<Train>(), boxcar_type);
            activated_city.remove_boxcar_from_inventory(boxcar_tile_pos); // after adding a boxcar to train, remove it from inventory
            return true;
        }
        else {
            print("no train found in this track");
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

    public void set_activated_city(GameObject city_object=null)
    {
        if (city_object == null) // hide shipyard
        {
            GameManager.city_menu_state = false;
            Activated_City.GetComponent<City>().enable_train_for_screen(); // hide trains before setting activated city to null
            Activated_City.GetComponent<City>().display_boxcar(false);
        }
        else // show shipyard
        {
            GameManager.city_menu_state = true;
            city_object.GetComponent<City>().display_boxcar(true);
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
                    GameObject city = Instantiate(City);
                    city.GetComponent<City>().set_location(cell_position);
                    gameobject_board[r, c] = city;
                }
            }
        }
    }
}
