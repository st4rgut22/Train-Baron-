using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class RouteManager : MonoBehaviour
{
    public static Tilemap tilemap;

    public Train[] train_list;

    public  Tile ES_tile;
    public Tile NE_tile;
    public Tile WN_tile;
    public Tile hor_tile;
    public Tile WS_tile;
    public Tile vert_tile;

    GameManager game_manager;

    public void place_tile(Vector3 tile_position, string tile_name)
    {
        Vector3Int tilemap_position = new Vector3Int((int)tile_position.x, (int)tile_position.y, (int)tile_position.z);
        print("place tile " + tile_name + " at position " + tilemap_position);
        switch (tile_name)
        {
            case "curve_ES(Clone)":
                tilemap.SetTile(tilemap_position, ES_tile);
                break;
            case "curve_NE(Clone)":
                tilemap.SetTile(tilemap_position, NE_tile);
                break;
            case "curve_WN(Clone)":
                tilemap.SetTile(tilemap_position, WN_tile);
                break;
            case "curve_WS(Clone)":
                tilemap.SetTile(tilemap_position, WS_tile);
                break;
            case "hor_track(Clone)":
                tilemap.SetTile(tilemap_position, hor_tile);
                break;
            case "vert_track(Clone)":
                tilemap.SetTile(tilemap_position, vert_tile);
                break;
            case "train(Clone)": // TODO: specify starting position of train
                game_manager.create_train(); // instantiate train at origin
                break;
            case "boxcar(Clone)":
                game_manager.create_boxcar(tilemap_position);
                break;
            default:
                print("You did not click a store item");
                break;
        }
    }

    public static Vector2 get_destination(MovingObject moving_thing)
    {
        Vector3Int tile_coord = new Vector3Int(moving_thing.tile_position[0], moving_thing.tile_position[1], 0);
        //print("next tile coordinate is " + tile_coord);
        Tile tile = (Tile) tilemap.GetTile(tile_coord);
        Vector2 tile_world_coord = tilemap.GetCellCenterWorld(tile_coord);
        Vector2 final_cell_dest = moving_thing.transform.position;
        try {
            string tile_name = tile.name;
            switch (tile_name)
            {
                case "curve_ES":
                    if (moving_thing.orientation == MovingObject.Orientation.West)
                    {                        
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - .5f);
                        moving_thing.final_orientation = MovingObject.Orientation.South;
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + 0.5f, tile_world_coord[1]);
                        moving_thing.final_orientation = MovingObject.Orientation.East;
                    }
                    break;
                case "curve_NE":
                    if (moving_thing.orientation == MovingObject.Orientation.South)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + 0.5f, tile_world_coord[1]);
                        moving_thing.final_orientation = MovingObject.Orientation.East;
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + 0.5f);
                        moving_thing.final_orientation = MovingObject.Orientation.North;
                    }
                    break;
                case "curve_WN":
                    if (moving_thing.orientation == MovingObject.Orientation.East)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + 0.5f);
                        moving_thing.final_orientation = MovingObject.Orientation.North;
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] - 0.5f, tile_world_coord[1]);
                        moving_thing.final_orientation = MovingObject.Orientation.West;
                    }

                    break;
                case "curve_WS":
                    if (moving_thing.orientation == MovingObject.Orientation.East)
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - 0.5f);
                        moving_thing.final_orientation = MovingObject.Orientation.South;
                    }
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] - 0.5f, tile_world_coord[1]);
                        moving_thing.final_orientation = MovingObject.Orientation.West;
                    }
                    break;
                case "vert_track":
                    if (moving_thing.orientation == MovingObject.Orientation.North)
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] + .5f);
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0], tile_world_coord[1] - .5f);
                    }
                    break;
                case "hor_track":
                    if (moving_thing.orientation == MovingObject.Orientation.West)
                        final_cell_dest = new Vector2(tile_world_coord[0] - .5f, tile_world_coord[1]);
                    else
                    {
                        final_cell_dest = new Vector2(tile_world_coord[0] + .5f, tile_world_coord[1]);
                    }
                    break;
                default:
                    print("none of the track tiles matched"); // return current position
                    break;
            }
        } catch (NullReferenceException e) {
            print("Train has reached the end of the track.");
            print(e.Message);
        }
        //print("FINAL cell destination " + final_cell_dest);
        return final_cell_dest;
    }

    // Start is called before the first frame update
    void Start()
    {
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        GameObject train_layer = GameObject.FindGameObjectsWithTag("train_layer")[0];
        if (train_layer != null)
        {
            tilemap = train_layer.GetComponent<Tilemap>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
