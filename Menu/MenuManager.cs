using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;

public class MenuManager : EventDetector
{
    public Button store_btn;
    public Button shipyard_exit_btn;
    public Button exit_game_btn;
    public Button confirm_exit_btn;
    public Button deny_exit_btn;
    public GameObject blocking_canvas;
    protected StoreMenuManager store_menu_manager;
    public GameMenuManager game_menu_manager;
    public static CityMenuManager city_menu_manager;
    public static GameObject exit_confirm;
    public static GameObject store_menu;
    public GameObject start_menu;
    public static GameObject game_menu;
    public static GameObject review_menu;
    public static GameObject shipyard_exit_menu;
    public static GameObject game_icon_canvas;
    public static GameObject previous_menu;
    public static GameObject exit_bck;// = GameObject.Find("Exit Bck"); // just a blue background

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

    public Button play_btn;
    public Button tutorial_btn;
    public int screen_idx = 0; // track close screen for appropriate delay

    public static bool is_btn_active = true; // toggle false if tutorial is ON  and incorrect btn is pressed

    static List<GameObject> event_handler_list; // names of gameobjects that listen for events
    City city;

    static Camera camera;

    private void Awake()
    {
        city_menu_manager = GameObject.Find("Exit Bck").GetComponent<CityMenuManager>();
        exit_bck = GameObject.Find("Exit Bck");
        exit_confirm = GameObject.Find("Exit Confirmation");
        store_menu = GameObject.Find("Store Menu");
        game_menu = GameObject.Find("Game Menu");
        previous_menu = game_menu;
        review_menu = GameObject.Find("Review Canvas");
        shipyard_exit_menu = GameObject.Find("Exit Bar");
        game_icon_canvas = GameObject.Find("Iconic Canvas");
        store_menu_manager = store_menu.GetComponent<StoreMenuManager>();
        game_menu_manager = game_menu.GetComponent<GameMenuManager>();
        camera = GameObject.Find("Camera").GetComponent<Camera>();
        event_handler_list = new List<GameObject>();
        event_handler_list.Add(exit_confirm);
        event_handler_list.Add(store_menu);
        event_handler_list.Add(start_menu);
        event_handler_list.Add(game_menu);
        event_handler_list.Add(shipyard_exit_menu);
        event_handler_list.Add(game_icon_canvas);
        event_handler_list.Add(review_menu);
        //activate_start_menu_handler(); // activate the start menu
        play_btn.onClick.AddListener(activate_begin_game_handler);
        tutorial_btn.onClick.AddListener(activate_tutorial);
        store_btn.onClick.AddListener(delegate { activate_handler(new List<GameObject> { store_menu }); });
        shipyard_exit_btn.onClick.AddListener(turn_off_shipyard);
        exit_game_btn.onClick.AddListener(exit_game);
        confirm_exit_btn.onClick.AddListener(exit_final);
        deny_exit_btn.onClick.AddListener(return_to_game);
        //PauseManager.pause_game(true);
        blocking_canvas.SetActive(false);
        //test_pay.onClick.AddListener(pay_all);
        if (GameState.show_start_screen)
        {
            //start_menu.SetActive(true);
            activate_start_menu_handler();
            //activate_begin_game_handler();
        }
        else
        {
            activate_begin_game_handler();
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

    public static void exit_final()
    {
        GameManager.exit_game();
    }

    public void return_to_game()
    {
        PauseManager.pause_game(false);
        activate_default_handler();
    }

    public void exit_game()
    {
        if (GameManager.is_tutorial_mode)
        {
            GameManager.exit_game();
            return;
        }
        PauseManager.pause_game(true);
        activate_handler(new List<GameObject>() { exit_confirm }); 
    }

    public void turn_off_shipyard()
    {
        exit_bck.SetActive(false);
        GameManager.game_menu_state = true;
        print("turn OFF SHIPYARD, set game menu state true");
        GameManager.city_manager.set_activated_city(); // hide all the trains in the city
        GameObject.Find("GameManager").GetComponent<GameManager>().switch_on_shipyard(false);
        activate_default_handler();
    }

    public void activate_tutorial()
    {
        activate_begin_game_handler();
        GameManager.is_tutorial_mode = true;
        blocking_canvas.SetActive(true); // only makes a certain part of screen selectable
        StartCoroutine(GameManager.tutorial_manager.activate_next_tutorial_step());
    }

    public void activate_begin_game_handler()
    {
        //activates handlers for game screen
        PauseManager.pause_game(false);
        GameObject.Find("GameManager").GetComponent<GameManager>().close_shipyard_btn.SetActive(true);
        GameObject.Find("GameManager").GetComponent<BoxCollider2D>().enabled = true;
        activate_handler(new List<GameObject> { game_menu, game_icon_canvas });
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

        activate_handler(new List<GameObject> { game_menu, game_icon_canvas });
    }

    public void activate_handler(List<GameObject> menu)
    {
        //open one menu, set listeners from all other screens off
        //is_open stands for activating a screen versus closing the active one
        // check if following tutorial
        if (GameManager.is_tutorial_mode && menu.Contains(store_menu))
        {
            StartCoroutine(GameManager.tutorial_manager.activate_next_tutorial_step());
        }
        if (menu[0] == game_menu && (previous_menu == store_menu || previous_menu == review_menu)) // transition from a pause menu. otherwise no need to unpause the game
        {
            PauseManager.pause_game(false);
            GameManager.game_menu_state = true;
            print("set game menu state TRUE becuase prev menu is store menu/review menu");
        }
        if (menu[0] == store_menu || menu[0] == review_menu)
        {
            PauseManager.pause_game(true);
            GameManager.Structure.GetComponent<TilemapCollider2D>().enabled = false;
        }            
        else
        {
            GameManager.Structure.GetComponent<TilemapCollider2D>().enabled = true;
        }
        foreach (GameObject handler in event_handler_list)
        {
            if (menu.Contains(handler)) {
                handler.SetActive(true);
            }
            else { handler.SetActive(false); }
        }
        previous_menu = menu[0];
    }

    public static Vector3 convert_screen_to_world_coord(Vector3 position)
    {
        Vector3 world_position = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, Mathf.Abs(camera.transform.position.z)));
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
