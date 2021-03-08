using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Tilemaps;

public class MovingObject : Simple_Moving_Object
{
    // all the vehicle movement math goes here
    public bool is_pause = false; // pause state
    public bool is_halt = false;
    public float sprite_renderer_delay; // used when vehicles depart city to prevent vehicle buttt showing
    public bool depart_for_turntable = true;
    public bool leave_turntable = false;
    public bool leave_city = false;
    public bool complete_exit = false; // on verge of departing city
    public bool departure_track_chosen = false;
    public bool end_of_track = false;
    public bool is_wait_for_turntable;
    public bool is_boxcar_stopped;
    public bool is_fill_void;
    //public bool is_instantiated;
    public bool go_to_turntable = false;
    public bool one_time_move_pass = false;

    public string train_name = "train(Clone)";
    public RouteManager.Orientation exit_track_orientation = RouteManager.Orientation.None;
    public RouteManager.Orientation steep_angle_orientation = RouteManager.Orientation.None;
    public Vector3 train_destination;

    // Start is called before the first frame update
    public void Awake()
    {
        //is_instantiated = true;
        is_fill_void = false;
        is_boxcar_stopped = true;
        is_wait_for_turntable = false;
        Vector2Int home_base = CityManager.home_base_location;
        tile_position = new Vector3Int(home_base.x, home_base.y, 0);
        next_tilemap_position = home_base;
        prev_city = null;
        //orientation = TrackManager.flip_straight_orientation(CityManager.home_base.get_station_orientation_with_room()); //VehicleManager.round_robin_orientation();//RouteManager.Orientation.North; // TODOED temporary
        sprite_renderer_delay = .3f;
    }

    public override void Start()
    {
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        target_position = transform.position;
    }

    public void initialize_orientation(RouteManager.Orientation orientation)
    {
        this.orientation = orientation;
        final_orientation = orientation;
    }

public void set_destination()
    {
        orientation = final_orientation; // updating the orientation at every new tile
        prev_tile_position = tile_position;
        tile_position = new Vector3Int(next_tilemap_position.x, next_tilemap_position.y, 0);
        PositionPair position_pair;
        if (!in_city)
        {
            Tilemap toggled_tilemap = TrackManager.instance.top_tilemap;
            ////print("update vehicle board at " + tile_position);
            VehicleManager.instance.update_vehicle_board(VehicleManager.vehicle_board, gameObject, tile_position, prev_tile_position);
            position_pair = RouteManager.get_destination(this, toggled_tilemap, offset); // set the final orientation and destination
        }
        else
        {
            VehicleManager.instance.update_vehicle_board(city.city_board, gameObject, tile_position, prev_tile_position);
            position_pair = RouteManager.get_destination(this, station_track.tilemap, offset); // set the final orientation and destination
        }
        Vector2 train_dest_xy = position_pair.abs_dest_pos;
        next_tilemap_position = position_pair.tile_dest_pos;
        train_destination = new Vector3(train_dest_xy[0], train_dest_xy[1], z_pos);
    }

