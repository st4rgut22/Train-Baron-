using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TrafficLightManager : BoardManager
{
    public Traffic_Light[,] traffic_light_matrix;
    public GameObject traffic_tilemap_go;
    public Tilemap traffic_tilemap;

    public enum Traffic_Light
    {
        Green,
        Red,
        Yellow
    }

    private void Awake()
    {
        traffic_tilemap_go = GameObject.Find("Traffic Light");
        traffic_tilemap = traffic_tilemap_go.GetComponent<Tilemap>();
        traffic_tilemap_go.SetActive(false);
        traffic_light_matrix = new Traffic_Light[board_width, board_height];
        initialize_traffic_matrix();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void initialize_traffic_matrix()
    {
        for (int i = 0; i < board_height;i++)
        {
            for (int j = 0; j < board_width; j++)
            {
                Tile traffic_light_tile = (Tile)traffic_tilemap.GetTile(new Vector3Int(i, j, 0));
                if (traffic_light_tile != null)
                {
                    switch (traffic_light_tile.name)
                    {
                        case "green_light":
                            traffic_light_matrix[i, j] = Traffic_Light.Green;
                            break;
                        case "yellow_light":
                            traffic_light_matrix[i, j] = Traffic_Light.Yellow;
                            break;
                        case "red_light":
                            traffic_light_matrix[i, j] = Traffic_Light.Red;
                            break;
                        default:
                            throw new Exception("Not a valid Traffic Light Tile");
                    }
                }
            }
        }
    }

}
