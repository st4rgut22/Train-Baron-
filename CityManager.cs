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
        gameobject_board = new GameObject[board_dimension.x, board_dimension.y];
        // initialize board with stationary tiles eg cities
        for (int r = 0; r < board_dimension.x; r++)
        {
            for (int c = 0; c < board_dimension.y; c++)
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
