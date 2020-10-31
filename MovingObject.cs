using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class MovingObject : EventDetector
{
    // all the vehicle movement math goes here

    protected Vector2 target_position;
    protected float speed = 1f;
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

    public bool in_city;        //in_city = false; TODO: Set False when leaving the shipyard
    public City city;

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

    // Start is called before the first frame update
    public virtual void Awake()
    {
        orientation = RouteManager.Orientation.East;
        final_orientation = orientation; 
        arrive_at_city(); // call immediately on instantiation. Otherwise, in_city = false and the wrong board is updated
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
                Vector2 train_dest_xy = transform.position; // default value
                PositionPair position_pair;
                if (!in_city)
                {
                    GameManager.vehicle_manager.update_vehicle_board(VehicleManager.vehicle_board, gameObject, tile_position, prev_tile_position);
                    position_pair = RouteManager.get_destination(this, RouteManager.track_tilemap); // set the final orientation and destination
                }
                else
                {
                    GameManager.vehicle_manager.update_vehicle_board(city.city_board, gameObject, tile_position, prev_tile_position);
                    position_pair = RouteManager.get_destination(this, station_track.tilemap); // set the final orientation and destination
                }
                if (position_pair.abs_dest_pos.Equals(stranded_state) && gameObject.name == train_name)
                {
                    gameObject.GetComponent<Train>().halt_train(true, true);
                    return; // dont move
                }
                train_dest_xy = position_pair.abs_dest_pos;
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
                        if (gameObject.tag=="boxcar") distance_multiplier -= gameObject.GetComponent<Boxcar>().boxcar_id; // offset boxcar n units away from train
                        print("distance multiplier is " + distance_multiplier);
                        train_destination = -transform.up * distance_multiplier * RouteManager.cell_width + transform.position; // 5 units in the direction the train is facing
                        if (gameObject.name == train_name) gameObject.transform.parent = gameObject.GetComponent<Train>().city.turn_table.transform; // make train child of turntable so it rotates with it
                        else { gameObject.transform.parent = gameObject.GetComponent<Boxcar>().city.turn_table.transform; }
                        StartCoroutine(straight_move(transform.position, train_destination, true)); // turn turntable on completion of movement
                    }
                    else
                    {
                        StartCoroutine(straight_move(transform.position, train_destination, false));
                    }
                    
                }
            }
        }
        if (leave_city) // train is ready to depart from city
        {
            leave_city = false;
            city.set_destination_track(RouteManager.Orientation.None); // reset the destination track for future trains
            gameObject.transform.parent = null; // decouple train from turntable parent
            Vector3 train_destination;
            if (depart_city_orientation == RouteManager.Orientation.West || depart_city_orientation == RouteManager.Orientation.East) train_destination = -transform.up * 6f * RouteManager.cell_width + transform.position;
            else { train_destination = -transform.up * 3f * RouteManager.cell_width + transform.position; }
            StartCoroutine(straight_move(transform.position, train_destination, false, true)); // turn on exit city flag
        }
    }

    public void initialize_orientation(RouteManager.Orientation orientation)
    {
        // when leaving a city, vehicle orientation be initialized to final orientation (which the user chooses)
        final_orientation = orientation;
    }

    public virtual void arrive_at_city()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false; // hide the vehicle when arrive at a city
        in_city = true;
    }

    public void prepare_for_departure()
    {        
        in_tile = true; // allow vehicle to move to the border of tile before resuming its route
        is_halt = false; // indicates a vehicle is about to leave
        Vector3 vehicle_departure_point = RouteManager.get_city_boundary_location(tile_position, orientation);
        print("departure point is " + vehicle_departure_point);
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
                    final_orientation == RouteManager.Orientation.ne_SteepCurve)
        {
            bezier_position[1] = -bezier_position[1];
        } else if ((orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.West) ||
            final_orientation == RouteManager.Orientation.se_SteepCurve)
        {
            bezier_position[0] = -bezier_position[0];
        } else if ((orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.West) ||
            final_orientation == RouteManager.Orientation.nw_SteepCurve)
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
            print("none of the orientations match"); // bezier coordinates returned unchanged
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
        bool curve_steep = RouteManager.is_curve_steep(final_orientation);
        if (curve_steep)
        {
            if (depart_for_turntable && !leave_turntable)
            {
                if (gameObject.name == "train(Clone)")
                {
                    gameObject.GetComponent<Train>().halt_train(false, true); // will pause the train until the turntable has arrived
                    city.turn_turntable(gameObject, final_orientation, depart_for_turntable);
                }
                depart_for_turntable = false;
                leave_turntable = true;
            }
            end_angle = start_angle + TrackManager.get_steep_angle_rotation(final_orientation);
        }
        else
        {
            end_angle = start_angle + TrackManager.get_right_angle_rotation(orientation, final_orientation); //end_angle is a static field for steep curves
        }
        print("Start angle is " + start_angle + " End angle is " + end_angle);
        in_tile = true;
        while (!final_step)
        {
            if (is_pause)
            {
                yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
                continue; // don't execute the code below
            }
            float interp = 1.0f - t_param;
            t_param -= Time.deltaTime * speed;
            if (t_param < 0) // set t_param to 0 to get bezier coordinates closer to the destination (and be within tolerance)
            {
                interp = 1;
                t_param = 0;
                final_step = true;
            }
            float angle = Mathf.LerpAngle(start_angle, end_angle, interp); // interpolate from [0,1]
            if (curve_steep) next_position = bezier_equation(t_param, TrackManager.steep_curve);
            else { next_position = bezier_equation(t_param, TrackManager.right_angle_curve); }
            next_position = transform_curve(next_position, orientation, final_orientation) + position; //offset with current position
            transform.position = next_position;
            location.eulerAngles = new Vector3(0, 0, angle);
            yield return new WaitForEndOfFrame();
        }
        in_tile = false;
    }

    protected IEnumerator straight_move(Vector2 start_position, Vector2 destination, bool turntable_dest=false, bool exit_dest=false)
    {
        in_tile = true;
        next_position = start_position;
        if (gameObject.tag == "boxcar")
            print("Destination of Boxcar is " + destination);
        // Move our position a step closer to the target.
        while (Vector2.Distance(next_position, destination) > tolerance)
        {
            if (is_pause)
            {
                yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
                continue; // don't execute the code below
            }
            float step = speed * Time.deltaTime; // calculate distance to move
            next_position = Vector2.MoveTowards(next_position, destination, step);
            transform.position = next_position;
            yield return new WaitForEndOfFrame();
        }
        if (turntable_dest) // train reaches other end of turntable
        {
            // wait for user input to choose depart track
            // city knows which train is on the turntable
            while (city.destination_orientation==RouteManager.Orientation.None)
                yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
            print("city destination orientation is " + city.destination_orientation);
            depart_city_orientation = city.destination_orientation;
            if (gameObject.name == train_name)
            {
                Train train = gameObject.GetComponent<Train>();
                while (!train.is_all_car_reach_turntable()) // wait for all boxcars to arrive before spinning the turntable
                {
                    yield return new WaitForEndOfFrame();
                }
                city.turn_turntable(gameObject, depart_city_orientation); // TODO: replace West with user input
                gameObject.GetComponent<Train>().halt_train(true, true); // prevent orientation from being updated with last tile position (eg se_diag) while rotating
            }
        }
        else if (exit_dest)
        {
            gameObject.GetComponent<Train>().halt_train(true, false); //allow train dest updates after it has left the city
            print("train is leaving the city. display it emerging from city in game view now");
        }
        in_tile = false;
    }
}
