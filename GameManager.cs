using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static Camera camera;
    // manage score, game state 

    const int cell_width = 1;
    const int cell_height = 1;

    public static GameObject Structure;
    public static GameObject Base;
    public static GameObject Shipyard_Base;
    public static GameObject Shipyard_Track;
    public static GameObject Shipyard_Track2;
    public static GameObject Shipyard_Inventory;
    public static GameObject exit_north;
    public static GameObject exit_south;
    public static GameObject exit_west;
    public static GameObject exit_east;

    public static VehicleManager vehicle_manager;
    public static CityManager city_manager;
    public static RouteManager route_manager;
    public static TrackManager track_manager;
    public static MenuManager menu_manager;
    public static TrainMenuManager train_menu_manager;

    public static bool city_menu_state = false;
    public static bool prev_city_menu_state = false;

    public static bool game_menu_state = true;
    public static bool prev_game_menu_state = true;
    public static int prev_train_list_length = 0;
    public Button test_btn;
   
    //public static StoreMenuManager game_menu_manager;

    public static bool shipyard_state;

    public List<GameObject> train_list; // list of trains inside the game view

    private void Awake()
    {
        train_list = new List<GameObject>();
        train_menu_manager = GameObject.Find("Exit Bar").GetComponent<TrainMenuManager>();
        shipyard_state = false;
        Structure = GameObject.Find("Structure");
        Base = GameObject.Find("Base");
        camera = GameObject.Find("Camera").GetComponent<Camera>();
        Shipyard_Base = GameObject.Find("Shipyard Base");
        Shipyard_Track = GameObject.Find("Shipyard Track");
        Shipyard_Track2 = GameObject.Find("Shipyard Track 2");
        Shipyard_Inventory = GameObject.Find("Shipyard Inventory");
        exit_north = GameObject.Find("Shipyard Track Exit North");
        exit_south = GameObject.Find("Shipyard Track Exit South");
        exit_west = GameObject.Find("Shipyard Track Exit West");
        exit_east = GameObject.Find("Shipyard Track Exit East");
        test_btn.onClick.AddListener(activate_train);
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

    public void activate_train()
    {
        for (int i = 0; i < 8; i++) // TEMPORARY testing train display UI
        {
            vehicle_manager.create_vehicle_at_home_base();
        }
    }

    public static Collider2D get_object_at_cursor(Vector3 cursor_pos)
    {
        Vector3 position = new Vector3(cursor_pos.x, cursor_pos.y, Mathf.Abs(camera.transform.position.z));
        Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
        Vector2 mouse_pos_2d = new Vector2(mouse_pos.x, mouse_pos.y);
        RaycastHit2D hit = Physics2D.Raycast(mouse_pos_2d, Vector2.zero);
        return hit.collider;
    }

    public static Vector2Int get_selected_tile(Vector3 pos)
    {
        Vector3 position = new Vector3(pos.x, pos.y, Mathf.Abs(camera.transform.position.z));
        Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
        Vector2Int selected_tile = new Vector2Int((int)(mouse_pos.x / RouteManager.cell_width), (int)(mouse_pos.y / RouteManager.cell_width));
        return selected_tile;
    }

    // Update is called once per frame
    void Update()
    {
        //print("train list is this long " + train_list.Count);
        //enable_train_for_screen();
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D selected_object = get_object_at_cursor(Input.mousePosition);
            if (selected_object != null)
            {
                Vector2Int selected_tile;
                GameObject clicked_gameobject = selected_object.gameObject;
                string object_name = clicked_gameobject.name.Replace("(Clone)", "");
                print("selected " + object_name);
                if (object_name == "Track Layer" || object_name == "Track Layer 2" || object_name == "Track Layer 3")
                {
                    // "ES" || object_name == "NE" || object_name == "WN" || object_name == "WS" || object_name == "hor" || object_name == "vert"
                    selected_tile = get_selected_tile(Input.mousePosition);
                    GameManager.track_manager.toggle_on_train_track(selected_tile);
                }
                else if(object_name == "Structure")
                {
                    selected_tile = get_selected_tile(Input.mousePosition);
                    GameObject city_object = city_manager.get_city(selected_tile);
                    // display boxcars
                    switch_on_shipyard(true);
                    city_manager.set_activated_city(city_object);
                    MenuManager.activate_handler(new List<GameObject> { MenuManager.shipyard_exit_menu });
                    City activated_city = city_object.GetComponent<City>();
                    train_menu_manager.update_train_menu(activated_city);
                }
                else
                {
                    print("You did not click a gameobject. Object name is " + object_name);

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

        Structure.SetActive(!state); // turn off colliders for city
        Shipyard_Base.SetActive(state);
        Shipyard_Track.SetActive(state);
        Shipyard_Track2.SetActive(state);

        exit_east.SetActive(state);
        exit_north.SetActive(state);
        exit_south.SetActive(state);
        exit_west.SetActive(state);

        RouteManager.Track_Layer.SetActive(!state);
        RouteManager.Track_Layer_2.SetActive(!state);
        RouteManager.Track_Layer_3.SetActive(!state);

        Structure.SetActive(!state);
        Base.SetActive(!state);

        game_menu_state = !state;
        city_menu_state = state;
    }
}
