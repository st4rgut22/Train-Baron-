using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
public class GameMenuManager : EventDetector
{
    public GameObject ne_curve;
    public GameObject es_curve;
    public GameObject wn_curve;
    public GameObject ws_curve;
    public GameObject hor_track;
    public GameObject vert_track;
    public GameObject dummy_train; // train sprite without train script for visual purposes
    public GameObject boxcar;
    public Camera camera;

    public Tile ES_tile;
    public Tile NE_tile;
    public Tile WN_tile;
    public Tile hor_tile;
    public Tile WS_tile;
    public Tile vert_tile;
    public Tile clicked_tile;

    string item_name;

    GameObject clicked_item;
    GameObject train_menu;

    protected GameObject train_object; // the train is referenced in TrainDisplay which has MenuManager as a base class

    City city;
    void Start()
    {
        camera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void update_track_inventory()
    {
        List<GameObject> Track_Gameobject_List = new List<GameObject>();
        random_algos.dfs_find_child_objects(transform, Track_Gameobject_List, new string[] { "Text" });
        foreach (GameObject track_object in Track_Gameobject_List)
        {
            Text track_object_text = track_object.GetComponent<Text>();
            string track_name = track_object.transform.parent.name;
            switch (track_name)
            {
                case "vert":
                    track_object_text.text = "x" + TrackManager.vert_count.ToString();
                    break;
                case "hor":
                    track_object_text.text = "x" + TrackManager.hor_count.ToString();
                    break;
                case "NE":
                    track_object_text.text = "x" + TrackManager.ne_count.ToString();
                    break;
                case "WS":
                    track_object_text.text = "x" + TrackManager.ws_count.ToString();
                    break;
                case "WN":
                    track_object_text.text = "x" + TrackManager.wn_count.ToString();
                    break;
                case "ES":
                    track_object_text.text = "x" + TrackManager.es_count.ToString();
                    break;
                default:
                    break;
            }
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        List<List<int[]>> track_action_coord = new List<List<int[]>>();
        List<string> track_hint_list = new List<string>();
        item_name = eventData.pointerCurrentRaycast.gameObject.name;
        Tilemap structure_tilemap = GameManager.Structure.GetComponent<Tilemap>();
        List<int[]> track_action_list = new List<int[]>();
        track_hint_list.Add("track");
        switch (item_name)
        {
            case "ES":
                clicked_tile = ES_tile;
                break;
            case "NE":
                clicked_tile = NE_tile;
                break;
            case "WN":
                clicked_tile = WN_tile;
                break;
            case "WS":
                clicked_tile = WS_tile;
                break;
            case "hor":
                clicked_tile = hor_tile;
                break;
            case "vert":
                clicked_tile = vert_tile;
                break;
            default:
                print("not a valid track selected in game menu manager");
                break;
        }
        for (int i=0; i < BoardManager.track_width; i++)
        {
            for (int j = 0; j < BoardManager.track_height; j++)
            {
                Tile struct_tile = (Tile)structure_tilemap.GetTile(new Vector3Int(i, j, 0));
                if (struct_tile == null) // if no city on this tile you can place it here
                {
                    track_action_list.Add(new int[] { i, j });
                }
            }
        }
        track_action_coord.Add(track_action_list);
        GameObject.Find("GameManager").GetComponent<GameManager>().mark_tile_as_eligible(track_action_coord, track_hint_list, gameObject);
    }

    //public override void OnBeginDrag(PointerEventData eventData)
    //{
    //    try
    //    {
    //        item_name = eventData.pointerCurrentRaycast.gameObject.name;
    //        Vector3 position = MenuManager.convert_screen_to_world_coord(eventData.position);
    //        clicked_tile = null; // reset this variable
    //        switch (item_name)
    //        {
    //            case "ES":
    //                clicked_item = Instantiate(es_curve, position, Quaternion.identity);
    //                clicked_tile = ES_tile;
    //                break;
    //            case "NE":
    //                clicked_item = Instantiate(ne_curve, position, Quaternion.identity);
    //                clicked_tile = NE_tile;
    //                break;
    //            case "WN":
    //                clicked_item = Instantiate(wn_curve, position, Quaternion.identity);
    //                clicked_tile = WN_tile;
    //                break;
    //            case "WS":
    //                clicked_item = Instantiate(ws_curve, position, Quaternion.identity);
    //                clicked_tile = WS_tile;
    //                break;
    //            case "hor":
    //                clicked_item = Instantiate(hor_track, position, Quaternion.identity);
    //                clicked_tile = hor_tile;
    //                break;
    //            case "vert":
    //                clicked_item = Instantiate(vert_track, position, Quaternion.identity);
    //                clicked_tile = vert_tile;
    //                break;
    //            case "boxcar":
    //                clicked_item = Instantiate(boxcar, position, Quaternion.identity);
    //                break;
    //            case "trainee": // tell the train where to go
    //                clicked_item = Instantiate(dummy_train, position, Quaternion.identity);
    //                break;
    //            default:
    //                break;
    //        }
    //    } catch (NullReferenceException)
    //    {
    //        print("null");
    //    }

    //}

    //public override void OnDrag(PointerEventData eventData)
    //{
    //    try
    //    {
    //        Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
    //        clicked_item.transform.position = world_position;
    //    }
    //    catch (NullReferenceException e)
    //    {
    //        //print("tried to drag something that is not draggable");
    //        print(e.Message);
    //    }
    //    catch (MissingReferenceException e)
    //    {
    //        //print("Trying to access a destroyed object");
    //        print(e.Message);
    //    }
    //}

    //public override void OnEndDrag(PointerEventData eventData)
    //{
    //    // REVISE ME!!!
    //    VehicleManager vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
    //    TrackManager track_manager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
    //    Vector2Int final_tilemap_position = GameManager.get_selected_tile(eventData.position);
    //    try
    //    {
    //        string item_name = clicked_item.name.Replace("(Clone)", ""); // remove clone from the game object name
    //        if (item_name == "ES" || item_name == "NE" || item_name == "WN" || item_name == "WS" || item_name == "hor" || item_name == "vert")
    //        {
    //            track_manager.place_tile(final_tilemap_position, clicked_tile, true);
    //        }
    //        if (clicked_item != null)
    //        {
    //            Destroy(clicked_item);
    //        }
    //    }
    //    catch (NullReferenceException e)
    //    {
    //        print(e.StackTrace);
    //    }
    //    catch (MissingReferenceException e)
    //    {
    //        print(e.Message); // tried to drag something that is not draggable
    //    }
    //}

}
