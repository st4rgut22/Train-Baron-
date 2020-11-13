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
    public GameObject[,] gameobject_board;
    //protected static Tilemap tilemap;
    public static Vector2Int invalid_tile = new Vector2Int(-1, -1);
    protected const int board_width = 17;
    protected const int board_height = 10; // size of the shipyard tilemap (usable tiles in track tilemap is slightly smaller)
    
    public void Awake()
    {
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        gameobject_board = new GameObject[board_width, board_height];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void place_tile(Vector2Int tilemap_position, GameObject tile_object, Tile tile, Tilemap tilemap, bool display)
    {
        // place a tile if no tile already exists
        print("place tile " + tile_object.name + " at position " + tilemap_position);
        GameObject existing_tile = gameobject_board[tilemap_position.x, tilemap_position.y];
        //if (existing_tile == null)
        //{
        if (display)
        {
            tilemap.SetTile((Vector3Int)tilemap_position, tile);
            gameobject_board[tilemap_position.x, tilemap_position.y] = tile_object;
        }
        else
        {
            tilemap.SetTile((Vector3Int)tilemap_position, null); // remove tile (eg hiding the boxcar)
        }

        //}
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
