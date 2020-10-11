using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TrackManager : BoardManager
{
    //responsible for placing tracks on tilemaps, switching tracks, etc.

    Track[,] track_board;

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
        track_board = new Track[board_dimension.x, board_dimension.y];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void place_tile(Vector3Int tilemap_position, string tile_name, Tile tile)
    {
        print("place tile " + tile_name + " at position " + tilemap_position);
        tilemap.SetTile(tilemap_position, tile);
        if (tile_name != "train(Clone)" && tile_name!="boxcar(Clone") // if track is clicked, add it to track map
            track_board[tilemap_position.x, tilemap_position.y] = new Track(tile_name);
    }



}
