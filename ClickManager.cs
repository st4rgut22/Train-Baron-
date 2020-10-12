using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ClickManager : MonoBehaviour
{
    VehicleManager vehicle_manager;
    public Camera camera;
    // detect clicks on colliders. Useful for interacting with moving objects.

    // Start is called before the first frame update
    void Start()
    {
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
                    case "train":
                        Train train_component = clicked_gameobject.GetComponent<Train>();
                        train_component.change_motion(); 
                        break;
                    case "Structure Layer": // redundant. Remove when implement the train leave feature for cities
                        clicked_gameobject = GameObject.Find("train(Clone)");
                        clicked_gameobject.SetActive(true);
                        Train red_train_component = clicked_gameobject.GetComponent<Train>();
                        red_train_component.change_motion();
                        vehicle_manager.spawn_moving_object(red_train_component);
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
