using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    protected GameManager game_manager;
    protected CityManager city_manager;

    protected string prefab_tag;
    protected static GameObject[,] gameobject_board;
    protected static Tilemap tilemap;
    protected Vector2Int board_dimension;
    protected Vector2Int home_base_location;
    protected City home_base;


    public void Awake()
    {
        city_manager = GameObject.Find("CityManager").GetComponent<CityManager>();
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        board_dimension = game_manager.get_board_dimension();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        home_base_location = game_manager.get_home_base();
        home_base = CityManager.get_city(home_base_location).GetComponent<City>();
    }

    // Update is called once per frame
    void Update()
    {

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
