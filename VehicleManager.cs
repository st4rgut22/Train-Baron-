using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VehicleManager : BoardManager
{
    public GameObject Train; // prefabs
    public List<GameObject> Train_List;
    public GameObject Boxcar;
    GameObject[,] vehicle_board; //contains moving objects eg trains, boxcars
    City start_city;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        prefab_tag = "train";
        vehicle_board = new GameObject[board_dimension.x, board_dimension.y];
        Train_List = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // check if train has left the station, and reveal boxcars in the order added to the train

    }

    public bool is_vehicle_in_cell(Vector3Int location)
    {
        GameObject vehicle_object = vehicle_board[location.x, location.y];
        if (vehicle_object != null)
        {
            return true;
        }
        return false;
    }

    public IEnumerator Make_All_Boxcars_Depart(List<GameObject> boxcar_list, Train train)
    {
        // start coroutine when a train departs a city
        int boxcar_count = boxcar_list.Count;
        int boxcar_depart_id = 0; // counter
        Vector3Int last_city_location = train.get_city().get_location();
        while (boxcar_depart_id < boxcar_count)
        {
            if (!is_vehicle_in_cell(last_city_location)) // if vehicle has left city
            {
                GameObject boxcar = boxcar_list[boxcar_depart_id];
                MovingObject moving_object = boxcar.GetComponent<MovingObject>();
                boxcar.SetActive(true); // activate the boxcar.
                place_vehicle(last_city_location, boxcar);
                moving_object.set_motion(true);
                spawn_moving_object(moving_object);
                boxcar_depart_id++;
            }
            else
            {
                print("Can't activate boxcar, there is a vehicle departing the city");
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void create_boxcar(Vector3Int tilemap_position)
    {
        // check if the location has an idling train and is a city. Then instantiate a boxcar and deactivate it until train has departed
        GameObject vehicle = vehicle_board[tilemap_position.x, tilemap_position.y];
        // save the train on the board, but boxcar will overwrite it 
        GameObject city = CityManager.get_city(new Vector2Int(tilemap_position.x, tilemap_position.y));
        if (vehicle!=null && city != null)
        {
            Train train = vehicle.GetComponent<Train>();
            if (!train.is_in_motion()) // if train is idling then add boxcars
            {
                int boxcar_id = train.get_boxcar_id(); // id is the order in which the boxcar is added (0 being the first one added)
                GameObject boxcar = Instantiate(Boxcar); // change the orientation and position when train departs
                Boxcar boxcar_component = boxcar.GetComponent<Boxcar>();
                boxcar_component.attach_to_train(vehicle);
                boxcar_component.set_boxcar_id(boxcar_id);
                train.GetComponent<Train>().attach_boxcar(boxcar);
                boxcar.SetActive(false);
            }
        }
    }

    public void create_vehicle_at_home_base()
    {
        // buying a new train in MenuManager
        GameObject new_train = Instantiate(Train);
        Train_List.Add(new_train);
        Train train_component = new_train.GetComponent<Train>();
        train_component.set_id(Train_List.Count);
        train_component.set_city(home_base); // new vehicles always created at home base
        Vector3Int home_location = new Vector3Int(home_base_location.x, home_base_location.y, 0);
        place_vehicle(home_location, new_train);
    }

    public void spawn_moving_object(MovingObject moving_object)
    {
        // position vehicle in center of tile and set first destination
        Vector3Int tile_position = moving_object.tile_position;
        Vector3 moving_object_position = RouteManager.get_spawn_location(tile_position, moving_object.orientation);
        moving_object.transform.position = moving_object_position;
        moving_object.GetComponent<SpriteRenderer>().enabled = true;
        moving_object.prepare_for_departure();
    }

    public void place_vehicle(Vector3Int tilemap_position, GameObject moving_gameobject)
    {
        // initialize orientation and position of vehicle based on user input. Vehicle is not visible as it is idling at the station
        MovingObject moving_object = moving_gameobject.GetComponent<MovingObject>();
        MenuManager menu_manager = GameObject.Find("Store Menu").GetComponent<MenuManager>();
        CityManager city_manager = GameObject.Find("CityManager").GetComponent<CityManager>();
        GameObject city_gameobject = city_manager.in_cell(tilemap_position);
        City city = city_gameobject.GetComponent<City>();
        if (city_gameobject != null) // place the vehicle in a city
        {
            // orient the vehicles in any direction with a track 
            Dictionary<string,bool>exit_map = RouteManager.get_exit_track(tilemap_position);             
            string input = menu_manager.get_user_input(exit_map);
            switch (input)
            {
                case "N":
                    moving_object.set_orientation(RouteManager.Orientation.North);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 0);
                    break;
                case "E":
                    moving_object.set_orientation(RouteManager.Orientation.East);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 90);
                    break;
                case "S":
                    moving_object.set_orientation(RouteManager.Orientation.South);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 180);
                    break;
                case "W":
                    moving_object.set_orientation(RouteManager.Orientation.West);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 270);
                    break;
                default:
                    print("not a valid user input");
                    break;
            }
        }
        update_vehicle_board(moving_gameobject, tilemap_position, new Vector3Int(-1, -1, -1));
        moving_object.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void update_vehicle_board(GameObject game_object, Vector3Int position, Vector3Int prev_position)
    {
        try
        {
            bool initial_vector = prev_position.Equals(new Vector3Int(-1, -1, -1));
            if (!initial_vector)
            {
                if (vehicle_board[prev_position.x, prev_position.y] == null)
                    print("WARNING. Gameobject " + game_object.name + " not found in previous position " + prev_position);
                else
                {
                    if (vehicle_board[prev_position.x, prev_position.y] == game_object) // only remove gameobject references to itself
                        vehicle_board[prev_position.x, prev_position.y] = null;
                }
            }
            GameObject city_object = CityManager.get_city(new Vector2Int(position.x, position.y));
            vehicle_board[position.x, position.y] = game_object;
            print("updating vehicle board tile at " + position + " with vehicle " + game_object.tag);

        }
        catch (IndexOutOfRangeException e)
        {
            print(e.Message + " Position: " + position);
        }
    }


}
