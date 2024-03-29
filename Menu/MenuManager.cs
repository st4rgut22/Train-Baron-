﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.SceneManagement;

public class MenuManager : EventDetector
{
    public Button store_btn;
    public Button shipyard_exit_btn;
    public Button confirm_exit_btn;
    public Button deny_exit_btn;

    public static GameObject blocking_canvas;

    protected StoreMenuManager store_menu_manager;
    public GameMenuManager game_menu_manager;
    public GameObject start_menu;

    public Texture empty_inventory_bubble;
    public Texture food_inventory_bubble;
    public Texture home_inventory_bubble;
    public Texture work_inventory_bubble;
    public Texture track_inventory_bubble;
    public Texture train_inventory_bubble;
    public Texture vacation_inventory_bubble;

    public Button test_pay;
    public Button test_unpay;

    public Color32 home_color = new Color32(254, 205, 173, 100);
    public Color32 work_color = new Color32(82, 213, 253, 100);
    public Color32 vacation_color = new Color32(176, 234, 133, 100);
    public Color32 food_color = new Color32(255, 141, 180, 100);
    public Color32 entrance_color = new Color32(117, 123, 209, 100);

    public static Button play_btn;
    public static Button tutorial_btn;
    public int screen_idx = 0; // track close screen for appropriate delay

    public static bool is_btn_active = true; // toggle false if tutorial is ON  and incorrect btn is pressed
    public static Button exit_game_btn;

    static List<GameObject> event_handler_list; // names of gameobjects that listen for events
    City city;

    public static MenuManager instance;

    static Camera camera;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            event_handler_list = new List<GameObject>();
            event_handler_list.Add(GameManager.exit_confirm);
            event_handler_list.Add(GameManager.store_menu);
            event_handler_list.Add(GameManager.start_menu);
            event_handler_list.Add(GameManager.game_menu);
            event_handler_list.Add(GameManager.shipyard_exit_menu);
            event_handler_list.Add(GameManager.game_icon_canvas);
            event_handler_list.Add(GameManager.review_menu);
            event_handler_list.Add(GameManager.win_screen);
            event_handler_list.Add(GameManager.lose_screen);
            play_btn = GameObject.Find("Play Btn").GetComponent<Button>();
            tutorial_btn = GameObject.Find("Tutorial Btn").GetComponent<Button>();
            play_btn.onClick.AddListener(start_game);
            tutorial_btn.onClick.AddListener(activate_tutorial);
            store_btn.onClick.AddListener(delegate { activate_handler(new List<GameObject> { GameManager.store_menu }); });
            GameManager.close_shipyard_btn.onClick.AddListener(turn_off_shipyard);
            confirm_exit_btn.onClick.AddListener(activate_start_menu_handler);
            deny_exit_btn.onClick.AddListener(return_to_game);
            exit_game_btn = GameObject.Find("Iconic Close Game Btn").GetComponent<Button>();
            exit_game_btn.onClick.AddListener(exit_game);
            // if found u u little poopoo
            //print(gameObject.name);
            blocking_canvas = GameObject.Find("Tutorial Canvas");
            blocking_canvas.SetActive(false);
            //blocking_canvas.SetActive(false);
            GameManager.close_shipyard_go.SetActive(false);

