using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class GameManager : MonoBehaviour
{
    public Camera camera;
    // manage score, game state 

    const int cell_width = 1;
    const int cell_height = 1;

    public GameObject Track_Layer;
    public GameObject Structure;
    public GameObject Base;
    public GameObject Shipyard_Base;
    public GameObject Shipyard_Track;
    public GameObject Shipyard_Turntable;
    public GameObject Shipyard_Turntable_Circle;
    public GameObject Shipyard_Track2;

    public static VehicleManager vehicle_manager;
    public static CityManager city_manager;
    public static RouteManager route_manager;
    public static TrackManager track_manager;
    public static MenuManager menu_manager;
    //public static StoreMenuManager game_menu_manager;

    // Start is called before the first frame update
    void Start()
    {
        vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>(); ;
        city_manager = GameObject.Find("CityManager").GetComponent<CityManager>();
        track_manager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        menu_manager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        //game_menu_manager = GameObject.Find("Store Menu").GetComponent<StoreMenuManager>();
        switch_on_shipyard(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(camera.transform.position.z));
            Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
            Vector2 mouse_pos_2d = new Vector2(mouse_pos.x, mouse_pos.y);

            RaycastHit2D hit = Physics2D.Raycast(mouse_pos_2d, Vector2.zero);
            if (hit.collider != null)
            {
                GameObject clicked_gameobject = hit.collider.gameObject;
                string object_name = clicked_gameobject.name.Replace("(Clone)", "");
                switch (object_name)
                {
                    case "ES":
                        break;
                    case "NE":
                        break;
                    case "WN":
                        break;
                    case "WS":
                        break;
                    case "hor":
                        break;
                    case "vert":
                        break;
                    case "train": // start/pause a train
                        //Train train_component = clicked_gameobject.GetComponent<Train>();
                        //train_component.change_motion(); 
                        break;
                    case "Structure": // if user clicks on city, create city menu
                        GameObject city_object = CityManager.get_city(new Vector2Int((int)mouse_pos.x, (int)mouse_pos.y));
                        try
                        {
                            switch_on_shipyard(true);
                            city_manager.set_activated_city(city_object);
                            MenuManager.activate_handler(new List<GameObject> { MenuManager.shipyard_exit_menu });
                        }
                        catch (NullReferenceException e)
                        { // city should not be null
                            print("Error!" + e.StackTrace);
                        }
                        break;
                    case "boxcar":
                        break;
                    default:
                        print("You did not click a gameobject. Object name is " + object_name);
                        break;
                }
            }
        }
    }

    public void switch_on_shipyard(bool state)
    {
        Structure.GetComponent<TilemapCollider2D>().enabled = !state; // turn off colliders for city
        Shipyard_Base.SetActive(state);
        Shipyard_Track.SetActive(state);
        Shipyard_Track2.SetActive(state);
        Shipyard_Turntable.SetActive(state);
        Shipyard_Turntable_Circle.SetActive(state);
        Track_Layer.SetActive(!state);
        Structure.SetActive(!state);
        Base.SetActive(!state);
    }
}
