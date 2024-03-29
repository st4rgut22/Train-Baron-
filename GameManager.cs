﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : EventDetector
{
    public static Camera camera;
    // manage score, game state 
    public static int goal;

    public static int macro_morale; // effected by health and economy
    public static int macro_health;
    public static int macro_economy;
    public static int day;

    public Tilemap hint_tilemap;
    public Tile hint_tile;

    public static GameObject Structure;
    public static GameObject Base;
    public static GameObject Shipyard_Base;
    public static GameObject Shipyard_Base2;
    public static GameObject top_nature;
    public static GameObject medium_nature;
    public static GameObject bottom_nature;
    public static GameObject bottom_nature_2;
    public static GameObject bottom_nature_pass;
    public static GameObject Shipyard_Track;
    public static GameObject Shipyard_Track2;
    public static GameObject Shipyard_Inventory;
    public static GameObject exit_north;
    public static GameObject exit_south;
    public static GameObject exit_west;
    public static GameObject exit_east;
    public static GameObject building_lot_north;
    public static GameObject building_lot_south;
    public static GameObject building_lot_west;
    public static GameObject building_lot_east;
    public static GameObject close_shipyard_go;
    public static Button close_shipyard_btn;

    public static GameObject exit_bck;// = GameObject.Find("Exit Bck"); // just a blue background
    public static GameObject exit_confirm;
    public static GameObject store_menu;
    public static GameObject start_menu;
    public static GameObject game_menu;
    public static GameObject previous_menu;
    public static GameObject review_menu;
    public static GameObject shipyard_exit_menu;
    public static GameObject game_icon_canvas;

    public static GameObject Track_Layer;//DO THIS
    public static Tilemap track_tilemap;
    public static Tilemap shipyard_track_tilemap;
    public static Tilemap shipyard_track_tilemap2;
    public static Tilemap exit_north_tilemap;
    public static Tilemap exit_south_tilemap;
    public static Tilemap exit_east_tilemap;
    public static Tilemap exit_west_tilemap;

    public static RawImage win_star_img;
    public static RawImage lose_star_img;

    public static GameObject win_screen;
    public static GameObject lose_screen;

    public static GameObject win_play_btn;
    public static GameObject lose_play_btn;
    public static GameObject win_quit_btn;
    public static GameObject lose_quit_btn;

    public static GameObject game_complete_btn;

    public Text high_score_text;

    public static int west_bound = 0;
    public static int east_bound = 16;
    public static int north_bound = 9;
    public static int south_bound = 1;
    public static RouteManager route_manager;
    public static SoundManager sound_manager;

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
    public static Text game_money_text;
    public static Text egghead_total_text;

    public Building building_expansion;

    public static bool shipyard_state;

    public static List<GameObject> train_list; // list of trains inside the game view

    public static float tolerance = .004f;
    public static float speed = 1;

    public GameObject close_game;

    public static GameObject star_review_image_go;

    public Tilemap offset_boxcar_tilemap; // saved offset tilemap

    public static Button Easy_Btn;
    public static Button Medium_Btn;
    public static Button Hard_Btn;

    public static GameObject content_scroll;

    public static GameObject traffic_tilemap_go;
    public static Tilemap traffic_tilemap;
    public static Tilemap traffic_tilemap_offset_north;
    public static Tilemap traffic_tilemap_offset_south;
    public static Tilemap traffic_tilemap_offset_east;
    public static Tilemap traffic_tilemap_offset_west;
    public static GameObject traffic_tilemap_offset_north_go;
    public static GameObject traffic_tilemap_offset_south_go;
    public static GameObject traffic_tilemap_offset_east_go;
    public static GameObject traffic_tilemap_offset_west_go;
    public static Text notification_count_text;

    public static GameObject person_manager;
    public static GameObject game_menu_manager;
    public static GameObject scroll_handler;

    public static TutorialManager tutorial_manager;

    public static bool is_tutorial_mode;

    public static GameManager instance;

    public static GameObject tutorial_canvas;

    public static Text money_text;
    public static Text remainder_text;

    private void Awake()
    {
        PlayerPrefs.SetInt("level",5); // temporary. REMOVE
        if (instance == null)
        {
            instance = this;
            Easy_Btn = GameObject.Find("Easy Btn").GetComponent<Button>();
            Medium_Btn = GameObject.Find("Medium Btn").GetComponent<Button>();
            Hard_Btn = GameObject.Find("Hard Btn").GetComponent<Button>();
            money_text = GameObject.Find("Money Text").GetComponent<Text>();
            remainder_text = GameObject.Find("Remainder Text").GetComponent<Text>();

            Easy_Btn.onClick.AddListener(select_easy);
            Medium_Btn.onClick.AddListener(select_medium);
            Hard_Btn.onClick.AddListener(select_hard);

            sound_manager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            win_screen = GameObject.Find("Win");
            lose_screen = GameObject.Find("Lose");
            win_star_img = GameObject.Find("Win Star").GetComponent<RawImage>();
            lose_star_img = GameObject.Find("Lose Star").GetComponent<RawImage>();

            notification_count_text = GameObject.Find("MsgCount").GetComponent<Text>();
            tutorial_canvas = GameObject.Find("Tutorial Canvas");
            tutorial_manager = tutorial_canvas.GetComponent<TutorialManager>();
            exit_bck = GameObject.Find("Exit Bck");
            exit_confirm = GameObject.Find("Exit Confirmation");
            store_menu = GameObject.Find("Store Menu");
            start_menu = GameObject.Find("Start");
            game_menu = GameObject.Find("Game Menu");
            previous_menu = game_menu;
            Track_Layer = GameObject.Find("Top Track Layer");
            track_tilemap = Track_Layer.GetComponent<Tilemap>();
            Track_Layer = GameObject.Find("Top Track Layer");
            exit_north_tilemap = GameObject.Find("Shipyard Track Exit North").GetComponent<Tilemap>();
            exit_south_tilemap = GameObject.Find("Shipyard Track Exit South").GetComponent<Tilemap>();
            exit_west_tilemap = GameObject.Find("Shipyard Track Exit West").GetComponent<Tilemap>();
            exit_east_tilemap = GameObject.Find("Shipyard Track Exit East").GetComponent<Tilemap>();
            shipyard_track_tilemap = GameObject.Find("Shipyard Track").GetComponent<Tilemap>();
            shipyard_track_tilemap2 = GameObject.Find("Shipyard Track 2").GetComponent<Tilemap>();
            review_menu = GameObject.Find("Review Canvas");
            shipyard_exit_menu = GameObject.Find("Exit Bar");
            content_scroll = GameObject.Find("content");
            game_icon_canvas = GameObject.Find("Iconic Canvas");
            star_review_image_go = GameObject.Find("star_review");
            game_money_text = GameObject.Find("Game Money Text").GetComponent<Text>();
            egghead_total_text = GameObject.Find("Egghead Goal").GetComponent<Text>();
            traffic_tilemap_go = GameObject.Find("Traffic Light");
            traffic_tilemap_offset_north_go = GameObject.Find("Traffic Light Offset North");
            traffic_tilemap_offset_east_go = GameObject.Find("Traffic Light Offset East");
            traffic_tilemap_offset_west_go = GameObject.Find("Traffic Light Offset West");
            traffic_tilemap_offset_south_go = GameObject.Find("Traffic Light Offset South");
            traffic_tilemap_offset_north = traffic_tilemap_offset_north_go.GetComponent<Tilemap>();
            traffic_tilemap_offset_south = traffic_tilemap_offset_south_go.GetComponent<Tilemap>();
            traffic_tilemap_offset_east = traffic_tilemap_offset_east_go.GetComponent<Tilemap>();
            traffic_tilemap_offset_west = traffic_tilemap_offset_west_go.GetComponent<Tilemap>();
            camera = GameObject.Find("Camera").GetComponent<Camera>();
            Shipyard_Base = GameObject.Find("Shipyard Base");
            Shipyard_Base2 = GameObject.Find("Shipyard Base 2");
            Shipyard_Track = GameObject.Find("Shipyard Track");
            Shipyard_Track2 = GameObject.Find("Shipyard Track 2");
            Shipyard_Inventory = GameObject.Find("Shipyard Inventory");
            exit_north = GameObject.Find("Shipyard Track Exit North");
            exit_south = GameObject.Find("Shipyard Track Exit South");
            exit_west = GameObject.Find("Shipyard Track Exit West");
            exit_east = GameObject.Find("Shipyard Track Exit East");
            undeveloped_land = GameObject.Find("Undeveloped Land");
            close_shipyard_btn = GameObject.Find("Close Shipyard Btn").GetComponent<Button>();
            close_shipyard_go = GameObject.Find("Close Shipyard Menu");

            win_play_btn = GameObject.Find("Win Play Again");
            win_quit_btn = GameObject.Find("Win Quit");
            lose_play_btn = GameObject.Find("Lose Play Again");
            lose_quit_btn = GameObject.Find("Lose Quit");
            game_complete_btn = GameObject.Find("Final Win Btn");

            game_complete_btn.GetComponent<Button>().onClick.AddListener(quit);

            win_quit_btn.GetComponent<Button>().onClick.AddListener(quit);

            lose_quit_btn.GetComponent<Button>().onClick.AddListener(quit);

            building_lot_north = GameObject.Find("Building Lot North");
            building_lot_south = GameObject.Find("Building Lot South");
            building_lot_west = GameObject.Find("Building Lot West");
            building_lot_east = GameObject.Find("Building Lot East");

            close_game.GetComponent<Button>().onClick.AddListener(exit);
            initialize();
            DontDestroyOnLoad(transform.gameObject);            
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void add_click_listeners()
    {
        win_play_btn.GetComponent<Button>().onClick.AddListener(MenuManager.instance.start_game);
        lose_play_btn.GetComponent<Button>().onClick.AddListener(MenuManager.instance.start_game);
    }

    public static void initialize(string difficulty = null)
    {
        if (difficulty == null)
            if (PlayerPrefs.HasKey("difficulty"))
                difficulty = PlayerPrefs.GetString("difficulty");
            else
            {
                difficulty = "easy";
                Easy_Btn.Select();
                PlayerPrefs.SetString("difficulty",difficulty);
            }
        else
        {
            PlayerPrefs.SetString("difficulty", difficulty);
        }
        if (GameState.show_start_screen)
        {
            if (difficulty == "easy")
                Easy_Btn.Select();
            else if (difficulty == "medium")
                Medium_Btn.Select();
            else if (difficulty == "hard")
                Hard_Btn.Select();
            PlayerPrefs.SetString("difficulty", difficulty);
        }
        GameState.set_egghead_goal(); // initialize level if not done so.
    }

    public void initialize_scene()
    {
        // UPDATE ON SCENE LOAD
        Scene scene = SceneManager.GetActiveScene();
        print(scene.name);
        Structure = GameObject.Find("Structure");
        Base = GameObject.Find("Base");
        // DESTROY EVERY SCENE

        top_nature = GameObject.Find("Top Nature");
        medium_nature = GameObject.Find("Medium Nature");
        bottom_nature_pass = GameObject.Find("Bottom Nature");
        bottom_nature = GameObject.Find("Bottom Nature Blocking");
        bottom_nature_2 = GameObject.Find("Bottom Nature Blocking 2");
        city_tilemap_go = GameObject.Find("City Tilemap");
        money = 5000;

        money_text.text = money.ToString();
        remainder_text.text = money.ToString();
        game_menu_manager = GameObject.Find("Game Menu");
        person_manager = GameObject.Find("People Manager");
        scroll_handler = GameObject.Find("scrollHandler");

        traffic_tilemap_offset_north_go.SetActive(false);
        traffic_tilemap_offset_east_go.SetActive(false);
        traffic_tilemap_offset_west_go.SetActive(false);
        traffic_tilemap_offset_south_go.SetActive(false);

        game_money_text.text = money.ToString();
        notification_count_text.text = 0.ToString();
        scrollScript.instance.initialize();
        train_list = new List<GameObject>();
        hint_context_list = new List<string>();
        hint_context_pos_list = new List<List<int[]>>();
        shipyard_state = false;


        //is_tutorial_mode = false; // CHANGE

        win_screen.SetActive(false);
        lose_screen.SetActive(false);
        switch_on_shipyard(false);

        foreach (Transform child in content_scroll.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        GetComponent<BoxCollider2D>().enabled = true; // turn on blocking mask

        if (is_tutorial_mode)
        {
            MenuManager.blocking_canvas.SetActive(true);
        }
        else
        {
            MenuManager.blocking_canvas.SetActive(false);
        }
        TrackManager.instance.initialize();
        CityManager.instance.initialize();
        VehicleManager.instance.initialize();
        GameMenuManager.instance.initialize();
        CityMenuManager.instance.initialize();
        TutorialManager.instance.initialize();
    }

    public void exit()
    {
        Application.Quit();
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    public void select_easy()
    {
        Easy_Btn.Select();
        GameState.set_egghead_goal();
        initialize("easy");
    }

    public void select_medium()
    {
        Medium_Btn.Select();
        GameState.set_egghead_goal();
        initialize("medium");
    }

    public void select_hard()
    {
        Hard_Btn.Select();
        GameState.set_egghead_goal();
        initialize("hard");
    }

    public static void update_egghead_total(int total_people)
    {
        string goal = total_people.ToString() + "/" + GameState.egghead_goal.ToString();
        egghead_total_text.text = goal;
        if (total_people >= GameState.egghead_goal)
        {
            end_level(true);
        }
        else if (total_people == 0)
        {
            end_level(false);
        }
    }

    public void quit()
    {
        PauseManager.pause_game(true);
        GameObject.Find("GameManager").GetComponent<BoxCollider2D>().enabled = false;
        MenuManager.instance.activate_start_menu_handler();
    }

    public static void update_game_money_text(int delta_money)
    {
        money += delta_money;
        if (money < 0)
            end_level(false);
        game_money_text.text = money.ToString();
    }
    
    public static bool is_position_in_bounds(Vector2Int position)
    {
        if (position.x >= west_bound && position.x <= east_bound && position.y >= south_bound && position.y <= north_bound)
        {
            return true;
        }
        else
        {
            return false;
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

    public static void end_level(bool is_level_beaten)
    {
        Texture star_img = CityManager.home_base.get_star_image_from_reputation();
        int level = PlayerPrefs.GetInt("level");
        //print("level is " + level + " level list length is " + GameState.level_list.Length);
        GameObject start_obj = GameObject.Find("Start");
        GameObject lose_obj = GameObject.Find("Lose");
        GameObject win_obj = GameObject.Find("Win");
        if (start_obj == null && lose_obj == null && win_obj == null) // check if new level is already triggered
        {
            if (level == GameState.level_list.Length) // last level, start over
            {
                PlayerPrefs.SetInt("level", 0);
                game_complete_btn.SetActive(true);
                lose_quit_btn.SetActive(false);
                lose_play_btn.SetActive(false);
                win_quit_btn.SetActive(false);
                win_play_btn.SetActive(false);
            }
            else
            {
                lose_quit_btn.SetActive(true);
                lose_play_btn.SetActive(true);
                win_quit_btn.SetActive(true);
                win_play_btn.SetActive(true);
                game_complete_btn.SetActive(false);
            }
            if (is_level_beaten)
            {
                GameState.next_level();
                //print("level is beaten");
                win_screen.SetActive(true);
                win_star_img.texture = star_img;
            }
            else
            {
                //print("level is lost");
                lose_screen.SetActive(true);
                lose_star_img.texture = star_img;
            }
            GameObject.Find("GameManager").GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    public static void convert_star_to_profit(int star_review)
    {
        int profit = 0;
        switch (star_review)
        {
            case 0:
                profit = -75;
                break;
            case 1:
                profit = -25;
                break;
            case 2:
                profit = -10;
                break;
            case 3:
                profit = 10;
                break;
            case 4:
                profit = 25;
                break;
            case 5:
                profit = 50;
                break;
        }
        update_game_money_text(profit);
        //print("STAR TO PROFIT IS " + star_review + " and " + profit);
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
        base.OnPointerClick(eventData);
        // use the previous hint context to do something, called on click anywhere in board 
        RaycastHit2D[] selected_object = get_all_object_at_cursor(Input.mousePosition);
        List<Collider2D> collider_list = get_all_collider(selected_object);
        List<string> collider_tag_list = get_collider_name_list(collider_list);

        if (hint_context_list.Contains("board")) // behaves different from other hint contexts because eligible tiles are offset from tilemap and must be found by cloesst distance
        {
            if (collider_tag_list.Contains("boxcar")) // second condition checks if it is an eligible tile
            {
                if (is_tutorial_mode && !TutorialManager.board_flag)
                {
                    TutorialManager.board_flag = true;
                    StartCoroutine(tutorial_manager.activate_next_tutorial_step(6));
                }
                GameObject boxcar_go = CityManager.get_vehicle_in_activated_city(collider_list, "boxcar"); // get the right boxcar (if exists) if they are on top of each other
                if (boxcar_go != null)
                {
                    Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
                    if (boxcar.is_wait_for_turntable) // if user clicks on a leaving boxcar ignore
                    {
                        Vector2Int unoffset_boxcar_city_location = (Vector2Int)boxcar_go.GetComponent<Boxcar>().tile_position;
                        //print("board boxcar with tile position " + unoffset_boxcar_city_location);
                        int hint_index = is_selected_tile_in_context(unoffset_boxcar_city_location);
                        if (hint_index != -1)
                        {
                            CityManager.Activated_City_Component.board_train(boxcar_go, hint_tile_pos);
                        }
                    }
                }
            }
            StartCoroutine(clear_hint_list()); // clearn hint list so no other hints get triggered during executing of this hint
        }
        else // hhint index is the context of the action (board, unload etc. ). Check if selection is valid for prior hint set. 
        {
            Vector2Int selected_tile = get_selected_tile(Input.mousePosition);
            int hint_index = is_selected_tile_in_context(selected_tile); // get from offset grid for "board"
            if (hint_index != -1) // there is a hint. check if selection is valid
            {
                string hint_context = hint_context_list[hint_index];
                if (hint_context == "add") // ADD BOXCAR
                {
                    InventoryPusher ip = GameObject.Find("Shipyard Inventory").GetComponent<InventoryPusher>();
                    CityManager.instance.add_boxcar_to_station(ip.selected_tile.name, selected_tile, ip.selected_tile_pos);
                }
                else if (hint_context == "unload")
                {
                    //print("unload from boxcar to city");
                    if (is_tutorial_mode && !TutorialManager.unload_flag && !TutorialManager.step_in_progress)
                    {
                        TutorialManager.unload_flag = true;
                        StartCoroutine(tutorial_manager.activate_next_tutorial_step(5));
                    }                        
                    if (hint_tile_go.GetComponent<Boxcar>().is_wait_for_turntable)
                        CityManager.Activated_City_Component.unload_train(hint_tile_go, selected_tile); // hint tile position is boxcar position
                }
                else if (hint_context == "park")
                {
                    Boxcar boxcar = hint_gameobject.GetComponent<Boxcar>();
                    boxcar.city.place_boxcar_tile(boxcar.boxcar_type, (Vector3Int) selected_tile);
                    boxcar.city.add_boxcar_to_tilemap_with_location(boxcar.boxcar_type, selected_tile);
                    VehicleManager.instance.boxcar_fill_void(hint_gameobject); // move boxcars behind this one forward
                    boxcar.train.boxcar_squad.Remove(hint_gameobject);
                    Destroy(hint_gameobject);
                    //boxcar.train.remove_boxcar(boxcar.boxcar_id);
                }
                else if (hint_context == "north exit" || hint_context == "east exit" || hint_context == "west exit" || hint_context == "south exit") // DEPART TRAIN
                {
                    if (is_tutorial_mode && !TutorialManager.exit_flag && !TutorialManager.step_in_progress)
                    {
                        TutorialManager.exit_flag = true;
                        StartCoroutine(tutorial_manager.activate_next_tutorial_step(10));
                    }
                    bool all_aboard = hint_gameobject.GetComponent<Train>().is_all_boxcar_boarded();
                    if (all_aboard)
                        hint_gameobject.GetComponent<Train>().exit_city(hint_context);
                }
                else if (hint_context == "track") // PLACE TILE
                {
                    Tile clicked_tile = hint_gameobject.GetComponent<GameMenuManager>().clicked_tile;
                    TrackManager.instance.place_tile(selected_tile, clicked_tile, true);
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

                if (collider_tag_list.Contains("city_building") && CityManager.Activated_City_Component != null) // ADD A PERSON TO THE BOXCAR BOARD TRAIN HINT
                {
                    if (is_tutorial_mode && !TutorialManager.room_flag && !TutorialManager.step_in_progress)
                    {
                        TutorialManager.room_flag = true;
                        StartCoroutine(tutorial_manager.activate_next_tutorial_step());
                    }                        
                    Collider2D collider = get_from_collider_list("city_building", collider_list);
                    collider.gameObject.GetComponent<CityDetector>().click_room(eventData);
                }
                else if (collider_tag_list.Contains("boxcar") && CityManager.Activated_City_Component != null)
                {
                    if (is_tutorial_mode && !TutorialManager.boxcar_flag && !TutorialManager.step_in_progress)
                    {
                        StartCoroutine(tutorial_manager.activate_next_tutorial_step());
                    }                        
                    hint_tile_go = CityManager.get_vehicle_in_activated_city(collider_list, "boxcar");
                    if (hint_tile_go != null) hint_tile_go.GetComponent<Boxcar>().click_boxcar(eventData);
                }
                else if (collider_tag_list.Contains("train") && CityManager.Activated_City_Component != null)
                {
                    if (is_tutorial_mode && !TutorialManager.train_flag && !TutorialManager.step_in_progress)
                    {
                        TutorialManager.boxcar_flag = false;
                        TutorialManager.train_flag = true;
                        StartCoroutine(tutorial_manager.activate_next_tutorial_step());
                    }
                    hint_tile_go = CityManager.get_vehicle_in_activated_city(collider_list, "train");
                    if (hint_tile_go != null) hint_tile_go.GetComponent<Train>().click_train(eventData);
                }
                else if (collider_tag_list.Contains("inventory") && CityManager.Activated_City_Component != null)
                {
                    Collider2D collider = get_from_collider_list("inventory", collider_list);
                    collider.gameObject.GetComponent<InventoryPusher>().click_inventory(eventData);
                }
                else if (collider_tag_list.Contains("track_layer"))
                {
                    GameObject veh = VehicleManager.vehicle_board[selected_tile.x + 1, selected_tile.y + 1];
                    if (veh == null || veh.GetComponent<MovingObject>().is_pause || veh.GetComponent<MovingObject>().is_halt) // dont switch track as vehicle is passing over it
                        TrackManager.instance.toggle_on_train_track(selected_tile);
                }
                else if (collider_tag_list.Contains("structure"))
                {
                    //print("collider tag list includes structure");
                    if (is_tutorial_mode && !TutorialManager.step_in_progress)
                    {
                        if (!selected_tile.Equals(CityManager.home_base_location))
                            StartCoroutine(tutorial_manager.activate_next_tutorial_step());
                        else
                        {
                            StartCoroutine(tutorial_manager.activate_next_tutorial_step());
                        }
                    }                        
                    GameObject city_object = CityManager.instance.get_city(selected_tile);
                    // display boxcars
                    switch_on_shipyard(true);
                    CityManager.instance.set_activated_city(city_object);
                    MenuManager.instance.activate_handler(new List<GameObject> { shipyard_exit_menu });
                    if (city_object != CityManager.home_base.gameObject) // not home base, dont show vehicle creation options
                    {

                        CityMenuManager.instance.turn_of_vehicle_in_exit_bar(false);
                    }
                    else
                    {
                        CityMenuManager.instance.turn_of_vehicle_in_exit_bar(true);
                    }
                    CityMenuManager.instance.change_bck_color(city_object.GetComponent<City>().city_type);
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
                //if (moving_object.gameObject.tag == "boxcar")
                    //print("enable boxcar " + moving_object.gameObject.GetComponent<Boxcar>().boxcar_id + "  for screen line. GAME menu state is TRUE but moving object NOT IN city");
                //print("GAME MENU ON object NOT in city. TURN on VEHICLE");
                StartCoroutine(moving_object.switch_on_vehicle(true, is_delayed:true)); // delay
            }
            else {
                //print("GAME MENU OFF object IN city turn OFF vehicle");
                StartCoroutine(moving_object.switch_on_vehicle(false));
            }
        if (city_menu_state)
        {            
            if (moving_object.in_city && moving_object.city == CityManager.Activated_City_Component)
            {
                //print("CITY MENU ON object IN ACTIVATED city turn ON vehicle");
                StartCoroutine(moving_object.switch_on_vehicle(true));
            }
            else
            {
                //print("CITY MENU ON object not IN ACTIVATED city turn OFF vehicle");
                StartCoroutine(moving_object.switch_on_vehicle(false));
            }
        }
    }

    public static List<GameObject> get_all_train()
    {
        List<City> city_list = CityManager.city_list;
        List<GameObject> all_train_list = new List<GameObject>();
        all_train_list.AddRange(train_list);
        foreach (City city in city_list)
        {
            all_train_list.AddRange(city.train_list);
        }
        return all_train_list;
    }
     
    public static void enable_train_for_screen()
    {
        //rendering trains
        if (game_menu_state != prev_game_menu_state)
        {
            if (game_menu_state)
            {
                foreach (GameObject train in train_list) // hide or show trains depending on whether I'm in a game view
                {
                    train.GetComponent<Train>().turn_on_train(game_menu_state, true);  // show trains and boxcars
                }
            }
            else
            {
                List<GameObject> all_train_list = get_all_train();
                foreach (GameObject train in all_train_list) // hide or show trains depending on whether I'm in a game view
                {
                    train.GetComponent<Train>().turn_on_train(game_menu_state, false);  // TURN OFF ALL trains and boxcars including those in GAME VIEW and in OTHER CITIES
                }
            }
        }
        prev_game_menu_state = game_menu_state;
    }

    public static void switch_on_shipyard(bool state)
    {
        Structure.SetActive(!state); // turn off colliders for city
        Shipyard_Base.SetActive(state);
        Shipyard_Base2.SetActive(state);
        Shipyard_Track.SetActive(state);
        Shipyard_Track2.SetActive(state);

        exit_east.SetActive(state);
        exit_north.SetActive(state);
        exit_south.SetActive(state);
        exit_west.SetActive(state);

        undeveloped_land.SetActive(state);
        building_lot_east.SetActive(state);
        building_lot_north.SetActive(state);
        building_lot_south.SetActive(state);
        building_lot_west.SetActive(state);

        //traffic_tilemap_go.SetActive(state);
        traffic_tilemap_offset_east_go.SetActive(state);
        traffic_tilemap_offset_north_go.SetActive(state);
        traffic_tilemap_offset_west_go.SetActive(state);
        traffic_tilemap_offset_south_go.SetActive(state);

        close_shipyard_go.SetActive(state);

        top_nature.SetActive(!state);
        medium_nature.SetActive(!state);
        bottom_nature.SetActive(!state);

        if (bottom_nature_2 != null)
            bottom_nature_2.SetActive(!state);

        TrackManager.instance.bottom_tilemap_go_1.SetActive(!state);
        TrackManager.instance.bottom_tilemap_go_2.SetActive(!state);
        TrackManager.instance.bottom_tilemap_go_3.SetActive(!state);
        TrackManager.instance.bottom_tilemap_go_4.SetActive(!state);
        TrackManager.instance.bottom_tilemap_go_5.SetActive(!state);
        TrackManager.instance.top_tilemap_go.SetActive(!state);

        Structure.SetActive(!state);
        Base.SetActive(!state);

        game_menu_state = !state;
        city_menu_state = state;

        if (state)
        {
            exit_bck.SetActive(true); // show the star reviews
        }

        enable_train_for_screen(); // switch on trains if exit shipyard
    }
}