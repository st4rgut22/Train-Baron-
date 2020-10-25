using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TrackManager : BoardManager
{
    //responsible for placing tracks on tilemaps, switching tracks, etc.

    Track[,] track_board;
    public static int hor_count = 0;
    public static int wn_count = 0;
    public static int vert_count = 0;
    public static int ne_count = 0;
    public static int ws_count = 0;
    public static int es_count = 0;

    class Track 
    {
        public Track(string name)
        {
            string track_name = name;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        set_tilemap("track_layer");
        track_board = new Track[board_width, board_height];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void add_track(string track_type)
    {
        switch (track_type)
        {
            case "vert_desc":
                vert_count++;
                break;
            case "hor_desc":
                hor_count++;
                break;
            case "wn_desc":
                wn_count++;
                break;
            case "ne_desc":
                ne_count++;
                break;
            case "ws_desc":
                ws_count++;
                break;
            case "es_desc":
                es_count++;
                break;
            default:
                break;
        }
    }

    public void place_tile(Vector3Int tilemap_position, string tile_name, Tile tile)
    {
        print("place tile " + tile_name + " at position " + tilemap_position);
        tilemap.SetTile(tilemap_position, tile);
        if (tile_name != "train(Clone)" && tile_name!="boxcar(Clone") // if track is clicked, add it to track map
            track_board[tilemap_position.x, tilemap_position.y] = new Track(tile_name);
    }



}
