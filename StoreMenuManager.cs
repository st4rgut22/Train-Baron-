using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StoreMenuManager : MonoBehaviour
{
    public Button close_btn;

    // Start is called before the first frame update
    void Start()
    {
        close_btn.onClick.AddListener(close_menu);
        add_listener_to_all_btn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void close_menu()
    {
        GameMenuManager game_menu_manager = MenuManager.game_menu.GetComponent<GameMenuManager>();
        change_item_count();
        //reset item count
        game_menu_manager.update_track_inventory();
        MenuManager.activate_default_handler();
        GameManager.game_menu_state = true;

        // TESTING
        //GameManager game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //GameObject city_object = CityManager.get_city(new Vector2Int(3, 6));
        //game_manager.switch_on_shipyard(true);
        //GameManager.city_manager.set_activated_city(city_object);
    }

    void change_item_count()
    {
        //update inventory with bought items
        List<GameObject> text_game_objects = new List<GameObject>();
        string[] find_name_list = new string[] { "count" };
        random_algos.dfs_find_child_objects(transform, text_game_objects, find_name_list);
        foreach (GameObject go in text_game_objects)
        {
            string count = go.GetComponent<Text>().text;
            string item_name = go.transform.parent.parent.gameObject.name;            
            for (int i = 0; i < Int16.Parse(count); i++)
            {
                switch (item_name)
                {
                    case "train":
                        GameManager.vehicle_manager.create_vehicle_at_home_base();
                        break;
                    case "bomb":
                        GameManager.vehicle_manager.add_boxcar("bomb");
                        break;
                    case "supply":
                        GameManager.vehicle_manager.add_boxcar("supply");
                        break;
                    case "troop":
                        GameManager.vehicle_manager.add_boxcar("troop");
                        break;
                    case "vert_desc":
                        TrackManager.add_track(item_name);
                        break;
                    case "hor_desc":
                        TrackManager.add_track(item_name);
                        break;
                    case "wn_desc":
                        TrackManager.add_track(item_name);
                        break;
                    case "ne_desc":
                        TrackManager.add_track(item_name);
                        break;
                    case "ws_desc":
                        TrackManager.add_track(item_name);
                        break;
                    case "es_desc":
                        TrackManager.add_track(item_name);
                        break;
                    default:
                        break;
                }
            }
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
        else
            print("UNSUPPORTED btn action");
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
