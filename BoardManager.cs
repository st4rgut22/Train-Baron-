using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{

    //information about static positions on board
    protected GameManager game_manager;

    protected string prefab_tag;
    protected static GameObject[,] gameobject_board;
    protected static Tilemap tilemap;
    protected City home_base;

    public static Vector2Int west_end_1 = new Vector2Int(4, 0);
    public static Vector2Int west_end_2 = new Vector2Int(4, 1);
    public static Vector2Int west_start_1 = new Vector2Int(-1,0);
    public static Vector2Int west_start_2 = new Vector2Int(-1,1);

    public static Vector2Int north_end_1 = new Vector2Int(4, 5);
    public static Vector2Int north_end_2 = new Vector2Int(4, 6);
    public static Vector2Int north_start_1 = new Vector2Int(0,7);
    public static Vector2Int north_start_2 = new Vector2Int(1,7);

    public static Vector2Int east_end_1 = new Vector2Int(10, 5);
    public static Vector2Int east_end_2 = new Vector2Int(10, 6);
    public static Vector2Int east_start_1 = new Vector2Int(15,5);
    public static Vector2Int east_start_2 = new Vector2Int(15,6);

    public static Vector2Int south_end_1 = new Vector2Int(10, 1);
    public static Vector2Int south_end_2 = new Vector2Int(10, 0);
    public static Vector2Int south_start_1 = new Vector2Int(14,-2);
    public static Vector2Int south_start_2 = new Vector2Int(13,-2);
    public static Vector2Int home_base_location = new Vector2Int(0, 0);

    protected const int board_width = 15;
    protected const int board_height = 7;

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
