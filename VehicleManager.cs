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

    public void create_vehicle_at_home_base()
    {
        // buying a new train in MenuManager
        GameObject new_train = Instantiate(Train);
        Train_List.Add(new_train);
        Train train_component = new_train.GetComponent<Train>();
        train_component.in_city = true;
        train_component.set_id(Train_List.Count);
        train_component.set_city(home_base); // new vehicles always created at home base
        add_all_boxcar_to_train(train_component);
        place_vehicle(new_train); // place the vehicle, which proceeds to depart
        train_component.arrive_at_city(); // call immediately on instantiation. Otherwise, in_city = false and the wrong board is updated        
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
        // start coroutine when a train departs or arrives at a city
        int boxcar_count = boxcar_list.Count;
        int boxcar_depart_id = 0; // counter
        Vector3Int last_location = train.tile_position;
        RouteManager.Orientation depart_orientation = train.orientation;
        GameObject boxcar = boxcar_list[boxcar_depart_id];
        Boxcar moving_boxcar = boxcar.GetComponent<Boxcar>();
        if (train.in_city) board = train.get_city().city_board;
        while (boxcar_depart_id < boxcar_count) 
        {
            if (!is_vehicle_in_cell(last_location, board) && moving_boxcar.in_city == train.in_city) // dont depart until boxcar has arrived at city
            {
                print("Make Boxcar depart. boxcar orientation is " + moving_boxcar.get_orientation() + " tile position is " + last_location);
                moving_boxcar.set_depart_status(true);
                if (train.in_city) moving_boxcar.receive_train_order = true;
                moving_boxcar.tile_position = last_location;
                place_vehicle(boxcar);
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
            boxcar_component.arrive_at_city();
            train.GetComponent<Train>().attach_boxcar(boxcar);
            int boxcar_id = train.get_boxcar_id(); // id is the order in which the boxcar is added (0 being the first one added)
            boxcar_component.set_boxcar_id(boxcar_id);
        }
    }

    public void spawn_moving_object(MovingObject moving_object)
    {
        // position vehicle in center of tile and set first destination
        Vector3Int tile_position = moving_object.tile_position;
        Vector3 moving_object_position = RouteManager.get_spawn_location(tile_position, moving_object.orientation);
        moving_object.transform.position = moving_object_position;
        moving_object.prepare_for_departure();
    }

    public void depart(GameObject train_object, Vector3Int new_tile_position, GameObject[,] board=null)
    {
        print(gameObject.name + " departing to new tile position " + new_tile_position);
        Train train = train_object.GetComponent<Train>();
        // remove train from station and depart.
        train.tile_position = new_tile_position; //TODO: depart should update vehicle's position to track position in TrackManager
        place_vehicle(train_object);
        //add_all_boxcar_to_train(train);
        print("departing station. Adding all boxcars to the train");
        //assign the type of board depending on if leaving or arriving
        if (board==null) StartCoroutine(Make_All_Boxcars_Depart(vehicle_board, train.boxcar_squad, train));
        else { StartCoroutine(Make_All_Boxcars_Depart(board, train.boxcar_squad, train)); }
        spawn_moving_object(train);
        //city.remove_train_from_list(train); 
    }

    public void place_vehicle(GameObject moving_gameobject)
    {
        // place vehicle on entering or exiting a city
        MovingObject moving_object = moving_gameobject.GetComponent<MovingObject>();
        Vector3Int city_position = moving_object.city.get_location();
        Vector3Int station_start_position = moving_object.tile_position;
        if (moving_object.city != null) // place the vehicle in a city
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
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                case RouteManager.Orientation.South:
                    moving_object.set_orientation(RouteManager.Orientation.South);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, 180);
                    break;
                case RouteManager.Orientation.West:
                    moving_object.set_orientation(RouteManager.Orientation.West);
                    moving_gameobject.transform.eulerAngles = new Vector3(0, 0, -270);
                    break;
                default:
                    print("invalid orientation");
                    break;
            }
        }
        // if not in city update vehicle position with city position
        if (moving_object.in_city)
        {
            update_vehicle_board(moving_object.city.city_board, moving_gameobject, station_start_position, new Vector3Int(-1, -1, -1));
        }
        else { update_vehicle_board(vehicle_board, moving_gameobject, city_position, new Vector3Int(-1, -1, -1));  }
    }

    public void update_vehicle_board(GameObject[,] vehicle_board, GameObject game_object, Vector3Int position, Vector3Int prev_position)
    {
        print("Update Vehicle Board with object " + game_object.name + " to position " + position);
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