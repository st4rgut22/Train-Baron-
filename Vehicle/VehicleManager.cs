using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class VehicleManager : BoardManager
{
    public GameObject Train; // prefabs
    public List<GameObject> Train_List;
    public List<GameObject> Food_Boxcar_Inventory;
    public List<GameObject> Vacation_Boxcar_Inventory;
    public List<GameObject> Work_Boxcar_Inventory;
    public List<GameObject> Home_Boxcar_Inventory;

    public GameObject Vacation_Boxcar;
    public GameObject Work_Boxcar;
    public GameObject Food_Boxcar;
    public GameObject Home_Boxcar;
    public static GameObject[,] vehicle_board; //contains moving objects eg trains, boxcars
    City start_city;
    public static int train_counter;
    public static int boxcar_counter;

    static int orient_count = 0; // for testing

    private void Awake()
    {
        base.Awake();
        vehicle_board = new GameObject[board_width, board_height];
        boxcar_counter = 0;
        train_counter = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        prefab_tag = "train";
        Train_List = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // check if train has left the station, and reveal boxcars in the order added to the train

    }

    public static RouteManager.Orientation round_robin_orientation()
    {
        // TEMPORARY: FOR TESTING!
        RouteManager.Orientation[] orient_list = { RouteManager.Orientation.North, RouteManager.Orientation.East, RouteManager.Orientation.West, RouteManager.Orientation.South };
        RouteManager.Orientation orientation = orient_list[orient_count % 4];
        orient_count++;
        return orientation;
    }

    public void create_vehicle_at_home_base()
    {
        // buying a new train in MenuManager
        train_counter++;
        GameObject new_train = Instantiate(Train);
        Train_List.Add(new_train);
        Train train_component = new_train.GetComponent<Train>();
        train_component.in_city = true;
        train_component.set_id(train_counter);
        train_component.set_city(CityManager.home_base); // new vehicles always created at home base
        place_vehicle(new_train); // place the vehicle, which proceeds to depart
        train_component.arrive_at_city(); // call immediately on instantiation. Otherwise, in_city = false and the wrong board is updated        
        // TESTING        //add_all_boxcar_to_train(train_component);

    }

    public void add_boxcar_to_train(Train train, string boxcar_type) //Temporary
    {
        // add all boxcars in inventory to a train. Revise to select boxcars to add to a train
        GameObject boxcar_object;
        if (boxcar_type=="work_boxcar")
        {
            boxcar_object = Instantiate(Work_Boxcar);
        }
        else if (boxcar_type=="food_boxcar")
        {
            boxcar_object = Instantiate(Food_Boxcar);
        }
        else if (boxcar_type=="vacation_boxcar")
        {
            boxcar_object = Instantiate(Vacation_Boxcar);
        }
        else if (boxcar_type == "home_boxcar")
        {
            boxcar_object = Instantiate(Home_Boxcar);
        }
        else
        {
            throw new Exception("no such boxcar exists");
        }
        create_boxcar(train, boxcar_object);
    }

    public void boxcar_fill_void(GameObject boxcar_object)
    {
        Boxcar boxcar = boxcar_object.GetComponent<Boxcar>();
        Train train = boxcar.train;
        int removed_boxcar_id = boxcar.boxcar_id;
        List<GameObject> boxcar_squad = train.boxcar_squad;
        int remove_boxcar_idx = train.get_boxcar_by_id(removed_boxcar_id);
        if (removed_boxcar_id > 0)
        {
            Boxcar prev_boxcar = boxcar_squad[remove_boxcar_idx].GetComponent<Boxcar>(); // spot of the previous boxcar
            for (int i = remove_boxcar_idx; i < boxcar_squad.Count; i++)
            {
                boxcar = boxcar_squad[i].GetComponent<Boxcar>();
                if (boxcar.orientation != prev_boxcar.orientation)
                {
                    StartCoroutine(boxcar.one_time_bezier_move(prev_boxcar));
                } else
                {
                    StartCoroutine(boxcar.one_time_straight_move(prev_boxcar));
                }
                prev_boxcar = boxcar; // set prev boxcar location to location of the boxcar that moved in to fill the void
            }
        }
    }

    public bool is_vehicle_in_cell(Vector3Int unadjusted_location, GameObject[,] board)
    {
        Vector3Int location = new Vector3Int(unadjusted_location.x+1, unadjusted_location.y+1, unadjusted_location.z);
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
        // 1. train departs
        // 2. train is placed in city tile
        // 3. train calls Boxcar Depart Coroutine
        // 4. train moves to city boundary
        // 5. train updates tile position
        // 6. train confirms end of track is next tile
        // 7. train stops and tells boxcars to stop
        Vector3Int last_location = train.tile_position; 
        print("train last location is " + last_location);
        RouteManager.Orientation depart_orientation = train.orientation;
        MovingObject last_vehicle = train;
        if (train.in_city) board = train.get_city().city_board;
        while (boxcar_depart_id < boxcar_count) 
        {
            GameObject boxcar = boxcar_list[boxcar_depart_id];
            if (boxcar == null)
                yield break; // the boxcar collidd with another train while entering city. 
            Boxcar moving_boxcar = boxcar.GetComponent<Boxcar>();

            if (!is_vehicle_in_cell(last_location, board) && moving_boxcar.in_city == last_vehicle.in_city && !moving_boxcar.is_pause) // dont depart until boxcar has arrived at city
            {
                if (moving_boxcar.in_city && moving_boxcar.city != last_vehicle.city)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                last_vehicle = moving_boxcar;
                moving_boxcar.departing = false; // reset departing 
                print("Make Boxcar depart. boxcar orientation is " + moving_boxcar.get_orientation() + " new tile position is " + last_location + "old tile position is " + moving_boxcar.tile_position);
                moving_boxcar.set_depart_status(true);
                if (train.in_city) moving_boxcar.receive_train_order = true;
                moving_boxcar.tile_position = last_location;
                place_vehicle(boxcar);
                moving_boxcar.is_halt = false;
                spawn_moving_object(moving_boxcar);
                moving_boxcar.set_depart_status(false);
                boxcar_depart_id++;
                game_manager.enable_vehicle_for_screen(boxcar); // switch on when boxcar is departing from the city. Dont show entire train cargo.
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void add_boxcar(string boxcar_type)
    {
        CityManager.home_base.add_boxcar_to_tilemap(boxcar_type);
    }

    public static void initialize_position(MovingObject moving_object, PositionPair pos_pair)
    {
        moving_object.tile_position = new Vector3Int(pos_pair.tile_dest_pos.x, pos_pair.tile_dest_pos.y, 0);
        moving_object.next_tilemap_position = pos_pair.tile_dest_pos;
        moving_object.prev_tile_position = (Vector3Int)pos_pair.prev_tile_dest_pos;
        moving_object.transform.position = pos_pair.abs_dest_pos;
        moving_object.orientation = pos_pair.orientation;
        moving_object.final_orientation = pos_pair.orientation;
    }

    public static void create_boxcar(Train train, GameObject boxcar)
    {
        // save the train on the board, but boxcar will overwrite it
        if (train.in_city) // if train is in city then add boxcars 
        {
            Tilemap tilemap = train.station_track.tilemap;
            Boxcar boxcar_component = boxcar.GetComponent<Boxcar>();
            boxcar_counter += 1;
            boxcar_component.attach_to_train(train);
            MovingObject last_vehicle = train.get_last_vehicle_added().GetComponent<MovingObject>();
            // initalize boxcar position
            PositionPair pos_pair = TrainRouteManager.get_initial_destination(last_vehicle, tilemap);
            initialize_position(boxcar_component, pos_pair);
            boxcar_component.set_initial_rotation(boxcar_component.orientation);
            //set_initial_angle(boxcar, boxcar_component);
            boxcar_component.city = train.city;
            boxcar_component.station_track = train.station_track;
            boxcar_component.arrive_at_city();
            boxcar_component.initialize_boxcar(boxcar_counter);
            boxcar_component.is_wait_for_turntable = true;
            train.boxcar_squad.Add(boxcar);
            // save gameobject tile with adjustments. when a user clicks on a tile, it will be in the tile opposite the vehicle's orientation. Therefore, flip orientation
            //Vector2Int boxcar_board_position = RouteManager.get_straight_next_tile_pos(TrackManager.flip_straight_orientation(boxcar_component.orientation), (Vector2Int)boxcar_component.tile_position);
            boxcar_component.city.city_board[pos_pair.tile_dest_pos.x+1, pos_pair.tile_dest_pos.y+1] = boxcar; // offset boxcar to be consistent
            print("new boxcar created at tile position " + pos_pair.tile_dest_pos);
        }
    }

    public void spawn_moving_object(MovingObject moving_object)
    {
        // position vehicle in center of tile and set first destination
        Vector3Int tile_position = moving_object.tile_position;
        Vector3 moving_object_position = TrainRouteManager.get_spawn_location(tile_position, moving_object.orientation);
        moving_object.transform.position = moving_object_position;
        if (moving_object.tag == "boxcar")
        {
            Boxcar boxcar = (Boxcar)moving_object;
        }
        StartCoroutine(moving_object.prepare_for_departure());
    }

    public void depart(GameObject train_object, Vector3Int new_tile_position, GameObject[,] board=null)
    {
        //print(gameObject.name + " departing to new tile position " + new_tile_position);
        Train train = train_object.GetComponent<Train>();
        // remove train from station and depart.
        train.tile_position = new_tile_position; //TODO: depart should update vehicle's position to track position in TrackManager
        place_vehicle(train_object);
        //assign the type of board depending on if leaving or arriving
        if (board==null) StartCoroutine(Make_All_Boxcars_Depart(vehicle_board, train.boxcar_squad, train)); // last location????
        else { StartCoroutine(Make_All_Boxcars_Depart(board, train.boxcar_squad, train)); }
        spawn_moving_object(train);  
        //city.remove_train_from_list(train); 
    }

    public void place_vehicle(GameObject moving_gameobject)
    {
        // place vehicle on entering or exiting a city
        MovingObject moving_object = moving_gameobject.GetComponent<MovingObject>();
        Vector3Int city_position = moving_object.city.get_location();
        if (moving_object.city != null) // place the vehicle in a city
        {
            // orient the vehicles in any direction with a track
            moving_object.set_initial_rotation(moving_object.orientation);
        }
        if (!moving_object.in_city) {
            update_vehicle_board(vehicle_board, moving_gameobject, city_position, new Vector3Int(-1, -1, -1));
        }
    }

    public void update_vehicle_board(GameObject[,] vehicle_board, GameObject game_object, Vector3Int unadjusted_position, Vector3Int unadjusted_prev_position)
    {
        // note the OFFSET by 1 in xy direction to include negative tile positions in the board. This board is updated outside city and inside city, up to the boarding position (not after dont ask me why)
        Vector3Int position = new Vector3Int(unadjusted_position.x + 1, unadjusted_position.y + 1, unadjusted_position.z);
        Vector3Int prev_position = new Vector3Int(unadjusted_prev_position.x + 1, unadjusted_prev_position.y + 1, unadjusted_prev_position.z);
        try
        {
            bool initial_vector = prev_position.Equals(new Vector3Int(-1, -1, -1));
            if (!initial_vector)
            {
                if (vehicle_board[prev_position.x, prev_position.y] == null)
                {
                    print("WARNING. Gameobject " + game_object.name + " not found in previous position " + prev_position);
                }
                else
                {
                    if (vehicle_board[prev_position.x, prev_position.y] == game_object) // only remove gameobject references to itself
                        vehicle_board[prev_position.x, prev_position.y] = null;
                }
            }
            bool in_city = game_object.GetComponent<MovingObject>().in_city;
            GameObject city_object = GameManager.city_manager.get_city(new Vector2Int(position.x, position.y));
            string destination_type = TrainRouteManager.get_destination_type(unadjusted_position, in_city);
            if (destination_type=="city") // if vehicle arriving at city is a boxcar, don't update tile
            {
                if (game_object.tag=="train") vehicle_board[position.x, position.y] = game_object; // only trains should be in cities, it stores a list of attached boxcars
            }
            else
            {
                vehicle_board[position.x, position.y] = game_object;
                print("Update Vehicle Board with object " + game_object.name + " to position " + position);
            }

        }
        catch (IndexOutOfRangeException e)
        {
            print(e.Message + " Position: " + position);
        }
    }
}