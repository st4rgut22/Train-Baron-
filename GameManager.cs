using System.Collections;
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
    public static GameObject building_lot_north_outer;
    public static GameObject building_lot_north_inner;
    public static GameObject building_lot_south_outer;
    public static GameObject building_lot_south_inner;
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
    public GameObject hint_tile_go;

    public static GameObject city_tilemap_go;
    public static GameObject undeveloped_land;

    public static int money;

    public Building building_expansion;

    public static bool shipyard_state;

    public List<GameObject> train_list; // list of trains inside the game view

    public static float tolerance = .004f;
    public static float speed = 1;

    public static GameObject reputation_text_go;
    public static GameObject star_review_image_go;

    public Tilemap offset_boxcar_tilemap; // saved offset tilemap

    public static GameObject traffic_tilemap_go;
    public static Tilemap traffic_tilemap;

    public static GameObject game_menu_manager;

    private void Awake()
    {
        traffic_tilemap_go = GameObject.Find("Traffic Light");
        traffic_tilemap = traffic_tilemap_go.GetComponent<Tilemap>();
        traffic_tilemap_go.SetActive(false);
        star_review_image_go = GameObject.Find("star_review");
        reputation_text_go = GameObject.Find("reputation_text");
        money = 5000;
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
        building_expansion = null;
        test_btn.onClick.AddListener(activate_train);
        city_tilemap_go = GameObject.Find("City Tilemap");
        undeveloped_land = GameObject.Find("Undeveloped Land");
        building_lot_north_outer = GameObject.Find("Building Lot North Outer");
        building_lot_north_inner = GameObject.Find("Building Lot North Inner");
        building_lot_south_outer = GameObject.Find("Building Lot South Outer");
        building_lot_south_inner = GameObject.Find("Building Lot South Inner");
        building_lot_west = GameObject.Find("Building Lot West");
        building_lot_east = GameObject.Find("Building Lot East");
        game_menu_manager = GameObject.Find("Game Menu");
    }

    // Start is called before the first frame update
    void Start()
    {
        vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>(); ;
        city_manager = GameObject.Find("CityManager").GetComponent<CityManager>();
        track_manager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        menu_manager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
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

    public Vector3Int get_selected_tile_for_boxcar(Vector3 pos)
    {
        Vector3 position = new Vector3(pos.x, pos.y, Mathf.Abs(camera.transform.position.z));
        Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
        Vector3Int tilemap_cell_pos = offset_boxcar_tilemap.WorldToCell(mouse_pos);
        return tilemap_cell_pos;
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
                    offset_boxcar_tilemap = cd.boxcar_tile_tracker[tilemap_pos];
                    if (!tilemap_list.Contains(offset_boxcar_tilemap))
                        tilemap_list.Add(offset_boxcar_tilemap);
                    if (offset_boxcar_tilemap == null) throw new Exception("boxcar location to tilemap dictionary should not be null at position " + tilemap_pos_arr);
                    offset_boxcar_tilemap.SetTile((Vector3Int)tilemap_pos, hint_tile);
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

    public List<Collider2D> get_all_collider(RaycastHit2D[] all_raycast_hit)
    {
        List<Collider2D> collider_tag = new List<Collider2D>();
        for (int r = 0; r < all_raycast_hit.Length; r++)
        {
            RaycastHit2D rh2d = all_raycast_hit[r];
            if (rh2d.collider != null)
            {
                collider_tag.Add(rh2d.collider);
            }
        }
        return collider_tag;
    }

    public List<string> get_collider_name_list(List<Collider2D> collider_list)
    {
        List<string> collider_string_list = new List<string>();
        foreach (Collider2D c in collider_list)
        {
            collider_string_list.Add(c.gameObject.tag);
        }
        return collider_string_list;
    }

    public Collider2D get_from_collider_list(string tag_name, List<Collider2D> collider_list)
    {
        int index = collider_list.FindIndex(a => a.gameObject.tag == tag_name);
        if (index != -1)
        {
            return collider_list[index];
        }
        else
        {
            throw new Exception(tag_name + " should be in collider list");
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        // use the previous hint context to do something, called on click anywhere in board 
        RaycastHit2D[] selected_object = get_all_object_at_cursor(Input.mousePosition);
        List<Collider2D> collider_list = get_all_collider(selected_object);
        List<string> collider_tag_list = get_collider_name_list(collider_list);
        if (hint_context_list.Contains("board")) // behaves different from other hint contexts because eligible tiles are offset from tilemap and must be found by cloesst distance
        {
            print("board from city to boxcar");
            if (collider_tag_list.Contains("boxcar")) // second condition checks if it is an eligible tile
            {
                Collider2D boxcar_collider = collider_list[0];
                Vector2Int unoffset_boxcar_city_location = (Vector2Int)boxcar_collider.GetComponent<Boxcar>().prev_tile_position;
                //Vector2Int offset_boxcar_city_location = new Vector2Int(unoffset_boxcar_city_location.x + 1, unoffset_boxcar_city_location.y + 1);
                int hint_index = is_selected_tile_in_context(unoffset_boxcar_city_location);
                if (hint_index != -1)
                {
                    Collider2D collider = get_from_collider_list("boxcar", collider_list);
                    GameObject boxcar_go = collider.gameObject;
                    CityManager.Activated_City_Component.board_train(boxcar_go, hint_tile_pos);
                }
            }
            else
            {
                print("boxcar tile is not clicked. abort board action");
            }
            StartCoroutine(clear_hint_list()); // clearn hint list so no other hints get triggered during executing of this hint
        }
        else // hhint index is the context of the action (board, unload etc. ). Check if selection is valid for prior hint set. 
        {
            Vector2Int selected_tile = get_selected_tile(Input.mousePosition);
            int hint_index = is_selected_tile_in_context(selected_tile); // get from offset grid for "board"
            if (hint_index != -1)
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
                    CityManager.Activated_City_Component.unload_train(hint_tile_go, selected_tile); // hint tile position is boxcar position
                }
                else if (hint_context == "park")
                {
                    print("park");
                    Boxcar boxcar = hint_gameobject.GetComponent<Boxcar>();
                    boxcar.city.place_boxcar_tile(hint_gameobject, selected_tile);
                    vehicle_manager.boxcar_fill_void(hint_gameobject); // move boxcars behind this one forward
                    boxcar.train.remove_boxcar(boxcar.boxcar_id);
                }
                else if (hint_context == "north exit" || hint_context == "east exit" || hint_context == "west exit" || hint_context == "south exit") // DEPART TRAIN
                {
                    bool all_aboard = hint_gameobject.GetComponent<Train>().is_all_boxcar_boarded();
                    if (all_aboard)
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
            else if (collider_list.Count > 0)
            {
                // call pointer events for boxcar and city from GameManager using Raycast data on the UI event detector
                hint_tile_pos = selected_tile;
                if (collider_tag_list.Contains("city_building")) // ADD A PERSON TO THE BOXCAR BOARD TRAIN HINT
                {
                    Collider2D collider = get_from_collider_list("city_building", collider_list);
                    collider.gameObject.GetComponent<CityDetector>().click_room(eventData);
                }
                else if (collider_tag_list.Contains("boxcar"))
                {
                    Collider2D collider = get_from_collider_list("boxcar", collider_list);
                    hint_tile_go = collider.gameObject;
                    collider.gameObject.GetComponent<Boxcar>().click_boxcar(eventData);
                }
                else if (collider_tag_list.Contains("train"))
                {

                    Collider2D collider = get_from_collider_list("train", collider_list);
                    if (collider.gameObject.GetComponent<Train>().city == CityManager.Activated_City_Component)
                    {
                        collider.gameObject.GetComponent<Train>().click_train(eventData);
                    }

                }
                else if (collider_tag_list.Contains("inventory"))
                {
                    Collider2D collider = get_from_collider_list("inventory", collider_list);
                    collider.gameObject.GetComponent<InventoryPusher>().click_inventory(eventData);
                }
                else if (collider_tag_list.Contains("traffic_light"))
                {
                    Collider2D collider = get_from_collider_list("traffic_light", collider_list);
                }
                else
                {
                    //invalid hint, clear hint list
                    StartCoroutine(clear_hint_list()); // clearn hint list so no other hints get triggered during executing of this hint
                }
            }
            else // not taking action or setting a new hint
            {
                StartCoroutine(clear_hint_list());
            }
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
                        //train_menu_manager.update_train_menu(activated_city);
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

    public void enable_vehicle_for_screen(GameObject vehicle_go)
    {
        //allow turn on vehicles individually instead of all trains. 
        // like enable_train_for_screen() except focus on vehicle exiting or entering the screen
        MovingObject moving_object = vehicle_go.GetComponent<MovingObject>();
        if (game_menu_state)
            if (!moving_object.in_city)
            {
                
                StartCoroutine(moving_object.switch_on_vehicle(true, is_delayed:true)); // delay
            }
            else { StartCoroutine(moving_object.switch_on_vehicle(false)); }
        if (city_menu_state)
        {
            StartCoroutine(moving_object.switch_on_vehicle(false));
            if (moving_object.in_city)
                if (moving_object.city == CityManager.Activated_City_Component)
                    StartCoroutine(moving_object.switch_on_vehicle(true));
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

        undeveloped_land.SetActive(state);
        building_lot_east.SetActive(state);
        building_lot_north_outer.SetActive(state);
        building_lot_north_inner.SetActive(state);
        building_lot_south_outer.SetActive(state);
        building_lot_south_inner.SetActive(state);
        building_lot_west.SetActive(state);

        traffic_tilemap_go.SetActive(state);

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