using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class GameManager : MonoBehaviour
{
    public Camera camera;
    // manage score, game state 

    const int cell_width = 1;
    const int cell_height = 1;

    public static GameObject Track_Layer;
    public static GameObject Structure;
    public static GameObject Base;
    public static GameObject Shipyard_Base;
    public static GameObject Shipyard_Track;
    public static GameObject Shipyard_Track2;
    public static GameObject exit_north;
    public static GameObject exit_south;
    public static GameObject exit_west;
    public static GameObject exit_east;

    public static VehicleManager vehicle_manager;
    public static CityManager city_manager;
    public static RouteManager route_manager;
    public static TrackManager track_manager;
    public static MenuManager menu_manager;

    public static bool city_menu_state = false;
    public static bool prev_city_menu_state = false;

    public static bool game_menu_state = true;
    public static bool prev_game_menu_state = true;
    public static int prev_train_list_length = 0;


    //public static StoreMenuManager game_menu_manager;

    public static bool shipyard_state;

    public List<GameObject> train_list; // list of trains inside the game view

    private void Awake()
    {
        train_list = new List<GameObject>();
        shipyard_state = false;
        Track_Layer = GameObject.Find("Track Layer");
        Structure = GameObject.Find("Structure");
        Base = GameObject.Find("Base");
        Shipyard_Base = GameObject.Find("Shipyard Base");
        Shipyard_Track = GameObject.Find("Shipyard Track");
        Shipyard_Track2 = GameObject.Find("Shipyard Track 2");
        exit_north = GameObject.Find("Shipyard Track Exit North");
        exit_south = GameObject.Find("Shipyard Track Exit South");
        exit_west = GameObject.Find("Shipyard Track Exit West");
        exit_east = GameObject.Find("Shipyard Track Exit East");
    }

    // Start is called before the first frame update
    void Start()
    {
        vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>(); ;
        city_manager = GameObject.Find("CityManager").GetComponent<CityManager>();
        track_manager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        menu_manager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        //game_menu_manager = GameObject.Find("Store Menu").GetComponent<StoreMenuManager>();
        switch_on_shipyard(false);
    }

    // Update is called once per frame
    void Update()
    {
        enable_train_for_screen();
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(camera.transform.position.z));
            Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
            Vector2 mouse_pos_2d = new Vector2(mouse_pos.x, mouse_pos.y);
            RaycastHit2D hit = Physics2D.Raycast(mouse_pos_2d, Vector2.zero);
            Vector2Int selected_tile = new Vector2Int((int)(mouse_pos.x / RouteManager.cell_width), (int)(mouse_pos.y / RouteManager.cell_width));
            if (hit.collider != null)
            {
                GameObject clicked_gameobject = hit.collider.gameObject;
                string object_name = clicked_gameobject.name.Replace("(Clone)", "");
                switch (object_name)
                {
                    case "ES":
                        break;
                    case "NE":
                        break;
                    case "WN":
                        break;
                    case "WS":
                        break;
                    case "hor":
                        break;
                    case "vert":
                        break;
                    case "train": // start/pause a train
                        //Train train_component = clicked_gameobject.GetComponent<Train>();
                        //train_component.change_motion(); 
                        break;
                    case "Structure": // if user clicks on city, create city menu
                        GameObject city_object = CityManager.get_city(selected_tile);
                        switch_on_shipyard(true);                        
                        city_manager.set_activated_city(city_object);
                        MenuManager.activate_handler(new List<GameObject> { MenuManager.shipyard_exit_menu });
                        break;
                    case "boxcar":
                        break;
                    default:
                        print("You did not click a gameobject. Object name is " + object_name);
                        break;
                }
            }
        }
    }

    public void enable_train_for_screen()
    {
        //rendering trains
        if (game_menu_state != prev_game_menu_state)
        {
            foreach (GameObject train in train_list) // hide or show trains depending on whether I'm in a game view
            {
                train.GetComponent<Train>().turn_on_train(game_menu_state);  // show trains and boxcars
            }
        }
        prev_game_menu_state = game_menu_state;
    }

    public void switch_on_shipyard(bool state)
    {

        Structure.GetComponent<TilemapCollider2D>().enabled = !state; // turn off colliders for city
        Shipyard_Base.GetComponent<TilemapRenderer>().enabled = state;
        Shipyard_Track.GetComponent<TilemapRenderer>().enabled = state;
        Shipyard_Track2.GetComponent<TilemapRenderer>().enabled = state;

        exit_east.GetComponent<TilemapRenderer>().enabled = state;
        exit_north.GetComponent<TilemapRenderer>().enabled = state;
        exit_south.GetComponent<TilemapRenderer>().enabled = state;
        exit_west.GetComponent<TilemapRenderer>().enabled = state;

        Track_Layer.GetComponent<TilemapRenderer>().enabled = !state;
        Structure.GetComponent<TilemapRenderer>().enabled = !state;
        Base.GetComponent<TilemapRenderer>().enabled = !state;

        game_menu_state = !state;
        city_menu_state = state;
    }
}
