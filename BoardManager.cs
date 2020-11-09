using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class BoardManager : EventDetector
{

    //information about static positions on board
    protected GameManager game_manager;

    protected string prefab_tag;
    public static GameObject[,] gameobject_board;
    protected static Tilemap tilemap;
    protected City home_base;

    public static Vector2Int home_base_location = new Vector2Int(3,6); // location of city

    protected const int board_width = 17;
    protected const int board_height = 10; // size of the shipyard tilemap (usable tiles in track tilemap is slightly smaller)

    public void Awake()
    {
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        home_base = CityManager.get_city(home_base_location).GetComponent<City>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector2Int get_board_dimension()
    {
        return new Vector2Int(board_width, board_height);
    }

    public static Vector2Int pos_to_tile(Vector3 tile_position)
    {
        return new Vector2Int((int)(tile_position.x / RouteManager.cell_width), (int)(tile_position.y / RouteManager.cell_width));
    }

    public GameObject in_cell(Vector3Int position)
    {
        if (gameobject_board[position.x, position.y] != null)
        {
            GameObject game_object = gameobject_board[position.x, position.y];
            if (game_object.tag == prefab_tag)
            {
                return game_object;
            }
        }
        return null;
    }
}
