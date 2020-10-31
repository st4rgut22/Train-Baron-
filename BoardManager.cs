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
    protected static GameObject[,] gameobject_board;
    protected static Tilemap tilemap;
    protected City home_base;

    public static Vector2Int home_base_location = new Vector2Int(0, 0);

    protected const int board_width = 17;
    protected const int board_height = 8;

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

    protected void set_tilemap(string tilemap_name)
    {
        GameObject tilemap_object = GameObject.FindGameObjectsWithTag(tilemap_name)[0];
        if (tilemap_object != null)
        {
            tilemap = tilemap_object.GetComponent<Tilemap>();
        }
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
