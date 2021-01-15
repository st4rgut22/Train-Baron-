using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StoreMenuManager : MonoBehaviour
{
    public Button close_btn;
    public Button buy_btn;
    public Dictionary<String, int> item_price_dict;
    public Text money_text;

    private void Awake()
    {
        item_price_dict = new Dictionary<string, int>() {
            { "train_desc", 200 },
            {"fast_train_desc", 400 },
            {"home_desc", 100 },
            {"food_desc", 100 },
            {"work_desc",300 },
            {"vacation_desc", 400 },
            {"vert",50 },
            {"hor",50 },
            {"WN", 50 },
            {"NE", 50 },
            {"WS",50 },
            {"ES", 50 },
            {"factory", 200 },
            {"scenery", 60 },
            {"restaurant", 150 },
            {"mansion", 300 },
            {"apartment", 100 },
            {"diner", 100 },            
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        close_btn.onClick.AddListener(close_menu);
        buy_btn.onClick.AddListener(buy_item);
        add_listener_to_all_btn();
        //initialize_bomb_boxcar(); //todo temporary! remove
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void update_money_text()
    {
        money_text.text = GameManager.money.ToString();
    }

    private void OnEnable() // update money when first visitingn the store
    {
        update_money_text();
    }

    void buy_item()
    {
        GameMenuManager game_menu_manager = MenuManager.game_menu.GetComponent<GameMenuManager>();
        bool is_money_sufficient = change_item_count();
        if (is_money_sufficient)
        {
            game_menu_manager.update_inventory(); // change game mneu item count
        }
    }

    void close_menu()
    {
        MenuManager.activate_default_handler(); // activates the game menu
    }

    void create_purchased_items(List<GameObject> text_game_objects)
    {
        foreach (GameObject go in text_game_objects)
        {
            // updates the count
            string count = go.GetComponent<Text>().text;
            string item_name = go.transform.parent.parent.gameObject.name;
            int item_count = Int16.Parse(count);
            for (int i = 0; i < item_count; i++)
            {
                if (item_name == "train_desc" || item_name == "fast_train_desc")
                    GameManager.vehicle_manager.create_vehicle_at_home_base();
                else if (item_name == "home_desc")
                {
                    GameManager.vehicle_manager.add_boxcar("home");
                }
                else if (item_name == "food_desc")
                {
                    GameManager.vehicle_manager.add_boxcar("food");
                }
                else if (item_name == "work_desc")
                {
                    GameManager.vehicle_manager.add_boxcar("work");
                }
                else if (item_name == "vacation_desc")
                    GameManager.vehicle_manager.add_boxcar("vacation");
                else if (item_name == "hor" || item_name == "WN" || item_name == "NE" || item_name == "WS" || item_name == "ES" || item_name == "vert")
                {
                    TrackManager.update_track_count(item_name, 1);
                    GameManager.game_menu_manager.GetComponent<GameMenuManager>().add_inventory_texture(item_name);
                }
                else
                {
                    CityManager.update_building_count(item_name, 1);
                    GameManager.game_menu_manager.GetComponent<GameMenuManager>().add_inventory_texture(item_name);
                }

            }
        }
    }

    bool change_item_count()
    {
        //update inventory with bought items
        List<GameObject> text_game_objects = new List<GameObject>();
        string[] find_name_list = new string[] { "count" };
        random_algos.dfs_find_child_objects(transform, text_game_objects, find_name_list);
        int total_price = 0;
        foreach (GameObject go in text_game_objects)
        {
            string count = go.GetComponent<Text>().text;
            string item_name = go.transform.parent.parent.gameObject.name;
            int item_count = Int16.Parse(count);
            total_price += item_count * item_price_dict[item_name];
        }
        print("total price is " + total_price);
        if (total_price <= GameManager.money)
        {
            create_purchased_items(text_game_objects);
            GameManager.money -= total_price;
            update_money_text();
            return true;
        }
        else
        {
            return false;
        }

    }

    void update_text(string item_name, Text item_count)
    {
        int cur_count = Int16.Parse(item_count.text);
        //update inventory with bought items
        if (item_name == "add")
            cur_count++;
        else if (item_name == "minus" && cur_count >= 0)
            cur_count--;
        //else
        //    print("UNSUPPORTED btn action");
        item_count.text = cur_count.ToString();
    }

    void add_listener_to_all_btn()
    {
        List<GameObject> btn_gameobjects = new List<GameObject>();
        string[] find_name_list = new string[] { "add", "minus" };
        random_algos.dfs_find_child_objects(transform, btn_gameobjects, find_name_list);
        foreach (GameObject btn_go in btn_gameobjects)
        {
            GameObject item = btn_go.transform.parent.parent.gameObject;
            Button btn = btn_go.GetComponent<Button>();
            Text btn_text = btn_go.transform.parent.Find("count").GetComponent<Text>();
            btn.onClick.AddListener(delegate { update_text(btn_go.name, btn_text); });
        }
    }
}
