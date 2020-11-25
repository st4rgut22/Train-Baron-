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

    public static int macro_morale; // effected by health and economy
    public static int macro_health;
    public static int macro_economy;
    public static int day;

    const int cell_width = 1;
    const int cell_height = 1;
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
    //public static StoreMenuManager game_menu_manager;

    public static bool shipyard_state;

    public List<GameObject> train_list; // list of trains inside the game view

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

    public void clean_up_hint(List<List<int[]>> tilemap_pos_list)
    {
        foreach (List<int[]> tilemap_pos_arr_list in tilemap_pos_list)
        {
            foreach (int[] tilemap_pos_arr in tilemap_pos_arr_list)
            {
                Vector2Int tilemap_pos = new Vector2Int(tilemap_pos_arr[0], tilemap_pos_arr[1]);
                hint_tilemap.SetTile((Vector3Int)tilemap_pos, null);
            }
        }
    }

    public IEnumerator show_hint(List<List<int[]>> tilemap_pos_list)
    {
        clean_up_hint(tilemap_pos_list);
        foreach (List<int[]> tilemap_pos_arr_list in tilemap_pos_list)
        {
            foreach (int[] tilemap_pos_arr in tilemap_pos_arr_list)
            {
                Vector2Int tilemap_pos = new Vector2Int(tilemap_pos_arr[0], tilemap_pos_arr[1]);
                hint_tilemap.SetTile((Vector3Int)tilemap_pos, hint_tile);
            }
        }
        yield return new WaitForSeconds(1);
        clean_up_hint(tilemap_pos_list);
        // after 1 second, unhighlight the tiles
    }


    public void mark_tile_as_eligible(List<List<int[]>> tilemap_pos_list, List<string> hint_context, GameObject go, bool hint_mode = true) // turn off false for gameplay
    {
        // if hint mode is on, highlight the valid tiles
        GameManager.hint_context_list = hint_context;
        GameManager.hint_gameobject = go;
        GameManager.hint_context_pos_list = tilemap_pos_list; // save the affected tiles to detect future clicks on these tiles
        if (hint_mode)
        {
            StartCoroutine(show_hint(tilemap_pos_list));
        }
    }

    public int is_selected_tile_in_context(Vector2Int selected_tile)
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
                    if (coord[0] == selected_tile.x && coord[1] == selected_tile.y) return i;
                }
            }
        }
        return -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D selected_object = get_object_at_cursor(Input.mousePosition);
            Vector2Int selected_tile = get_selected_tile(Input.mousePosition);
            print("selected tile is " + selected_tile);
            int hint_index = is_selected_tile_in_context(selected_tile);
            if (hint_index != -1)
            {
                string hint_context = hint_context_list[hint_index];
                if (hint_context == "add") // ADD BOXCAR
                {
                    print("add");
                    InventoryPusher ip = GameObject.Find("Shipyard Inventory").GetComponent<InventoryPusher>();
                    city_manager.add_boxcar_to_station(ip.selected_tile.name, selected_tile, ip.selected_tile_pos);
                }
                else if (hint_context == "unload" || hint_context == "park") // UNLOAD OR PARK BOXCAR
                {
                    Boxcar boxcar = hint_gameobject.GetComponent<Boxcar>();
                    if (hint_context == "unload")
                    {
                        print("unload");
                    }
                    if (hint_context == "park")
                    {
                        print("park");
                        //todo
                        boxcar.city.place_boxcar_tile(hint_gameobject, selected_tile);
                        GameManager.vehicle_manager.boxcar_fill_void(hint_gameobject); // move boxcars behind this one forward
                        boxcar.train.remove_boxcar(boxcar.boxcar_id);
                    }
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
            }
            else
            {
                if (selected_object != null)
                {
                    GameObject clicked_gameobject = selected_object.gameObject;
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
            hint_context_list.Clear(); // after completing or failing to complete an action, remove context to prepare for new context
        }
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