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
    public List<GameObject> text_game_objects;
    public Text cost_text;
    public Text remainder_text;
    public int total_cost;
    public Dictionary<string, int> shopping_cart;
    private void Awake()
    {
        shopping_cart = new Dictionary<string, int>();
        total_cost = 0;
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
            {"Factory", 200 },
            {"Scenery", 60 },
            {"Restaurant", 150 },
            {"Mansion", 300 },
            {"Apartment", 100 },
            {"Diner", 100 },
            {"Business", 300 }
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        close_btn.onClick.AddListener(close_menu);
        buy_btn.onClick.AddListener(buy_item);
        add_listener_to_all_btn();
        string[] find_name_list = new string[] { "count" };
        random_algos.dfs_find_child_objects(transform, text_game_objects, find_name_list);
        //initialize_bomb_boxcar(); //todo temporary! remove
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void add_item_to_cart(string item)
    {
        if (!shopping_cart.ContainsKey(item))
        {
            shopping_cart[item] = 1;
        }
        else
        {
            shopping_cart[item] += 1;
        }
    }

    void remove_item_from_cart(string item)
    {
        shopping_cart[item] -= 1;
    }

    void update_money_text()
    {
        money_text.text = GameManager.money.ToString();
        GameManager.update_game_money_text(0);       
    }

    private void OnEnable() // update money when first visitingn the store
    {

    }

    void buy_item()
    {
        MenuManager.is_btn_active = true; // keep true for follow up action (closing menu)
        GameMenuManager game_menu_manager = MenuManager.game_menu.GetComponent<GameMenuManager>();
        bool is_money_sufficient = change_item_count();
        if (is_money_sufficient)
        {
            game_menu_manager.update_inventory(); // change game mneu item count\
            MenuManager.city_menu_manager.update_inventory();
        }
        update_money_text();
        close_menu();
    }

    void close_menu()
    {
        reset_count();
        //activates handlers for game screen
        GameManager.menu_manager.activate_default_handler(); // activates the game menu
    }

    void create_purchased_items(List<GameObject> text_game_objects)
    {
        foreach (GameObject go in text_game_objects)
        {
            // updates the count
            string count = go.GetComponent<Text>().text;
            string item_name = go.transform.parent.parent.gameObject.name;
            int item_count = Int16.Parse(count);
            if (item_name.Contains("_desc"))
                item_name = item_name.Replace("_desc", "");
            for (int i = 0; i < item_count; i++)
            {
                if (item_name == "train" || item_name == "fast_train" || item_name == "home" || item_name == "food" || item_name == "work" || item_name == "vacation")
                {
                    VehicleManager.update_vehicle_count(item_name, 1);
                    MenuManager.city_menu_manager.add_inventory_texture(item_name);
                }
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
        if (total_cost <= GameManager.money)
        {
            create_purchased_items(text_game_objects);
            GameManager.money -= total_cost;
            //update_money_text();
            return true;
        }
        else
        {
            return false;
        }

    }

    void reset_count()
    {
        shopping_cart.Clear();
        total_cost = 0;
        foreach (GameObject go in text_game_objects)
        {
            go.GetComponent<Text>().text = "0";
        }
        remainder_text.text = GameManager.money.ToString();
        cost_text.text = "- 0";

    }

    void update_text(GameObject btn_go, Text item_count)
    {
        if (GameManager.is_tutorial_mode)
            StartCoroutine(GameManager.tutorial_manager.activate_next_tutorial_step());
        string btn_name = btn_go.name;
        string item_name = btn_go.transform.parent.parent.gameObject.name;
        int cur_count = Int16.Parse(item_count.text);
        //update inventory with bought items
        if (btn_name == "add")
        {
            if (item_price_dict[item_name] + total_cost > GameManager.money) // insufficient funds
                return;
            total_cost += item_price_dict[item_name];
            add_item_to_cart(item_name);
            cur_count++;
        }
        else if (btn_name == "minus" && cur_count > 0)
        {
            total_cost -= item_price_dict[item_name];
            remove_item_from_cart(item_name);
            cur_count--;
        }
        item_count.text = cur_count.ToString();
        cost_text.text = "- " + total_cost.ToString();
        int remainder = GameManager.money - total_cost;
        remainder_text.text = remainder.ToString();
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
            btn.onClick.AddListener(delegate { update_text(btn_go, btn_text); });
        }
    }
}
