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
        menu_manager = GameObject.Find("Store Menu").GetComponent<MenuManager>();
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
                        Train train_component = clicked_gameobject.GetComponent<Train>();
                        train_component.change_motion(); 
                        break;
                    case "Structure": // if user clicks on city, create city menu
                        GameObject city = CityManager.get_city(new Vector2Int((int)mouse_pos.x, (int)mouse_pos.y));
                        try
                        {
                            menu_manager.create_city_menu(city);
                        } catch (NullReferenceException e)
                        { // city should not be null
                            print(e.StackTrace);
                        }
                        // TODO: add commented code to the departure method
                        //clicked_gameobject = GameObject.Find("train(Clone)");
                        //clicked_gameobject.SetActive(true);
                        //Train red_train_component = clicked_gameobject.GetComponent<Train>();
                        //red_train_component.change_motion();
                        //vehicle_manager.spawn_moving_object(red_train_component);
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
