using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class CityManager : BoardManager
{
    //instantiate the cities.
    //maintain lists of allied / enemy cities
    //oversee routes between cities for to inform decision making

    public GameObject City;
    GameObject Activated_City;
    public Tilemap exit_north;
    public Tilemap exit_south;
    public Tilemap exit_west;
    public Tilemap exit_east;

    // Start is called before the first frame update
    void Start()
    {
        set_tilemap("city_layer");
        prefab_tag = "city";
        create_cities(); // instantiate cities and save their positions
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void set_destination_track(RouteManager.Orientation orientation)
    {
        this.Activated_City.GetComponent<City>().set_destination_track(orientation);
    }

    public void set_activated_city(GameObject city_object=null)
    {
        if (city_object == null) // hide shipyard
        {
            this.Activated_City.GetComponent<City>().is_train_turn_on(false);
        } else // show shipyard
        {
            city_object.GetComponent<City>().is_train_turn_on(true);
        }
        this.Activated_City = city_object;
    }

    public static GameObject get_city(Vector2Int city_location)
    {
        return gameobject_board[city_location.x, city_location.y];
    }

    public void create_cities()
    {
        gameobject_board = new GameObject[board_width, board_height];
        // initialize board with stationary tiles eg cities
        for (int r = 0; r < board_width; r++)
        {
            for (int c = 0; c < board_height; c++)
            {
                Vector3Int cell_position = new Vector3Int(r, c, 0);
                Tile structure_tile = (Tile) tilemap.GetTile(cell_position);
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
