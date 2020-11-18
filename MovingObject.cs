using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Tilemaps;

public class MovingObject : EventDetector
{
    // all the vehicle movement math goes here

    protected Vector2 target_position;
    protected float speed = 2f; // Temporary. changed from 2f
    protected float speed_multiplier = 1f;
    protected float tolerance = .004f;
    protected Vector2 next_position;
    protected bool in_tile = false; // if an object is in a tile, it is already has a destination

    public RouteManager.Orientation orientation; // orientation before moving to a new tile
    public RouteManager.Orientation final_orientation; // orientation after moving to a new tile 
    public RouteManager.Orientation prev_orientation; // orientation during the last idle state
    public RouteManager.Orientation depart_city_orientation = RouteManager.Orientation.None;
    public Vector3Int tile_position;
    public Vector2Int next_tilemap_position;
    protected const float z_pos = 0;

    public bool arriving_in_city = false; // next tile position is a city. upon movement completion set in_city=true
    public bool in_city;        //in_city = false; 
    public City city;
    public City prev_city; // used to check whether a city destination is not in fact the city youve just left
    protected CityManager city_manager;
    protected GameManager game_manager;

    public Station_Track station_track;

    Vector2 stranded_state = new Vector2(-10, -10); // waiting for a new track or turntable
    public bool is_pause = false; // pause state
    public bool is_halt = true;

