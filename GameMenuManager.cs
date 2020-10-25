﻿using System.Collections;
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
    Tile clicked_tile;

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

    public override void OnBeginDrag(PointerEventData eventData)
    {
        item_name = eventData.pointerCurrentRaycast.gameObject.name;
        print("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
        Vector3 position = MenuManager.convert_screen_to_world_coord(eventData.position);
        clicked_tile = null; // reset this variable
        switch (item_name)
        {
            case "ES":
                clicked_item = Instantiate(es_curve, position, Quaternion.identity);
                clicked_tile = ES_tile;
                break;
            case "NE":
                clicked_item = Instantiate(ne_curve, position, Quaternion.identity);
                clicked_tile = NE_tile;
                break;
            case "WN":
                clicked_item = Instantiate(wn_curve, position, Quaternion.identity);
                clicked_tile = WN_tile;
                break;
            case "WS":
                clicked_item = Instantiate(ws_curve, position, Quaternion.identity);
                clicked_tile = WS_tile;
                break;
            case "hor":
                clicked_item = Instantiate(hor_track, position, Quaternion.identity);
                clicked_tile = hor_tile;
                break;
            case "vert":
                clicked_item = Instantiate(vert_track, position, Quaternion.identity);
                clicked_tile = vert_tile;
                break;
            case "boxcar":
                clicked_item = Instantiate(boxcar, position, Quaternion.identity);
                break;
            case "trainee": // tell the train where to go
                clicked_item = Instantiate(dummy_train, position, Quaternion.identity);
                break;
            default:
                print("This is not a draggable item");
                break;
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        try
        {
            Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
            clicked_item.transform.position = world_position;
        }
        catch (NullReferenceException e)
        {
            print("tried to drag something that is not draggable");
            print(e.Message);
        }
        catch (MissingReferenceException e)
        {
            print("Trying to access a destroyed object");
            print(e.Message);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // REVISE ME!!!
        VehicleManager vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
        TrackManager track_manager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        Vector3 final_world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
        Vector3Int final_tilemap_position = new Vector3Int((int)final_world_position.x, (int)final_world_position.y, (int)final_world_position.z);
        print("final tilemap position of tile is " + final_tilemap_position);
        try
        {
            string item_name = clicked_item.name.Replace("(Clone)", ""); // remove clone from the game object name
            if (item_name == "ES" || item_name == "NE" || item_name == "WN" || item_name == "WS" || item_name == "hor" || item_name == "vert")
            {
                track_manager.place_tile(final_tilemap_position, item_name, clicked_tile);
            }
            //else if (item_name == "dummy_train")
            //{
            //    // verify train is leaving from same city it arrived at
            //    // verify train's orientation matchces track direction
            //    GameObject city_object = train_menu_manager.get_city_object();
            //    City city = city_object.GetComponent<City>();
            //    RouteManager.Orientation orientation = RouteManager.get_start_orientation(final_tilemap_position, city);
            //    Train train = train_object.GetComponent<Train>();
            //    if (orientation != RouteManager.Orientation.None && train.all_boxcar_arrived()) // place vehicle if the train placement is valid and boxcars are not enroute
            //    {
            //        Vector3Int city_location = city.get_location();
            //        vehicle_manager.place_vehicle(city_location, train_object, orientation);
            //        vehicle_manager.depart_station(train, city_object, city);
            //        create_train_menu(city_object); // update train menu
            //    }
            //}
            if (clicked_item != null)
            {
                Destroy(clicked_item);
            }
        }
        catch (NullReferenceException e)
        {
            print(e.StackTrace);
        }
        catch (MissingReferenceException e)
        {
            print(e.Message); // tried to drag something that is not draggable
        }
    }

}