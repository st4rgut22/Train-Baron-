using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VehicleManager : BoardManager
{
    public GameObject Train; // prefabs
    public List<GameObject> Train_List;
    public List<GameObject> Bomb_Boxcar_Inventory;
    public List<GameObject> Troop_Boxcar_Inventory;
    public List<GameObject> Supply_Boxcar_Inventory;
    public GameObject Troop_Boxcar;
    public GameObject Supply_Boxcar;
    public GameObject Bomb_Boxcar;
    public static GameObject[,] vehicle_board; //contains moving objects eg trains, boxcars
    City start_city;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        prefab_tag = "train";
        vehicle_board = new GameObject[board_width, board_height];
        Train_List = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // check if train has left the station, and reveal boxcars in the order added to the train

    }

    public void add_all_boxcar_to_train(Train train)
    {
        foreach (GameObject bomb_boxcar in Bomb_Boxcar_Inventory)
        {
            create_boxcar(train, bomb_boxcar);
        }
        foreach (GameObject troop_boxcar in Troop_Boxcar_Inventory)
        {
            create_boxcar(train, troop_boxcar);
        }
        foreach (GameObject supply_boxcar in Supply_Boxcar_Inventory)
        {
            create_boxcar(train, supply_boxcar);
        }
    }

    public bool is_vehicle_in_cell(Vector3Int location, GameObject[,] board)
    {
        GameObject vehicle_object = board[location.x, location.y];
        if (vehicle_object != null)
        {
            return true;
        }
        return false;
    }

    public IEnumerator Make_All_Boxcars_Depart(GameObject[,] board, List<GameObject> boxcar_list, Train train)
    {
        // start coroutine when a train departs a city
        int boxcar_count = boxcar_list.Count;
        int boxcar_depart_id = 0; // counter
        Vector3Int last_city_location = train.get_city().get_location();
        RouteManager.Orientation depart_orientation = train.orientation;
        if (train.in_city) board = train.get_city().city_board;
        while (boxcar_depart_id < boxcar_count)
        {
            if (!is_vehicle_in_cell(last_city_location, board)) // if vehicle has left city
            {
                GameObject boxcar = boxcar_list[boxcar_depart_id];
                Boxcar moving_boxcar = boxcar.GetComponent<Boxcar>();
                print("boxcar orientation is " + moving_boxcar.get_orientation());
                moving_boxcar.set_depart_status(true);
                boxcar.SetActive(true); // activate the boxcar.
                place_vehicle(last_city_location, boxcar);
                moving_boxcar.is_halt = false;
                spawn_moving_object(moving_boxcar);
                moving_boxcar.set_depart_status(false);
                boxcar_depart_id++;
            }
            else
            {
                print("Can't activate boxcar, there is a vehicle departing the city");
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void remove_boxcar(Train train)
    {
        train.remove_boxcar();
    }

    public void add_boxcar(string boxcar_type)
    {
        //add boxcar to the inventory
        if (boxcar_type == "bomb")
            Bomb_Boxcar_Inventory.Add(Instantiate(Bomb_Boxcar));
        else if (boxcar_type == "supply")
            Supply_Boxcar_Inventory.Add(Instantiate(Supply_Boxcar));
        else if (boxcar_type == "troop")
            Troop_Boxcar_Inventory.Add(Instantiate(Troop_Boxcar));
        else { print("No other type of boxcar");  }
    }

    public void create_boxcar(Train train, GameObject boxcar)
    {
        // save the train on the board, but boxcar will overwrite it
        if (train.in_city) // if train is in city then add boxcars //TODO: check train is in same city with boxcars
        {
            Boxcar boxcar_component = boxcar.GetComponent<Boxcar>();
            boxcar_component.attach_to_train(train);
            boxcar_component.city = train.city;
            train.GetComponent<Train>().attach_boxcar(boxcar);
            boxcar.SetActive(false);
            int boxcar_id = train.get_boxcar_id(); // id is the order in which the boxcar is added (0 being the first one added)
            boxcar_component.set_boxcar_id(boxcar_id);
        }
    }

    public void create_vehicle_at_home_base()
    {
        // buying a new train in MenuManager
        Vector3Int start_tile_position = new Vector3Int(-1, 0, 0); //TODO: change if this track is occupied
        GameObject new_train = Instantiate(Train);
        Train_List.Add(new_train);
        Train train_component = new_train.GetComponent<Train>();
        train_component.in_city = true;
        train_component.set_id(Train_List.Count);
        train_component.set_city(home_base); // new vehicles always created at home base
        train_component.set_position(start_tile_position);
        Vector3Int home_base_location = new Vector3Int(BoardManager.home_base_location.x, BoardManager.home_base_location.y, 0);
        place_vehicle(home_base_location, new_train);
    }

    public void spawn_moving_object(MovingObject moving_object)
    {
        // position vehicle in center of tile and set first destination
        Vector3Int tile_position = moving_object.tile_position;
        Vector3 moving_object_position = RouteManager.get_spawn_location(tile_position, moving_object.orientation);
        moving_object.transform.position = moving_object_position;
        moving_object.prepare_for_departure();
    }

    public void depart(GameObject train_object)
    {
        Train train = train_object.GetComponent<Train>();
        // remove train from station and depart.
        Vector3Int city_location = train.city.get_location();
        place_vehicle(city_location, train_object);
        add_all_boxcar_to_train(train); //TODO: replace with attach_boxcar_to_train()
        print("departing station. Adding all boxcars to the train");
        StartCoroutine(Make_All_Boxcars_Depart(vehicle_board, train.boxcar_squad, train)); //TODO: hide train on city tile. Call coroutine from city manager
        spawn_moving_object(train);
        //city.remove_train_from_list(train); 
    }

    public void attach_boxcar_to_train()
    {
        //TODO: allow users to add boxcars in the shipyard
    }

    public void depart_shipyard()
    {
        //TODO: Remove train from list AFTER it has left the shipyard
    }

    public void place_vehicle(Vector3Int city_position, GameObject moving_gameobject)
    {
        // initialize orientation and position of vehicle based on user input. Vehicle is not visible as it is idling at the station
        // vehicle is placed in a shipyard
        MovingObject moving_object = moving_gameobject.GetComponent<MovingObject>();
        GameObject city_gameobject = GameManager.city_manager.in_cell(city_position);
        City city = city_gameobject.GetComponent<City>();
        if (city_gameobject != null) // place the vehicle in a city
        {
            // orient the vehicles in any direction with a track 
            switch (moving_object.orientation)
            {
                case RouteManager.Orientation.North:
                    moving_object.set_orientation(RouteManager.Orientation.North);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 0);
                    break;
                case RouteManager.Orientation.East:
                    moving_object.set_orientation(RouteManager.Orientation.East);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 90);
                    break;
                case RouteManager.Orientation.South:
                    moving_object.set_orientation(RouteManager.Orientation.South);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 180);
                    break;
                case RouteManager.Orientation.West:
                    moving_object.set_orientation(RouteManager.Orientation.West);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 270);
                    break;
                default:
                    print("invalid orientation");
                    break;
            }
        }
        if (moving_object.in_city) update_vehicle_board(moving_object.city.city_board, moving_gameobject, city_position, new Vector3Int(-1, -1, -1));
        else { update_vehicle_board(vehicle_board, moving_gameobject, city_position, new Vector3Int(-1, -1, -1));  }
    }

    public void update_vehicle_board(GameObject[,] vehicle_board, GameObject game_object, Vector3Int position, Vector3Int prev_position)
    {
        print("move " + game_object.name + " to position " + position);
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
            bool in_city = game_object.GetComponent<MovingObject>().in_city;
            GameObject city_object = CityManager.get_city(new Vector2Int(position.x, position.y));
            string destination_type = RouteManager.get_destination_type(position, in_city);
            if (destination_type=="city" && !in_city) // if vehicle arriving at city is a boxcar, don't update tile
            {
                if (game_object.tag=="train") vehicle_board[position.x, position.y] = game_object; // only trains should be in cities, it stores a list of attached boxcars
                else if (game_object.tag == "boxcar") // only update city tile with boxcar if the boxcar is departing. 
                {
                    bool boxcar_is_departing = game_object.GetComponent<Boxcar>().get_depart_status();
                    if (boxcar_is_departing) vehicle_board[position.x, position.y] = game_object;
                }
            }
            else
            {
                vehicle_board[position.x, position.y] = game_object;  
            }
            print("updating vehicle board tile at " + position + " with vehicle " + game_object.tag);

        }
        catch (IndexOutOfRangeException e)
        {
            print(e.Message + " Position: " + position);
        }
    }
}