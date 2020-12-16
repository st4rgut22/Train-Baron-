using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Tilemaps;

public class MovingObject : Simple_Moving_Object
{
    // all the vehicle movement math goes here
    Vector2 stranded_state = new Vector2(-10, -10); // waiting for a new track or turntable
    public bool is_pause = false; // pause state
    public bool is_halt = false;

    public bool depart_for_turntable = true;
    public bool leave_turntable = false;
    public bool leave_city = false;
    public bool complete_exit = false; // on verge of departing city
    public bool departure_track_chosen = false;
    public bool end_of_track = false;
    public string train_name = "train(Clone)";
    VehicleManager vehicle_manager;
    public RouteManager.Orientation exit_track_orientation = RouteManager.Orientation.None;
    public RouteManager.Orientation steep_angle_orientation = RouteManager.Orientation.None;

    // Start is called before the first frame update
    public void Awake()
    {
        //is_halt = false;
        Vector2Int home_base = CityManager.home_base_location;
        tile_position = new Vector3Int(home_base.x, home_base.y, 0);
        next_tilemap_position = home_base;
        prev_city = null;
        orientation = VehicleManager.round_robin_orientation(); // TEMPORARY, TESTING create train display!
        //orientation = RouteManager.Orientation.North; // direction of incoming train 
        final_orientation = orientation;
    }

    public override void Start()
    {
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        target_position = transform.position;
    }

