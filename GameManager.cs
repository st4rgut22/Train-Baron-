﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : EventDetector
{
    public static Camera camera;
    // manage score, game state 

    public static int macro_morale; // effected by health and economy
    public static int macro_health;
    public static int macro_economy;
    public static int day;

    public Tilemap hint_tilemap;
    public Tile hint_tile;

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
    public static GameObject building_lot_north_1;
    public static GameObject building_lot_north_2;
    public static GameObject building_lot_south_1;
    public static GameObject building_lot_south_2;
    public static GameObject building_lot_west;
    public static GameObject building_lot_east;

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

    public static List<string> hint_context_list;
    public static GameObject hint_gameobject;
    public static List<List<int[]>> hint_context_pos_list;
    public Vector2Int hint_tile_pos;

    public Building_Lot building_expansion;

    public static bool shipyard_state;

    public List<GameObject> train_list; // list of trains inside the game view

    public static float tolerance = .004f;
    public static float speed = 1;

    private void Awake()
    {
        train_list = new List<GameObject>();
        hint_context_list = new List<string>();
        hint_context_pos_list = new List<List<int[]>>();
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

        building_lot_north_1 = GameObject.Find("Building Lot North 1");
        building_lot_north_2 = GameObject.Find("Building Lot North 2");
        building_lot_south_1 = GameObject.Find("Building Lot South 1");
        building_lot_south_2 = GameObject.Find("Building Lot South 2");
        building_lot_west = GameObject.Find("Building Lot West");
        building_lot_east = GameObject.Find("Building Lot East");
        building_expansion = null;
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
        macro_morale = 50;
        macro_economy = 50;
        macro_health = 50;
        day = 1;
    }

    public void activate_train()
    {
        for (int i = 0; i < 8; i++) // TEMPORARY testing train display UI
        {
            vehicle_manager.create_vehicle_at_home_base();
        }
    }

    public static RaycastHit2D get_object_at_cursor(Vector3 cursor_pos)
    {
        Vector3 position = new Vector3(cursor_pos.x, cursor_pos.y, Mathf.Abs(camera.transform.position.z));
        Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
        Vector2 mouse_pos_2d = new Vector2(mouse_pos.x, mouse_pos.y);
        RaycastHit2D hit = Physics2D.Raycast(mouse_pos_2d, Vector2.zero);
        return hit;
    }

    public static RaycastHit2D[] get_all_object_at_cursor(Vector3 cursor_pos)
    {
        Vector3 position = new Vector3(cursor_pos.x, cursor_pos.y, Mathf.Abs(camera.transform.position.z));
        Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
        Vector2 mouse_pos_2d = new Vector2(mouse_pos.x, mouse_pos.y);
        RaycastHit2D[] hit = Physics2D.RaycastAll(mouse_pos_2d, Vector2.zero);
        return hit;
    }

    public static Vector2Int get_selected_tile(Vector3 pos)
    {
        Vector3 position = new Vector3(pos.x, pos.y, Mathf.Abs(camera.transform.position.z));
        Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
        Vector2Int selected_tile = new Vector2Int((int)(mouse_pos.x / RouteManager.cell_width), (int)(mouse_pos.y / RouteManager.cell_width));
        return selected_tile;
    }

    public static Vector3 get_world_pos(Vector3 pos)
    {
        Vector3 position = new Vector3(pos.x, pos.y, Mathf.Abs(camera.transform.position.z));
        Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
        return mouse_pos;
    }

    public void clean_up_hint(List<List<int[]>> tilemap_pos_list, List<Tilemap>tilemap_list)
    {
        foreach (Tilemap tilemap in tilemap_list)
        {
            foreach (List<int[]> tilemap_pos_arr_list in tilemap_pos_list)
            {
                foreach (int[] tilemap_pos_arr in tilemap_pos_arr_list)
                {
                    Vector2Int tilemap_pos = new Vector2Int(tilemap_pos_arr[0], tilemap_pos_arr[1]);
                    tilemap.SetTile((Vector3Int)tilemap_pos, null);
                }
            }
        }
    }

    public IEnumerator show_hint(List<List<int[]>> tilemap_pos_list)
    {
        //clean_up_hint(tilemap_pos_list); ???
        List<Tilemap> tilemap_list = new List<Tilemap>();
        foreach (List<int[]> tilemap_pos_arr_list in tilemap_pos_list)
        {
            foreach (int[] tilemap_pos_arr in tilemap_pos_arr_list)
            { 
                Vector2Int tilemap_pos = new Vector2Int(tilemap_pos_arr[0], tilemap_pos_arr[1]);
                if (hint_context_list.Contains("board"))
                {
                    CityDetector cd = GameObject.Find("City Tilemap").GetComponent<CityDetector>();// if hint is board use a different hint tilemap
                    Tilemap boxcar_tilemap = cd.boxcar_tile_tracker[tilemap_pos];
                    if (!tilemap_list.Contains(boxcar_tilemap))
                        tilemap_list.Add(boxcar_tilemap);
                    if (boxcar_tilemap == null) throw new Exception("boxcar location to tilemap dictionary should not be null at position " + tilemap_pos_arr);
                    boxcar_tilemap.SetTile((Vector3Int)tilemap_pos, hint_tile);
                }
                else
                {
                    tilemap_list.Add(hint_tilemap);
                    hint_tilemap.SetTile((Vector3Int)tilemap_pos, hint_tile);
                }
            }
        }
        yield return new WaitForSeconds(1);
        clean_up_hint(tilemap_pos_list, tilemap_list);
        // after 1 second, unhighlight the tiles
    }


    public void mark_tile_as_eligible(List<List<int[]>> tilemap_pos_list, List<string> hint_context, GameObject go, bool hint_mode = true) // turn off false for gameplay
    {
        // if hint mode is on, highlight the valid tiles
        hint_context_list = hint_context;
        hint_gameobject = go;
        hint_context_pos_list = tilemap_pos_list; // save the affected tiles to detect future clicks on these tiles
        if (hint_mode)
        {
            StartCoroutine(show_hint(tilemap_pos_list));
        }
    }

    public static int is_selected_tile_in_context(Vector2Int selected_tile)
    {
        // is the pressed tile receiving an action from the user (specified by the previous touch)?
        if (hint_context_list.Count > 0)
        {
            int[] selection = new int[] { selected_tile.x, selected_tile.y };
            for (int i=0;i<hint_context_pos_list.Count; i++)
            {
                List<int[]> hint_context = hint_context_pos_list[i];
                foreach (int[] coord in hint_context)
                {
                    //print("hint context coordinate is (" + coord[0] + "," + coord[1] + ") and selected tile is " + selected_tile);
                    if (coord[0] == selected_tile.x && coord[1] == selected_tile.y) return i;
                }
            }
        }
        return -1;
    }

    public GameObject get_collider_by_tag(string tag_name, RaycastHit2D[] hit_list)
    {
        foreach (RaycastHit2D rh2d in hit_list)
        {
            if (rh2d.collider != null && rh2d.collider.tag == tag_name)
            {
                return rh2d.collider.gameObject;
            }
        }
        return null;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        RaycastHit2D[] selected_objects = get_all_object_at_cursor(Input.mousePosition);
        GameObject big_lot_object = get_collider_by_tag("big_lot", selected_objects);
        if (big_lot_object != null)
        {
            string name = big_lot_object.name;
            print("dragging object " + name);
            Building_Lot bl = CityManager.Activated_City_Component.building_map[name];
            building_expansion = bl;
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // add new rooms
        if (building_expansion != null)
        {
            Vector2Int selected_tile = get_selected_tile(Input.mousePosition);
            CityManager.Activated_City_Component.expand_building(building_expansion, selected_tile);
        }
        building_expansion = null;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        // use the previous hint context to do something, called on click anywhere in board 
        Vector2Int selected_tile = get_selected_tile(Input.mousePosition);
        
        int hint_index = is_selected_tile_in_context(selected_tile);
        RaycastHit2D selected_object = get_object_at_cursor(Input.mousePosition);
        if (hint_context_list.Contains("board")) // behaves different from other hint contexts because eligible tiles are offset from tilemap and must be found by cloesst distance
        {
            print("board from city to boxcar");
            Vector3 world_pos = get_world_pos(Input.mousePosition);
            GameObject boxcar_go = GameObject.Find("City Tilemap").GetComponent<CityDetector>().is_boxcar_tile_clicked((Vector2)world_pos);
            if (boxcar_go != null)
                CityManager.Activated_City_Component.board_train(boxcar_go, hint_tile_pos);
            else
            {
                print("boxcar tile is not clicked. abort board action");
            }
            StartCoroutine(clear_hint_list()); // clearn hint list so no other hints get triggered during executing of this hint
        }
        else if (hint_index != -1) // hhint index is the context of the action (board, unload etc. )
        {
            string hint_context = hint_context_list[hint_index];
            if (hint_context == "add") // ADD BOXCAR
            {
                InventoryPusher ip = GameObject.Find("Shipyard Inventory").GetComponent<InventoryPusher>();
                city_manager.add_boxcar_to_station(ip.selected_tile.name, selected_tile, ip.selected_tile_pos);
            }
            else if (hint_context == "unload")
            {
                print("unload from boxcar to city");
                //Tile struct_tile = (Tile) GameObject.Find("City").GetComponent<Tilemap>().GetTile((Vector3Int)selected_tile);
                //List<Cargo> cargo_list = boxcar.retrieve_cargo();
            }
            else if (hint_context == "park")
            {
                print("park");
                Boxcar boxcar = hint_gameobject.GetComponent<Boxcar>();
                boxcar.city.place_boxcar_tile(hint_gameobject, selected_tile);
                GameManager.vehicle_manager.boxcar_fill_void(hint_gameobject); // move boxcars behind this one forward
                boxcar.train.remove_boxcar(boxcar.boxcar_id);
            }
            else if (hint_context == "north exit" || hint_context == "east exit" || hint_context == "west exit" || hint_context == "south exit") // DEPART TRAIN
            {
                hint_gameobject.GetComponent<Train>().exit_city(hint_context);
            }
            else if (hint_context == "track") // PLACE TILE
            {
                Tile clicked_tile = hint_gameobject.GetComponent<GameMenuManager>().clicked_tile;
                track_manager.place_tile(selected_tile, clicked_tile, true);
            }
            else
            {
                throw new Exception("not a valid hint context");
            }
            StartCoroutine(clear_hint_list()); // clearn hint list so no other hints get triggered during executing of this hint
        }
        else if (selected_object != null) // SET A NEW HINT
        {
            if (selected_object.collider != null)
            {
                // call pointer events for boxcar and city from GameManager using Raycast data on the UI event detector
                string object_name = selected_object.collider.tag;
                hint_tile_pos = selected_tile;
                if (object_name == "city_building") 
                {
                    selected_object.collider.gameObject.GetComponent<CityDetector>().click_city(eventData);
                }
                else if (object_name == "boxcar")
                {
                    selected_object.collider.gameObject.GetComponent<Boxcar>().click_boxcar(eventData);
                }
                else if (object_name == "train")
                {
                    selected_object.collider.gameObject.GetComponent<Train>().click_train(eventData);
                }
                else if (object_name == "inventory")
                {
                    selected_object.collider.gameObject.GetComponent<InventoryPusher>().click_inventory(eventData);
                }
                else
                {
                    //invalid hint, clear hint list
                    StartCoroutine(clear_hint_list()); // clearn hint list so no other hints get triggered during executing of this hint
                }
            }
        }
        else // not taking action or setting a new hint
        {
            StartCoroutine(clear_hint_list());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && hint_context_list.Count == 0)
        {
            Vector2Int selected_tile = get_selected_tile(Input.mousePosition);
            RaycastHit2D selected_object = get_object_at_cursor(Input.mousePosition);
            if (selected_object != null)
            {
                Collider2D collided_object = selected_object.collider;
                if (collided_object != null) // the first collided object will be in the clickDetector Layer, which sould be ignored for non-hint clicks
                {
                    GameObject clicked_gameobject = collided_object.gameObject;
                    string object_tag = clicked_gameobject.tag;
                    if (object_tag == "track_layer")
                    {
                        GameManager.track_manager.toggle_on_train_track(selected_tile);
                    }
                    else if (object_tag == "structure")
                    {
                        GameObject city_object = city_manager.get_city(selected_tile);
                        // display boxcars
                        switch_on_shipyard(true);
                        city_manager.set_activated_city(city_object);
                        MenuManager.activate_handler(new List<GameObject> { MenuManager.shipyard_exit_menu });
                        City activated_city = city_object.GetComponent<City>();
                        train_menu_manager.update_train_menu(activated_city);
                    }
                }
            }
        }

    }

    public static IEnumerator clear_hint_list()
    {
        // clear the hint list after all input events for this frame are over to avoid confusing the execution of a hint for the start of another one
        yield return new WaitForEndOfFrame();
        hint_context_list.Clear(); // after completing or failing to complete an action, remove context to prepare for new context
    }

    public static void enable_vehicle_for_screen(GameObject boxcar_object)
    {
        //allow turn on vehicles individually instead of all trains
        // like enable_train_for_screen() except focus on vehicle exiting or entering the screen
        MovingObject boxcar = boxcar_object.GetComponent<MovingObject>();
        if (game_menu_state)
            if (!boxcar.in_city)
                MovingObject.switch_sprite_renderer(boxcar_object, true);
            else { MovingObject.switch_sprite_renderer(boxcar_object, false); }
        if (city_menu_state)
        {
            MovingObject.switch_sprite_renderer(boxcar_object, false);
            if (boxcar.in_city)
                if (boxcar.city == CityManager.Activated_City_Component)
                    MovingObject.switch_sprite_renderer(boxcar_object, true);
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

        building_lot_east.SetActive(state);
        building_lot_north_1.SetActive(state);
        building_lot_north_2.SetActive(state);
        building_lot_south_1.SetActive(state);
        building_lot_south_2.SetActive(state);
        building_lot_west.SetActive(state);

        track_manager.bottom_tilemap_go_1.SetActive(!state);
        track_manager.bottom_tilemap_go_2.SetActive(!state);
        track_manager.bottom_tilemap_go_3.SetActive(!state);
        track_manager.bottom_tilemap_go_4.SetActive(!state);
        track_manager.bottom_tilemap_go_5.SetActive(!state);
        track_manager.top_tilemap_go.SetActive(!state);

        Structure.SetActive(!state);
        Base.SetActive(!state);

        game_menu_state = !state;
        city_menu_state = state;

        enable_train_for_screen(); // switch on trains if exit shipyard
    }
}