    // Update is called once per frame
    public override void Update()
    {
        if (!is_halt && !in_tile && !end_of_track) // Completed tile route. update destination to next tile. Prevents repeated calls to StartCoroutine()
        {
            // stop the train here if track ends
            if (in_city)
            {
                Tile next_tile = (Tile)station_track.tilemap.GetTile((Vector3Int)next_tilemap_position);
                if (next_tile != null && TrackManager.is_curve_steep(next_tile.name))
                {
                    StartCoroutine(wait_for_turntable(next_tile.name));
                }
            }
            if (orientation != final_orientation) // curved track
            {
                StartCoroutine(bezier_move(transform, orientation, final_orientation));
            }
            else // straight track
            {
                if (leave_turntable) // go to end of turn table in straight line
                {
                    // train travels to the other end of the turntable
                    leave_turntable = false;
                    float distance_multiplier = 4.5f;
                    go_to_turntable = true;
                    if (gameObject.tag == "boxcar")
                    {
                        Boxcar boxcar = gameObject.GetComponent<Boxcar>();
                        distance_multiplier -= boxcar.train.get_distance_from_train(boxcar.boxcar_id); // offset boxcar n units away from train
                        //print("distance multiplier of boxcar " + boxcar.boxcar_id + " is " + distance_multiplier);
                    }
                    train_destination = transform.up * distance_multiplier * RouteManager.cell_width + transform.position; // 5 units in the direction the train is facing
                    //print("destination is " + train_destination);
                    if (gameObject.tag == "train")
                        gameObject.transform.parent = gameObject.GetComponent<Train>().city.turn_table.transform; // make train child of turntable so it rotates with it
                    else { gameObject.transform.parent = gameObject.GetComponent<Boxcar>().city.turn_table.transform; }
                    StartCoroutine(straight_move(transform.position, train_destination, true, false));
                }
                else
                {
                    StartCoroutine(straight_move(transform.position, train_destination, false, false));
                }
            }
        }
        if (leave_city) // train is ready to depart from city
        {
            int up_multiplier = 0;
            leave_city = false;
            gameObject.transform.parent = null; // decouple train from turntable parent
            Vector3 train_destination;
            Boxcar boxcar = gameObject.GetComponent<Boxcar>();
            if (gameObject.tag == "boxcar") up_multiplier = boxcar.train.get_distance_from_train(boxcar.boxcar_id); // disappear off screen
            if (exit_track_orientation == RouteManager.Orientation.West || exit_track_orientation == RouteManager.Orientation.East)
                up_multiplier += 7;
            else if (exit_track_orientation == RouteManager.Orientation.North)
                up_multiplier += 3;
            else if (exit_track_orientation == RouteManager.Orientation.South)
                up_multiplier += 4;
            train_destination = transform.up * up_multiplier * RouteManager.cell_width + transform.position;
            StartCoroutine(straight_move(transform.position, train_destination, false, true)); // turn on exit city flag
        }
    }

