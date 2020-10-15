using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MenuManager : EventDetector
{
    public GameObject ne_curve;
    public GameObject es_curve;
    public GameObject wn_curve;
    public GameObject ws_curve;
    public GameObject hor_track;
    public GameObject vert_track;
    public GameObject train;
    public GameObject boxcar;
    public Camera camera;

    public Tile ES_tile;
    public Tile NE_tile;
    public Tile WN_tile;
    public Tile hor_tile;
    public Tile WS_tile;
    public Tile vert_tile;

    string item_name;

    static GameObject train_menu;
    GameObject clicked_item;
    Tile clicked_tile;

    City city;

    //TODO: Split MenuManager into separate managers for Store Menu and Train Menu
    // Start is called before the first frame update
    void Start()
    {
        city = null;
        train_menu = GameObject.Find("Train Menu"); // just a blue background
        train_menu.SetActive(false);
    }

    public Vector3 convert_screen_to_world_coord(Vector3 position)
    {
        Vector3 world_position = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, Mathf.Abs(camera.transform.position.z)));
        position.z = 0;
        return world_position;
    }

    public string get_user_input(Dictionary<string, bool> exit_map)
    {
        // get user select N, E, W, S
        return "N";
    }

    public void zero_margins(RectTransform rectTransform)
    {
        // anchors are set correctly, but margins are off. zero all margins to match anchor location
        RectTransformExtensions.SetLeft(rectTransform, 0);
        RectTransformExtensions.SetBottom(rectTransform, 0);
        RectTransformExtensions.SetRight(rectTransform, 0);
        RectTransformExtensions.SetTop(rectTransform, 0);
    }

    public void create_train_menu(GameObject city_object)
    {
        //TODO: call in coroutine to update menu as trains arrive
        train_menu.SetActive(true); // display city menu
        TrainMenuManager train_menu_manager = train_menu.GetComponent<TrainMenuManager>();
        train_menu_manager.destroy_train_display();
        train_menu_manager.create_train_menu(city_object);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        VehicleManager vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
        GameObject clicked_object = eventData.pointerCurrentRaycast.gameObject;
        string item_name = clicked_object.name;
        if (item_name == "train")
        {
            vehicle_manager.create_vehicle_at_home_base();
            //clicked_object.GetComponent<SpriteRenderer>().enabled = false; // hide train sprite until vehicle departs

        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        item_name = eventData.pointerCurrentRaycast.gameObject.name;
        print("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
        Vector3 position = convert_screen_to_world_coord(eventData.position);
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
            default:
                print("This is not a draggable item");
                break;
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        try
        {
            Vector3 world_position = convert_screen_to_world_coord(eventData.position);
            clicked_item.transform.position = world_position;
        } catch(NullReferenceException e)
        {
            print("tried to drag something that is not draggable");
            print(e.Message);
        } catch(MissingReferenceException e)
        {
            print("Trying to access a destroyed object");
            print(e.Message);
        }    
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        VehicleManager vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
        TrackManager track_manager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        Vector3 final_world_position = convert_screen_to_world_coord(eventData.position);
        Vector3Int final_tilemap_position = new Vector3Int((int)final_world_position.x, (int)final_world_position.y, (int)final_world_position.z);
        print("final tilemap position of tile is " + final_tilemap_position);
        try
        {
            string item_name = clicked_item.name.Replace("(Clone)",""); // remove clone from the game object name
            if (item_name == "ES" || item_name == "NE" || item_name == "WN" || item_name == "WS" || item_name == "hor" || item_name == "vert")
            {
                track_manager.place_tile(final_tilemap_position, item_name, clicked_tile);
            }
            else if (item_name == "boxcar")
            {
                vehicle_manager.create_boxcar(final_tilemap_position);
            }
            if (clicked_item != null)
            {
                Destroy(clicked_item);
            }
        } catch (NullReferenceException e){
            print(e.StackTrace);
        } catch (MissingReferenceException e)
        {
            print(e.Message); // tried to drag something that is not draggable
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
