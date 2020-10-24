using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClickManager : MonoBehaviour
{
    VehicleManager vehicle_manager;
    CityManager city_manager;
    MenuManager menu_manager;

    public Camera camera;
    // detect clicks on colliders. Useful for interacting with moving objects.

    // Start is called before the first frame update
    void Start()
    {
        menu_manager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
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
                            GameObject.Find("GameManager").GetComponent<GameManager>().switch_on_shipyard(true);
                            city_object.GetComponent<City>().is_train_turn_on(true);
                            MenuManager.activate_handler(new List<GameObject> { MenuManager.shipyard_exit_menu });
                        } catch (NullReferenceException e)
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
}