    public override void set_initial_rotation(RouteManager.Orientation orientation)
    {
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                transform.eulerAngles = new Vector3(0, 0, 0);
                break;
            case RouteManager.Orientation.East:
                transform.eulerAngles = new Vector3(0, 0, -90);
                break;
            case RouteManager.Orientation.West:
                transform.eulerAngles = new Vector3(0, 0, 90);
                break;
            case RouteManager.Orientation.South:
                transform.eulerAngles = new Vector3(0, 0, 180);
                break;
        }
    }

    public void hide_explosion(List<GameObject> explosion_list)
    {
        foreach (GameObject explosion_go in explosion_list)
        {
            SpriteRenderer[] explosion_sprite_arr = explosion_go.GetComponentsInChildren<SpriteRenderer>();
            for (int c = 0; c < explosion_sprite_arr.Length; c++)
            {
                explosion_sprite_arr[c].enabled = false;
            }
        }
    }

    public void remove_vehicle_from_board()
    {
        if (in_city)
        {
            if (city.city_board[tile_position.x + 1, tile_position.y + 1] == null) //print("vehicle not found in city");
            city.city_board[tile_position.x + 1, tile_position.y + 1] = null;
        }
        else
        {
            if (city.city_board[tile_position.x + 1, tile_position.y + 1] == null) //print("vehicle not found in game view");
            VehicleManager.vehicle_board[tile_position.x + 1, tile_position.y + 1] = null;
        }
    }

    public bool is_end_of_track()
    {
        // assume we are NOT in city
        Tilemap track_tilemap = TrackManager.instance.top_tilemap;
        Tile next_tile = (Tile)track_tilemap.GetTile((Vector3Int)next_tilemap_position);
        Tile cur_tile = (Tile)track_tilemap.GetTile((Vector3Int)tile_position);
        if (next_tile == null)
        {
            GameObject city_object = CityManager.instance.get_city(next_tilemap_position);
            if (city_object == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (TrackManager.is_track_a_path(orientation, next_tile.name, cur_tile.name)) return false;
            else
            {
                return true;
            }
        }
    }

    public IEnumerator switch_on_vehicle(bool state, bool is_delayed = false)
    {
        if (is_delayed)
        {
            yield return new WaitForSeconds(sprite_renderer_delay);
            // if in the meantime, (during the delay) the game menu was toggled, mmake sure consistent with game menu state
            if (!GameManager.game_menu_state)
            {
                //print("game menu toggled OFF during delay. dont show BOXCAR sprite renderer");
                yield break;
            }
        }            
        else {
            yield return null;
        }
        if (gameObject.tag == "boxcar")
        {
            Boxcar boxcar = gameObject.GetComponent<Boxcar>();
            if (boxcar.is_occupied)
            {
                boxcar.passenger_go.GetComponent<SpriteRenderer>().enabled = state;
            }
        }
        if (gameObject.tag == "boxcar")
        {
            //print("boxcar " + gameObject.GetComponent<Boxcar>().boxcar_id + " sprite renderer state is " + state);
        }
        GetComponent<SpriteRenderer>().enabled = state;
    }

    public void reset_departure_flag()
    {
        depart_for_turntable = true;
        leave_turntable = false;
        leave_city = false;
        complete_exit = false; // on verge of departing city
        departure_track_chosen = false;
        is_boxcar_stopped = false;
        //is_instantiated = false;
        go_to_turntable = false;
    }

    public virtual void arrive_at_city()
    {
        in_city = true;
    }

    public IEnumerator prepare_for_departure()
    {
        Vector3 vehicle_departure_point = TrainRouteManager.get_city_boundary_location(tile_position, orientation); // tile pos is 3,6 not 10,6
        if (in_city) next_tilemap_position = BoardManager.pos_to_tile(vehicle_departure_point);
        is_halt = true; // NO MORE MOVEMNET UPDATES UJUST FINISH THE CURRENT ONE SO WE CAN PREPARE FOR DEPART
        while (in_tile) // wati for prev movment to complete 
        {
            yield return new WaitForEndOfFrame();
        }
        in_tile = true; // allow vehicle to move to the border of tile before resuming its route
        is_halt = false; // indicates a vehicle is about to leave
        StartCoroutine(straight_move(transform.position, vehicle_departure_point));
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        ////print("inside Moving Object class");
    }

    public void set_position(Vector3Int tile_position)
    {
        this.tile_position = tile_position;
    }

    public void set_orientation(RouteManager.Orientation orientation)
    {
        this.orientation = orientation;
    }

    public IEnumerator one_time_straight_move(Boxcar prev_boxcar)
    {
        Vector3Int prev_boxcar_position = prev_boxcar.tile_position;
        //Vector3Int prev_boxcar_prev_tile_position = prev_boxcar.prev_tile_position;
        //Vector2Int prevboxcar_next_tilemap_position = prev_boxcar.next_tilemap_position;
        RouteManager.Orientation prev_orientation = prev_boxcar.orientation;
        Vector3 prev_destination = prev_boxcar.train_destination;
        //one_time_move_pass = true; // lets boxcar move  even though train hasnt departed yet. 
        yield return StartCoroutine(simple_straight_move(transform.position, prev_boxcar.transform.position)); // dont set new positions until movement is completed
        //one_time_move_pass = false;
        tile_position = prev_boxcar_position;
        next_tilemap_position = (Vector2Int) prev_boxcar_position;
        prev_tile_position = prev_boxcar_position;
        orientation = prev_orientation;
        train_destination = prev_destination;
    }

    public IEnumerator one_time_bezier_move(Boxcar prev_boxcar)
    {
        yield return StartCoroutine(bezier_move(transform, orientation, prev_boxcar.orientation));
        orientation = prev_boxcar.orientation;
        tile_position = prev_boxcar.tile_position;
        next_tilemap_position = prev_boxcar.next_tilemap_position;
    }

    public IEnumerator wait_for_turntable(string track_name)
    {
        if (depart_for_turntable && !leave_turntable)
        {
            if (gameObject.name == "train(Clone)" && !gameObject.GetComponent<Train>().is_train_departed_for_turntable)
            {
                Train train = gameObject.GetComponent<Train>();
                // gameObject.GetComponent<Train>().halt_train(false, true); // TODOED1 will pause the train until the turntable has arrived
                // wait for train's turn
                train.station_track.train = gameObject; // assign train to station track after it has stopped. If it were set upon entrance, it could overwrite an existing train at this track
                train.is_pause = true; // TODOED1 remove
                train.set_boxcar_wait_flag(true);
                while (true)
                {
                    bool is_train_turn = city.turn_table.GetComponent<Turntable>().is_train_turn(gameObject);
                    if (is_train_turn && !PauseManager.game_is_paused)
                    {
                        city.remove_train_from_station(gameObject);
                        gameObject.GetComponent<Train>().set_boxcar_wait_flag(false);
                        break;
                    } 
                    else
                        yield return new WaitForEndOfFrame();
                }
                RouteManager.Orientation orio = TrackManager.get_steep_orientation(track_name); // trigger bezier move along steep path
                city.turn_turntable(gameObject, orio, depart_for_turntable); // halt the boxcar and train
            }
        }
    }

    public void stop_single_car_if_wait_tile(bool is_inner)
    {
        if (is_inner)
        {
            if (random_algos.list_contains_arr(CityManager.boxcar_city_inner_wait_tile, tile_position))
            {
                print("INNER WAIT TILE ENCOUNTERED AT " + tile_position + " STOP ALLL BOXCAR");
                GetComponent<Boxcar>().train.stop_single_boxcar_at_turntable(gameObject);
                GetComponent<Boxcar>().is_boxcar_stopped = true;
            }
        }
        else
        {
            if (random_algos.list_contains_arr(CityManager.boxcar_city_outer_wait_tile, tile_position))
            {
                print("OUTER WAIT TILE ENCOUNTERED AT " + tile_position + " STOP ALLL BOXCAR");
                GetComponent<Boxcar>().train.stop_single_boxcar_at_turntable(gameObject);
                GetComponent<Boxcar>().is_boxcar_stopped = true;
            }
        }
    }

    public void stop_car_if_wait_tile()
    {       
        if (!gameObject.GetComponent<Boxcar>().train.is_train_departed_for_turntable && in_city)
        {
            Boxcar boxcar = gameObject.GetComponent<Boxcar>();
            int track_location_idx = boxcar.station_track.inner; // 0 means outer track
            int orientation_idx = boxcar.station_track.station.orientation_to_idx();
            int boxcar_pos_idx = boxcar.train.get_boxcar_position(gameObject) - 1;
            if (boxcar_pos_idx == 0) print(tile_position);
            int[] boxcar_wait_tile = CityManager.boxcar_city_wait_tile[orientation_idx][track_location_idx][boxcar_pos_idx];
            if (tile_position.x == boxcar_wait_tile[0] && tile_position.y == boxcar_wait_tile[1])
            {
                boxcar.train.GetComponent<CapsuleCollider2D>().size = new Vector2(.205f, .837f);
                print("stopping boxcar at position " + boxcar_pos_idx + " at wait tile " + boxcar_wait_tile);
                boxcar.train.stop_single_boxcar_at_turntable(gameObject);
                is_boxcar_stopped = true;
            }
        }
    }

    public override IEnumerator bezier_move(Transform location, RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
    {
        Vector2 position = location.position;
        float t_param;
        t_param = 1;
        bool final_step = false;
        float start_angle = location.eulerAngles[2]; // rotation about z axis
        float end_angle;
        RouteManager.Orientation curve_type = TrackManager.is_curve_steep(final_orientation);
        in_tile = true;
        float original_speed = speed;
        if (is_fill_void)
            speed = normal_speed;
        if (curve_type == RouteManager.Orientation.Less_Steep_Angle || curve_type == RouteManager.Orientation.Steep_Angle) // turntable adjustements
        {
            steep_angle_orientation = final_orientation;
            end_angle = start_angle + TrackManager.get_steep_angle_rotation(final_orientation);
            depart_for_turntable = false;
            leave_turntable = true;
        }
        else // a 90 degree curve
        {
            end_angle = start_angle + TrackManager.get_rotation(orientation, final_orientation); //end_angle is a static field for steep curves
        }

        while (!final_step)
        {

            if (is_pause)
            {
                yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
                continue; // don't execute the code below
            }
            if (gameObject.tag == "boxcar")
            {
                bool is_boxcar_stopped = gameObject.GetComponent<Boxcar>().is_boxcar_stopped;
                if (!is_boxcar_stopped) // one time boolean flag only execute once
                {
                    stop_car_if_wait_tile(); // stop all boxcars if wait tile
                    //gameObject.GetComponent<Boxcar>().stop_single_boxcar();
                }
            }
            float interp = 1.0f - t_param;
            t_param -= Time.deltaTime * speed; // use the speed multiplier when boxcar is first instantiated due to gap between train and lead boxcar
            if (t_param < 0) // set t_param to 0 to get bezier coordinates closer to the destination (and be within tolerance)
            {
                interp = 1;
                t_param = 0;
                final_step = true;
            }
            float angle = Mathf.LerpAngle(start_angle, end_angle, interp); // interpolate from [0,1]
            if (curve_type == RouteManager.Orientation.Steep_Angle)
                next_position = bezier_equation(t_param, TrackManager.steep_curve);
            else if (curve_type == RouteManager.Orientation.Less_Steep_Angle)
                next_position = bezier_equation(t_param, TrackManager.less_steep_curve);
            else { next_position = bezier_equation(t_param, TrackManager.right_angle_curve); }
            next_position = transform_curve(next_position, orientation, final_orientation) + position; //offset with current position
            transform.position = next_position;
            location.eulerAngles = new Vector3(0, 0, angle);
            yield return new WaitForEndOfFrame();
        }
        in_tile = false;
        if (gameObject.tag == "boxcar" && gameObject.GetComponent<Boxcar>().boxcar_id == 3)
        {
            //print("FINISH BOXCAR 3 BEZIER MOVE FROM " + tile_position + " TO " + next_tilemap_position);
        }
        if (is_fill_void)
        {
            speed = original_speed;
            is_fill_void = false;
        }            
    }

    public IEnumerator simple_straight_move(Vector2 start_position, Vector2 destination)
    {
        float og_distance = Vector2.Distance(start_position, destination); // distance to destination
        float distance = og_distance;
        next_position = start_position;
        while (distance > GameManager.tolerance)
        {
            float step = normal_speed * Time.deltaTime; // calculate distance to move
            next_position = Vector2.MoveTowards(next_position, destination, step);
            transform.position = next_position;
            distance = Vector2.Distance(next_position, destination);
            yield return new WaitForEndOfFrame();
        }
        is_fill_void = false;
    }

    public IEnumerator straight_move(Vector2 start_position, Vector2 destination, bool turntable_dest = false, bool exit_dest = false)
    {
        float og_distance = Vector2.Distance(start_position, destination); // distance to destination
        float distance = og_distance;
        in_tile = true;
        next_position = start_position;
        float original_speed = speed;
        if (is_fill_void)
            speed = normal_speed;
        while (distance > GameManager.tolerance)
        {
            if (is_pause)
            {
                yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling is_inventory
                continue; // don't execute the code below
            }
            if (gameObject.tag == "boxcar")
            {
                Boxcar boxcar = gameObject.GetComponent<Boxcar>();
                if (!boxcar.train.is_train_departed_for_turntable) // only stop boxcar if train is stationary. 
                {
                    stop_car_if_wait_tile();
                }                
            }
            float step = speed * Time.deltaTime; // calculate distance to move
            next_position = Vector2.MoveTowards(next_position, destination, step);
            transform.position = next_position;
            distance = Vector2.Distance(next_position, destination);
            yield return new WaitForEndOfFrame();
        }
        if (turntable_dest) // train reaches other end of turntable
        {
            // wait for user input to choose depart track
            // city knows which train is on the turntable

            while (exit_track_orientation == RouteManager.Orientation.None)
                yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
            if (gameObject.tag == "train")
            {
                Train train = gameObject.GetComponent<Train>();
                train.halt_train(true, true); // prevent orientation from being updated with last tile position (eg se_diag) while rotating
                while (!train.is_all_car_reach_turntable())
                {
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(1);
                city.turn_turntable(gameObject, exit_track_orientation); // turntable turns to destination track
            }
            else // reset boxcar speed multiplier after it has been created
            {
                is_halt = true; // otherwise the boxcar wont spin with the turntable
                speed_multiplier = 1.0f;
            }
        }
        in_tile = false;
        if (exit_dest)
        {
            in_city = false;
            orientation = exit_track_orientation;
            final_orientation = orientation;
            if (gameObject.tag == "train")
            {
                city.delete_train(gameObject);
                prev_city = city;
                Vector3Int city_location = city.get_location();
                VehicleManager.instance.depart(gameObject, city_location);
                city.turn_table.GetComponent<Turntable>().remove_train_from_queue(gameObject);
                game_manager.enable_vehicle_for_screen(gameObject);
                gameObject.GetComponent<Train>().set_boxcar_to_depart(); // set depart = true so boxcars leave city
            }
            else
            {
                gameObject.GetComponent<Boxcar>().receive_train_order = false; // reset 
            }
            prev_city = city;
            next_tilemap_position = TrainRouteManager.get_depart_tile_position(orientation, city.get_location()); // otherwise get stuck on track
            exit_track_orientation = RouteManager.Orientation.None;
            steep_angle_orientation = RouteManager.Orientation.None;
            reset_departure_flag();
            // disable until given the goahead
        }
        if (arriving_in_city) 
        {
            is_halt = true; // NO MREO UPDATES UNTNIL THE TRAIN IS IN NEW CITY LOCATION OTHERWISE IT WILL DERP AND TRY TO GO TO INVALID SPACE
            arrive_at_city();
            arriving_in_city = false;
        }
        if (is_fill_void)
        {
            speed = original_speed;
            is_fill_void = false;
        }            
    }
}