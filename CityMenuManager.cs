using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class CityMenuManager : MenuManager
{
    // Detect clicks on boxcar and train inventory

    public GameObject train;
    public GameObject home_boxcar;
    public GameObject vacation_boxcar;
    public GameObject food_boxcar;
    public GameObject work_boxcar;
    string item_name;
    public string tutorial_clicked_item;

    GameObject clicked_item;

    GameObject clicked_go;

    public static new CityMenuManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        try
        {
            Vector3 position = MenuManager.convert_screen_to_world_coord(eventData.position);

            clicked_go = eventData.pointerCurrentRaycast.gameObject;
            clicked_go = eventData.pointerCurrentRaycast.gameObject;
            item_name = clicked_go.name;
            
            if (GameManager.is_tutorial_mode)
            {

                bool is_it_hit = GameManager.tutorial_manager.did_raycast_hit_blocking_mask();
                if (is_it_hit)
                {
                    eventData.pointerDrag = null;
                    return;
                }
            }
            if (GameManager.is_tutorial_mode)
                StartCoroutine(GameManager.tutorial_manager.activate_next_tutorial_step());
            string tag = eventData.pointerCurrentRaycast.gameObject.tag;
            if ((VehicleManager.get_vehicle_count(item_name) <= 0))
            {
                //print("vehicle count 0");
                eventData.pointerDrag = null;
                return;
            }
            switch (item_name)
            {
                case "train":
                    clicked_item = Instantiate(train, position, Quaternion.identity);
                    break;
                case "work":
                    clicked_item = Instantiate(work_boxcar, position, Quaternion.identity);
                    break;
                case "vacation":
                    clicked_item = Instantiate(vacation_boxcar, position, Quaternion.identity);
                    break;
                case "home":
                    clicked_item = Instantiate(home_boxcar, position, Quaternion.identity);
                    break;
                case "food":
                    clicked_item = Instantiate(food_boxcar, position, Quaternion.identity);
                    break;
                default:
                    break;
            }
            clicked_item.transform.localScale = new Vector3(4, 4);
        }
        catch (NullReferenceException)
        {
            //print("null");
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (clicked_item == null) return;
        Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
        clicked_item.transform.position = world_position;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        Vector2Int final_tilemap_position = GameManager.get_selected_tile(eventData.position);
        Station_Track st = TrackManager.get_station_from_location(final_tilemap_position, CityManager.Activated_City_Component);
        string vehicle_type = clicked_item.tag;
        Destroy(clicked_item);
        if (GameManager.is_tutorial_mode)
        {
            bool is_wrong_place;
            if (vehicle_type == "train")
                is_wrong_place = GameManager.tutorial_manager.is_click_in_wrong_place(final_tilemap_position, 4);
            else
                is_wrong_place = GameManager.tutorial_manager.is_click_in_wrong_place(final_tilemap_position);
            if (is_wrong_place) return;
        }
        if (st == null)
            return; // not a valid station
        if (vehicle_type == "home" || vehicle_type == "work" || vehicle_type == "vacation" || vehicle_type == "food")
        {
            if (st.train == null || !st.train.GetComponent<Train>().is_wait_for_turntable || !st.train.GetComponent<Train>().is_boxcar_within_max_limit())
                return; // no train at this station
            VehicleManager.instance.add_boxcar_to_train(st.train.GetComponent<Train>(), vehicle_type);
        }
        else if (vehicle_type.Contains("train"))
        {
            RouteManager.Orientation train_orientation = TrackManager.flip_straight_orientation(st.station.orientation);

            VehicleManager.instance.create_vehicle_at_home_base(train_orientation, st);
        }
        else
        {
            throw new Exception("nto a valid vehicle type");
        }
        VehicleManager.update_vehicle_count(vehicle_type, -1); // "home", "food", "work", "vacation"
        update_inventory();
        if (VehicleManager.get_vehicle_count(vehicle_type) == 0)
        {
            RawImage raw_image = clicked_go.GetComponent<RawImage>();
            raw_image.texture = empty_inventory_bubble;
        }
    }

    public void change_bck_color(string structure_name)
    {
        Color32 color;
        if (structure_name=="Apartment" || structure_name=="Mansion")
        {
            color = home_color;
        }
        else if (structure_name == "Factory" ||  structure_name=="business")
        {
            color = work_color;
        }
        else if (structure_name=="Diner" || structure_name=="Restaurant")
        {
            color = food_color;
        }
        else if (structure_name == "Entrance")
        {
            color = entrance_color;
        }
        else { throw new Exception("cant find color for structure " + structure_name); }
        transform.parent.Find("color bck").GetComponent<Image>().color = color;
    }

    public void turn_of_vehicle_in_exit_bar(bool is_on)
    {
        gameObject.transform.Find("work").gameObject.SetActive(is_on);
        gameObject.transform.Find("train").gameObject.SetActive(is_on);
        gameObject.transform.Find("vacation").gameObject.SetActive(is_on);
        gameObject.transform.Find("home").gameObject.SetActive(is_on);
        gameObject.transform.Find("food").gameObject.SetActive(is_on);
    }
}
