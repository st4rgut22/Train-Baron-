using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CityManager : BoardManager
{
    //instantiate the cities.
    //maintain lists of allied / enemy cities
    //oversee routes between cities for to inform decision making

    public GameObject City;
    GameObject Activated_City;
    public GameObject[] City_Object_List;

    // Start is called before the first frame update
    void Start()
    {
        set_tilemap("city_layer");
        prefab_tag = "city";
        create_cities(); // instantiate cities and save their positions
        City_Object_List = GameObject.FindGameObjectsWithTag("city");
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

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

    public void add_train_to_board(Vector3Int tile_position, GameObject train)
    {
        GameObject city_object = gameobject_board[tile_position.x, tile_position.y];
        city_object.GetComponent<City>().add_train_to_list(train);
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

    public void activate_city(GameObject city_object, bool state)
    {
        city_object.GetComponent<City>().is_train_turn_on(state);
        city_object.SetActive(state);
    }
}