    // Update is called once per frame
    public override void Update()
    {
        if (!is_halt)
        {
            if (!in_tile && !end_of_track) // Completed tile route. update destination to next tile. Prevents repeated calls to StartCoroutine()
            {
                orientation = final_orientation; // updating the orientation at every new tile
                Vector3Int prev_tile_position = tile_position;
                tile_position = new Vector3Int(next_tilemap_position.x, next_tilemap_position.y, 0);
                PositionPair position_pair;
                if (!in_city)
                {
                    Tilemap toggled_tilemap = GameManager.track_manager.top_tilemap;
                    GameManager.vehicle_manager.update_vehicle_board(VehicleManager.vehicle_board, gameObject, tile_position, prev_tile_position);
                    position_pair = RouteManager.get_destination(this, toggled_tilemap, offset); // set the final orientation and destination
                }
                else
                {
                    GameManager.vehicle_manager.update_vehicle_board(city.city_board, gameObject, tile_position, prev_tile_position);
                    position_pair = RouteManager.get_destination(this, station_track.tilemap, offset); // set the final orientation and destination
                }
                Vector2 train_dest_xy = position_pair.abs_dest_pos;
                next_tilemap_position = position_pair.tile_dest_pos;
                // stop the train here if track ends
                if (!in_city && gameObject.tag == "train" && is_end_of_track() && !is_pause)
                {
                    StartCoroutine(gameObject.GetComponent<Train>().wait_for_track_placement(next_tilemap_position));
                    gameObject.GetComponent<Train>().halt_train(false, true);
                    end_of_track = true;
                }         
                Vector3 train_destination = new Vector3(train_dest_xy[0], train_dest_xy[1], z_pos);
                 if (orientation != final_orientation) // curved track
                    StartCoroutine(bezier_move(transform, orientation, final_orientation));
                else // straight track
                {
                    if (leave_turntable) // go to end of turn table in straight line
                    {
                        // train travels to the other end of the turntable
                        leave_turntable = false;
                        float distance_multiplier = 4.5f;
                        if (gameObject.tag == "boxcar")
                        {
                            Boxcar boxcar = gameObject.GetComponent<Boxcar>();
                            distance_multiplier -= boxcar.train.get_distance_from_train(boxcar.boxcar_id); // offset boxcar n units away from train
                        }
                        train_destination = transform.up * distance_multiplier * RouteManager.cell_width + transform.position; // 5 units in the direction the train is facing
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
        }
        if (leave_city) // train is ready to depart from city
        {
            int up_multiplier = 0;
            leave_city = false;
            //city.set_destination_track(RouteManager.Orientation.None); // reset the destination track for future trains
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

    public bool is_end_of_track()
    {
        Tilemap next_tilemap = GameManager.track_manager.top_tilemap;
        if (next_tilemap.GetTile((Vector3Int)next_tilemap_position) == null)
        {
            GameObject city_object = GameManager.city_manager.get_city(next_tilemap_position);
            if (city_object == null)
            {
                return true;
            }
        }
        return false;
    }

    public static void switch_sprite_renderer(GameObject vehicle_object, bool state)
    {
        if (vehicle_object.tag == "boxcar")
        {
            Boxcar boxcar = vehicle_object.GetComponent<Boxcar>();
            if (boxcar.is_occupied)
                boxcar.passenger_go.GetComponent<SpriteRenderer>().enabled = state;
        }
        vehicle_object.GetComponent<SpriteRenderer>().enabled = state;
    }

    public void reset_departure_flag()
    {
        depart_for_turntable = true;
        leave_turntable = false;
        leave_city = false;
        complete_exit = false; // on verge of departing city
        departure_track_chosen = false;
    }

    public void initialize_orientation(RouteManager.Orientation orientation)
    {
        // when leaving a city, vehicle orientation be initialized to final orientation (which the user chooses)
        final_orientation = orientation;
    }

    public virtual void arrive_at_city()
    {
        in_city = true;
    }

    public void set_halt(bool is_halt)
    {
        //print("setting " + gameObject.tag + " halt to " + is_halt);
        this.is_halt = is_halt;
    }

    public void prepare_for_departure()
    {
        in_tile = true; // allow vehicle to move to the border of tile before resuming its route
        set_halt(false); // indicates a vehicle is about to leave
        Vector3 vehicle_departure_point = TrainRouteManager.get_city_boundary_location(tile_position, orientation); // tile pos is 3,6 not 10,6
        if (in_city) next_tilemap_position = BoardManager.pos_to_tile(vehicle_departure_point);
        //print("Next tilemap position initialized to " + next_tilemap_position); // if not in city, don't overwrite boxcar's next tile pos
        //print("move " + gameObject.name + " from " + transform.position + " to " + vehicle_departure_point);
        StartCoroutine(straight_move(transform.position, vehicle_departure_point));
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        //print("inside Moving Object class");
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
        yield return StartCoroutine(straight_move(transform.position, prev_boxcar.transform.position)); // dont set new positions until movement is completed
        tile_position = prev_boxcar.tile_position;
        next_tilemap_position = prev_boxcar.next_tilemap_position;
        orientation = prev_boxcar.orientation;
    }

    public IEnumerator one_time_bezier_move(Boxcar prev_boxcar)
    {
        yield return StartCoroutine(bezier_move(transform, orientation, prev_boxcar.orientation));
        orientation = prev_boxcar.orientation;
        tile_position = prev_boxcar.tile_position;
        next_tilemap_position = prev_boxcar.next_tilemap_position;
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
        if (curve_type==RouteManager.Orientation.Less_Steep_Angle || curve_type== RouteManager.Orientation.Steep_Angle) // turntable adjustements
        {
            steep_angle_orientation = final_orientation;
            if (depart_for_turntable && !leave_turntable)
            {
                if (gameObject.name == "train(Clone)")
                {
                    Train train = gameObject.GetComponent<Train>();
                    gameObject.GetComponent<Train>().halt_train(false, true); // will pause the train until the turntable has arrived
                    // wait for train's turn
                    while (true)
                    {
                        bool is_train_turn = city.turn_table.GetComponent<Turntable>().is_train_turn(gameObject);
                        if (is_train_turn) break;
                        else
                            yield return new WaitForEndOfFrame();
                    }
                    city.turn_turntable(gameObject, final_orientation, depart_for_turntable);
                }
                depart_for_turntable = false;
                leave_turntable = true;
            }
            end_angle = start_angle + TrackManager.get_steep_angle_rotation(final_orientation);
        }
        else // a 90 degree curve
        {
            end_angle = start_angle + TrackManager.get_rotation(orientation, final_orientation); //end_angle is a static field for steep curves
        }
        //print("Start angle is " + start_angle + " End angle is " + end_angle);
        while (!final_step)
        {
            if (is_pause)
            {
                yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
                continue; // don't execute the code below
            }
            float interp = 1.0f - t_param;
            if (gameObject.tag=="boxcar")
                t_param -= Time.deltaTime * speed * speed_multiplier; // use the speed multiplier when boxcar is first instantiated due to gap between train and lead boxcar
            else
            {
                t_param -= Time.deltaTime * speed; 
            }

            if (t_param < 0) // set t_param to 0 to get bezier coordinates closer to the destination (and be within tolerance)
            {
                interp = 1;
                t_param = 0;
                final_step = true;
            }
            float angle = Mathf.LerpAngle(start_angle, end_angle, interp); // interpolate from [0,1]
            if (curve_type==RouteManager.Orientation.Steep_Angle)
                next_position = bezier_equation(t_param, TrackManager.steep_curve);
            else if (curve_type==RouteManager.Orientation.Less_Steep_Angle)
                next_position = bezier_equation(t_param, TrackManager.less_steep_curve);
            else { next_position = bezier_equation(t_param, TrackManager.right_angle_curve); }
            next_position = transform_curve(next_position, orientation, final_orientation) + position; //offset with current position
            transform.position = next_position;
            location.eulerAngles = new Vector3(0, 0, angle);
            yield return new WaitForEndOfFrame();
        }
        in_tile = false;
    }

    public IEnumerator straight_move(Vector2 start_position, Vector2 destination, bool turntable_dest = false, bool exit_dest = false)
    {
        float og_distance = Vector2.Distance(start_position, destination); // distance to destination
        float distance = og_distance;
        in_tile = true;
        next_position = start_position;
        //if (gameObject.tag == "boxcar")
        //    print("Destination of Boxcar is " + destination);
        // Move our position a step closer to the target.
        while (distance > GameManager.tolerance)
        {
            if (is_pause) 
            {
                if (gameObject.tag != "boxcar" || !gameObject.GetComponent<Boxcar>().departing)
                {
                    yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
                    continue; // don't execute the code below
                }
            }
            float step;
            if (gameObject.tag == "boxcar")
                step = speed * Time.deltaTime * speed_multiplier; // calculate distance to move
            else
            {
                step = speed * Time.deltaTime; // calculate distance to move
            }
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
            //print("city destination orientation is " + exit_track_orientation);
            if (gameObject.tag == "train")
            {
                Train train = gameObject.GetComponent<Train>();
                train.halt_train(true, true); // prevent orientation from being updated with last tile position (eg se_diag) while rotating
                while (!train.is_all_car_reach_turntable()) 
                {
                    yield return new WaitForEndOfFrame();
                }
                city.turn_turntable(gameObject, exit_track_orientation); // turntable turns to destination track
            }
            else // reset boxcar speed multiplier after it has been created
            {
                set_halt(true); // otherwise the boxcar wont spin with the turntable
                speed_multiplier = 1.0f; 
            }
        }
        in_tile = false;
        //print(gameObject.name + " reached destination " + destination);
        if (exit_dest)
        {
            in_city = false;
            city.remove_train_from_station(gameObject);
            orientation = exit_track_orientation;
            final_orientation = orientation;
            if (gameObject.tag == "train")
            {
                vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
                city.delete_train(gameObject);
                prev_city = city;
                Vector3Int city_location = city.get_location();
                vehicle_manager.depart(gameObject, city_location);
                city.turn_table.GetComponent<Turntable>().remove_train_from_queue(gameObject);
                GameManager.enable_vehicle_for_screen(gameObject);
                gameObject.GetComponent<Train>().set_boxcar_to_depart(); // set depart = true so boxcars leave city
                if (city==CityManager.Activated_City_Component) GameManager.train_menu_manager.update_train_menu(city);
                //print("after moving to city edge. the train tile position is " + next_tilemap_position);// depart train at correct tile position
            } else
            {
                gameObject.GetComponent<Boxcar>().receive_train_order = false; // reset 
                gameObject.GetComponent<Boxcar>().departing = true;
                //city.delete_boxcar(gameObject);
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
            arrive_at_city();
            arriving_in_city = false;
        }
    }
}