    public bool depart_for_turntable = true;
    public bool leave_turntable = false;
    public bool leave_city = false;
    public bool complete_exit = false; // on verge of departing city
    public bool departure_track_chosen = false;
    public string train_name = "train(Clone)";
    VehicleManager vehicle_manager;
    public RouteManager.Orientation exit_track_orientation = RouteManager.Orientation.None;
    public RouteManager.Orientation steep_angle_orientation = RouteManager.Orientation.None;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        Vector2Int home_base = CityManager.home_base_location;
        tile_position = new Vector3Int(home_base.x, home_base.y, 0);
        next_tilemap_position = home_base;
        prev_city = null;
        orientation = VehicleManager.round_robin_orientation(); // TEMPORARY, TESTING create train display!
        //orientation = RouteManager.Orientation.East; // RESTORE ! 
        final_orientation = orientation;
    }

    public virtual void Start()
    {
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        target_position = transform.position;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (!is_halt)
        {
            if (!in_tile) // Completed tile route. update destination to next tile. Prevents repeated calls to StartCoroutine()
            {
                orientation = final_orientation; // updating the orientation at every new tile
                Vector3Int prev_tile_position = tile_position;
                tile_position = new Vector3Int(next_tilemap_position.x, next_tilemap_position.y, 0);
                PositionPair position_pair;
                if (!in_city)
                {
                    Tilemap toggled_tilemap = GameManager.track_manager.get_toggled_tilemap((Vector2Int)tile_position);
                    print("toggled tilemap is " + toggled_tilemap + " at " + tile_position);
                    if (tile_position.Equals(new Vector3Int(8, 6, 0))){
                        print("break");
                    }
                    GameManager.vehicle_manager.update_vehicle_board(VehicleManager.vehicle_board, gameObject, tile_position, prev_tile_position);
                    position_pair = RouteManager.get_destination(this, toggled_tilemap); // set the final orientation and destination
                }
                else
                {
                    GameManager.vehicle_manager.update_vehicle_board(city.city_board, gameObject, tile_position, prev_tile_position);
                    position_pair = RouteManager.get_destination(this, station_track.tilemap); // set the final orientation and destination
                }
                Vector2 train_dest_xy = position_pair.abs_dest_pos;
                next_tilemap_position = position_pair.tile_dest_pos;
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
                        if (gameObject.tag == "boxcar") distance_multiplier -= gameObject.GetComponent<Boxcar>().boxcar_id; // offset boxcar n units away from train
                        train_destination = transform.up * distance_multiplier * RouteManager.cell_width + transform.position; // 5 units in the direction the train is facing
                        //}
                        if (gameObject.tag == "train") gameObject.transform.parent = gameObject.GetComponent<Train>().city.turn_table.transform; // make train child of turntable so it rotates with it
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
            if (gameObject.tag == "boxcar") up_multiplier = gameObject.GetComponent<Boxcar>().boxcar_id; // disappear off screen
            if (exit_track_orientation == RouteManager.Orientation.West || exit_track_orientation == RouteManager.Orientation.East)
                up_multiplier += 6;
            else if (exit_track_orientation == RouteManager.Orientation.North)
                up_multiplier += 3;
            else if (exit_track_orientation == RouteManager.Orientation.South)
                up_multiplier += 3;
            train_destination = transform.up * up_multiplier * RouteManager.cell_width + transform.position;
            StartCoroutine(straight_move(transform.position, train_destination, false, true)); // turn on exit city flag
        }
    }

    public static void switch_sprite_renderer(GameObject vehicle_object, bool state)
    {
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

    public void prepare_to_arrive_at_city(City city)
    {
        if (city == null) throw new Exception("city shouldn't be null");
        this.city = city;
        arriving_in_city = true;
    }

    public virtual void arrive_at_city()
    {
        in_city = true;
    }

    public void prepare_for_departure()
    {
        in_tile = true; // allow vehicle to move to the border of tile before resuming its route
        is_halt = false; // indicates a vehicle is about to leave
        Vector3 vehicle_departure_point = RouteManager.get_city_boundary_location(tile_position, orientation);
        if (in_city) next_tilemap_position = BoardManager.pos_to_tile(vehicle_departure_point);
        print("Next tilemap position initialized to " + next_tilemap_position); // if not in city, don't overwrite boxcar's next tile pos
        print("move " + gameObject.name + " from " + transform.position + " to " + vehicle_departure_point);
        StartCoroutine(straight_move(transform.position, vehicle_departure_point));
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        print("inside Moving Object class");
    }

    public void set_position(Vector3Int tile_position)
    {
        this.tile_position = tile_position;
    }

    public void set_orientation(RouteManager.Orientation orientation)
    {
        this.orientation = orientation;
    }

    Vector2 transform_curve(Vector2 bezier_position, RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
    {
        // transform the coord of bezier curve so it matches shape of appropriate track
        if (orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.East)
        {
            return bezier_position;
        } else if ((orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.East) ||
                    final_orientation == RouteManager.Orientation.ne_SteepCurve || final_orientation == RouteManager.Orientation.ne_LessSteepCurve)
        {
            bezier_position[1] = -bezier_position[1];
        } else if ((orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.West) ||
            final_orientation == RouteManager.Orientation.sw_SteepCurve || final_orientation == RouteManager.Orientation.sw_LessSteepCurve)
        {
            bezier_position[0] = -bezier_position[0];
        } else if ((orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.West) ||
            final_orientation == RouteManager.Orientation.nw_SteepCurve || final_orientation == RouteManager.Orientation.nw_LessSteepCurve)
        {
            bezier_position[0] = -bezier_position[0];
            bezier_position[1] = -bezier_position[1];
        } else if (orientation == RouteManager.Orientation.East && final_orientation == RouteManager.Orientation.South)
        { // rotate curve 90 degrees counter clockwise
            float temp_x = bezier_position[0];
            bezier_position[0] = bezier_position[1];
            bezier_position[1] = -temp_x;
        } else if (orientation == RouteManager.Orientation.East && final_orientation == RouteManager.Orientation.North)
        {
            float temp_x = bezier_position[0];
            bezier_position[0] = bezier_position[1];
            bezier_position[1] = temp_x;
        } else if (orientation == RouteManager.Orientation.West && final_orientation == RouteManager.Orientation.South)
        {
            float temp_x = bezier_position[0];
            bezier_position[0] = -bezier_position[1];
            bezier_position[1] = -temp_x;
        }
        else if (orientation == RouteManager.Orientation.West && final_orientation == RouteManager.Orientation.North)
        {
            float temp_x = bezier_position[0];
            bezier_position[0] = -bezier_position[1];
            bezier_position[1] = temp_x;
        } else
        {
            print(gameObject.name + " none of the orientations match"); // bezier coordinates returned unchanged
        }
        return bezier_position;
    }

    //public void leave_city()
    //{
    //    // restore flags to defaults
    //    // depart_turntable = true
    //    // leave_turntable = false
    //}

    Vector2 bezier_equation(float t_param, List<Vector2> bezier_curve)
    {
        next_position = Mathf.Pow(1 - t_param, 3) * bezier_curve[0] +
            3 * Mathf.Pow(1 - t_param, 2) * t_param * bezier_curve[1] +
            3 * (1 - t_param) * Mathf.Pow(t_param, 2) * bezier_curve[2] +
            Mathf.Pow(t_param, 3) * bezier_curve[3];
        return next_position;
    }

    protected IEnumerator bezier_move(Transform location, RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
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
            end_angle = start_angle + TrackManager.get_right_angle_rotation(orientation, final_orientation); //end_angle is a static field for steep curves
        }
        print("Start angle is " + start_angle + " End angle is " + end_angle);
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

    protected IEnumerator straight_move(Vector2 start_position, Vector2 destination, bool turntable_dest = false, bool exit_dest = false)
    {
        float og_distance = Vector2.Distance(start_position, destination); // distance to destination
        float distance = og_distance;
        in_tile = true;
        next_position = start_position;
        if (gameObject.tag == "boxcar")
            print("Destination of Boxcar is " + destination);
        // Move our position a step closer to the target.
        while (distance > tolerance)
        {
            if (is_pause) 
            {
                yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
                continue; // don't execute the code below
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
            print("city destination orientation is " + exit_track_orientation);
            //depart_city_orientation = city.destination_orientation; // unique to train!!!
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
                is_halt = true;
                speed_multiplier = 1.0f; 
            }
        }
        in_tile = false;
        print(gameObject.name + " reached destination " + destination);
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
            if (city==CityManager.Activated_City_Component) GameManager.train_menu_manager.update_train_menu(city);
                print("after moving to city edge. the train tile position is " + next_tilemap_position);// depart train at correct tile position
            } else
            {
                gameObject.GetComponent<Boxcar>().receive_train_order = false; // reset 
                city.delete_boxcar(gameObject);
            }
            prev_city = city;
            next_tilemap_position = RouteManager.get_depart_tile_position(orientation, city.get_location()); // otherwise get stuck on track
            exit_track_orientation = RouteManager.Orientation.None;
            steep_angle_orientation = RouteManager.Orientation.None;
            reset_departure_flag();
        }
        if (arriving_in_city)
        {
            arrive_at_city();
            arriving_in_city = false;
        }
    }
}