            store_menu_manager = GameManager.store_menu.GetComponent<StoreMenuManager>();
            game_menu_manager = GameManager.game_menu.GetComponent<GameMenuManager>();
            camera = GameObject.Find("Camera").GetComponent<Camera>();
            GameManager.instance.add_click_listeners(); // win play btn, lose play btn
            activate_start_menu_handler();
            DontDestroyOnLoad(transform.gameObject);
        }
        else if (instance != this)
        {
            Destroy(transform.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void add_inventory_texture(string item_name)
    {
        GameObject menu_go = gameObject.transform.Find(item_name).gameObject;
        RawImage raw = menu_go.GetComponent<RawImage>();
        if (item_name == "hor" || item_name == "WN" || item_name == "NE" || item_name == "WS" || item_name == "ES" || item_name == "vert")
        {
            menu_go.GetComponent<RawImage>().texture = track_inventory_bubble;
        }
        else if (item_name.Contains("home"))
        {
            menu_go.GetComponent<RawImage>().texture = home_inventory_bubble;
        }
        else if (item_name.Contains("vacation"))
        {
            menu_go.GetComponent<RawImage>().texture = vacation_inventory_bubble;
        }
        else if (item_name.Contains("work"))
        {
            menu_go.GetComponent<RawImage>().texture = work_inventory_bubble;
        }
        else if (item_name.Contains("food"))
        {
            menu_go.GetComponent<RawImage>().texture = food_inventory_bubble;
        }
        else if (item_name.Contains("train"))
        {
            menu_go.GetComponent<RawImage>().texture = train_inventory_bubble;
        }
        else // structure
        {
            if (item_name == "Apartment" || item_name == "Mansion")
            {
                menu_go.GetComponent<RawImage>().texture = home_inventory_bubble;
            }
            else if (item_name == "Factory" || item_name == "Business")
            {

                menu_go.GetComponent<RawImage>().texture = work_inventory_bubble;
            }
            else if (item_name == "Diner" || item_name == "Restaurant")
            {
                menu_go.GetComponent<RawImage>().texture = food_inventory_bubble;
            }
            else
            {
                throw new Exception("cannot find a structure with name " + item_name);
            }
        }
    }

    public void update_inventory()
    {
        List<GameObject> Track_Gameobject_List = new List<GameObject>();
        random_algos.dfs_find_child_objects(transform, Track_Gameobject_List, new string[] { "Text" });
        foreach (GameObject inventory_item in Track_Gameobject_List)
        {
            Text item_count = inventory_item.GetComponent<Text>();
            string item_name = inventory_item.transform.parent.name;
            if (item_name == "work" || item_name == "train" || item_name == "vacation" || item_name == "home" || item_name == "food")
                item_count.text = "x" + VehicleManager.get_vehicle_count(item_name).ToString();
            else if (item_name == "vert" || item_name == "hor" || item_name == "NE" || item_name == "WS" || item_name == "WN" || item_name == "ES")
                item_count.text = "x" + TrackManager.get_track_count(item_name).ToString();
            else if (item_name == "Restaurant" || item_name == "Factory" || item_name == "Apartment" || item_name == "Business" || item_name == "Mansion" || item_name == "Diner")
                item_count.text = "x" + CityManager.get_building_count(item_name).ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void return_to_game()
    {
        CityManager.update_total_people(0);
        PauseManager.pause_game(false);
        GameObject.Find("GameManager").GetComponent<BoxCollider2D>().enabled = true;
        activate_default_handler();
    }

    public void exit_game()
    {
        GameObject.Find("GameManager").GetComponent<BoxCollider2D>().enabled = false;
        if (GameManager.is_tutorial_mode)
        {
            blocking_canvas.SetActive(false);
            activate_start_menu_handler();
            return;
        }
        PauseManager.pause_game(true);     
        activate_handler(new List<GameObject>() { GameManager.exit_confirm });
    }

    public void turn_off_shipyard()
    {
        GameManager.exit_bck.SetActive(false);
        GameManager.game_menu_state = true;
        //print("turn OFF SHIPYARD, set game menu state true");
        CityManager.instance.set_activated_city(); // hide all the trains in the city
        GameManager.switch_on_shipyard(false);
        activate_default_handler();
    }

    public void activate_tutorial()
    {
        screen_idx = 0;
        GameManager.initialize();
        GameState.show_start_screen = false;
        GameManager.win_screen.SetActive(false);
        GameManager.lose_screen.SetActive(false);
        SceneManager.LoadScene("Train Game");
        SceneManager.sceneLoaded += tutorial_finished_loading;
    }

    void scene_finished_loading(Scene scene, LoadSceneMode mode)
    {
        GameManager.is_tutorial_mode = false;
        GameManager.instance.initialize_scene(); // first start game
        activate_begin_game_handler();
        SceneManager.sceneLoaded -= scene_finished_loading; // unsubscribe delegate
    }

    void tutorial_finished_loading(Scene scene, LoadSceneMode mode)
    {
        GameManager.is_tutorial_mode = true;
        GameManager.instance.initialize_scene(); // first start game
        activate_begin_game_handler();
        SceneManager.sceneLoaded -= tutorial_finished_loading; // unsubscribe delegate
    }

    public void start_game()
    {
        //print("game started");
        GameManager.initialize();
        GameState.show_start_screen = false;
        string level_name = GameState.get_level_name();
        GameManager.win_screen.SetActive(false);
        GameManager.lose_screen.SetActive(false);
        GameState.show_start_screen = false;
        SceneManager.LoadScene(level_name);
        SceneManager.sceneLoaded += scene_finished_loading;
    }

    public void activate_begin_game_handler()
    {
        //activates handlers for game screen
        //PauseManager.pause_game(false);
        GameObject.Find("GameManager").GetComponent<BoxCollider2D>().enabled = true;
        activate_handler(new List<GameObject> { GameManager.game_menu, GameManager.game_icon_canvas });
    }

    public void activate_start_menu_handler()
    {
        //activates handlers for game screen
        activate_handler(new List<GameObject> { start_menu });
    }

    public void activate_default_handler()
    {
        if (GameManager.is_tutorial_mode)
        {
            if (screen_idx == 0 || screen_idx == 2) // exit store / exit apartment
            {
                StartCoroutine(GameManager.tutorial_manager.activate_next_tutorial_step());
            }
            else if (screen_idx == 1)
            {
                StartCoroutine(GameManager.tutorial_manager.activate_next_tutorial_step(4)); // 5 seconds delayed from exiting shipyard to clicking on apartment to allow train time to arrives
            }
            screen_idx += 1;
        }

        activate_handler(new List<GameObject> { GameManager.game_menu, GameManager.game_icon_canvas });
    }

    public void activate_handler(List<GameObject> menu)
    {
        //open one menu, set listeners from all other screens off
        //is_open stands for activating a screen versus closing the active one
        // check if following tutorial
        if (GameManager.is_tutorial_mode && menu.Contains(GameManager.store_menu))
        {
            StartCoroutine(GameManager.tutorial_manager.activate_next_tutorial_step());
        }
        if (menu[0] == GameManager.game_menu && (GameManager.previous_menu == GameManager.store_menu || GameManager.previous_menu == GameManager.review_menu)) // transition from a pause menu. otherwise no need to unpause the game
        {
            CityManager.update_total_people(0);
            PauseManager.pause_game(false); // TODOED RETURN TO DELETE ME
            GameManager.game_menu_state = true;
        }
        if (menu[0] == GameManager.store_menu || menu[0] == GameManager.review_menu)
        {
            PauseManager.pause_game(true);
            GameManager.Structure.GetComponent<TilemapCollider2D>().enabled = false;
        }
        else if (menu[0] != start_menu)
        {
            GameManager.Structure.GetComponent<TilemapCollider2D>().enabled = true;
        }
        foreach (GameObject handler in event_handler_list)
        {
            if (menu.Contains(handler))
            {
                handler.SetActive(true);
            }
            else { handler.SetActive(false); }
        }
        if (menu[0] == start_menu)
        {
            GameState.show_start_screen = true;
            GameManager.initialize(); // select easy, medium or hard
        }
        GameManager.previous_menu = menu[0];
    }

    public static Vector3 convert_screen_to_world_coord(Vector3 position)
    {
        Vector3 world_position = GameManager.camera.ScreenToWorldPoint(new Vector3(position.x, position.y, Mathf.Abs(GameManager.camera.transform.position.z)));
        position.z = 0;
        return world_position;
    }

    public static void zero_margins(RectTransform rectTransform)
    {
        // anchors are set correctly, but margins are off. zero all margins to match anchor location
        RectTransformExtensions.SetLeft(rectTransform, 0);
        RectTransformExtensions.SetBottom(rectTransform, 0);
        RectTransformExtensions.SetRight(rectTransform, 0);
        RectTransformExtensions.SetTop(rectTransform, 0);
    }
}
