using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CityMenuManager : MenuManager
{
    // Detect clicks on boxcar and train inventory

    public Texture empty_inventory_bubble;
    public Texture train_inventory_bubble;
    public Texture food_inventory_bubble;
    public Texture home_inventory_bubble;
    public Texture work_inventory_bubble;
    public Texture vacation_inventory_bubble;

    public GameObject train;
    public GameObject home_boxcar;
    public GameObject vacation_boxcar;
    public GameObject food_boxcar;
    public GameObject work_boxcar;

    string item_name;

    GameObject clicked_item;

    GameObject clicked_go;

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
            clicked_go = eventData.pointerCurrentRaycast.gameObject;
            item_name = clicked_go.name;
            string tag = eventData.pointerCurrentRaycast.gameObject.tag;
            Vector3 position = MenuManager.convert_screen_to_world_coord(eventData.position);
            if ((VehicleManager.get_vehicle_count(item_name) <= 0))
            {
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
        }
        catch (NullReferenceException)
        {
            print("null");
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
        Destroy(clicked_item);
        if (st == null)
            return; // not a valid station
        string vehicle_type = clicked_item.tag;
        if (vehicle_type == "home" || vehicle_type == "work" || vehicle_type == "vacation" || vehicle_type == "food")
        {
            if (st.train == null)
                return; // no train at this station
            GameManager.vehicle_manager.add_boxcar_to_train(st.train.GetComponent<Train>(), vehicle_type);        
        }
        else if (vehicle_type.Contains("train"))
        {
            RouteManager.Orientation train_orientation = TrackManager.flip_straight_orientation(st.station.orientation);

            GameManager.vehicle_manager.create_vehicle_at_home_base(train_orientation, st);
        }
        else
        {
            throw new Exception("nto a valid vehicle type");
        }
        VehicleManager.update_vehicle_count(vehicle_type, -1); // "home", "food", "work", "vacation"
        update_inventory();
    }
}
