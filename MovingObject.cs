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
    public bool in_motion = false; // condition to start an object in motion

    //bezier vertices for a SE turn
    Vector2 p0 = new Vector2(.5f, .5f);
    Vector2 p1 = new Vector2(.2f, .5f);
    Vector2 p2 = new Vector2(0, .3f);
    Vector2 p3 = new Vector2(0, 0);

    public RouteManager.Orientation orientation; // orientation before moving to a new tile
    public RouteManager.Orientation final_orientation; // orientation after moving to a new tile 
    public RouteManager.Orientation prev_orientation; // orientation during the last idle state
    public Vector3Int tile_position;
    protected const float z_pos = 0;
    protected bool reached_destination = false;
    string destination_type = ""; // get destination type. If city, then disable after reaching destination. 

    protected CityManager city_manager;
    protected GameManager game_manager;
    protected VehicleManager vehicle_manager;

    // Start is called before the first frame update
    public virtual void Start()
    {
        city_manager = GameObject.Find("CityManager").GetComponent<CityManager>();
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
        Vector2Int home_base = game_manager.get_home_base();
        tile_position = new Vector3Int(home_base.x, home_base.y, 0); //initialize every vehicle at home base
        target_position = transform.position;
        orientation = RouteManager.Orientation.North;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (in_motion)
        {
            if (!in_tile) // Reached tile. update destination to next tile
            {
                orientation = final_orientation;
                Vector3Int prev_tile_position = tile_position;
                if (orientation == RouteManager.Orientation.East)
                {
                    tile_position = new Vector3Int(Mathf.RoundToInt(transform.position[0]), (int)(transform.position[1]), 0);
                }
                else if (orientation == RouteManager.Orientation.West)
                {
                    tile_position = new Vector3Int(Mathf.RoundToInt(transform.position[0]) - 1, (int)(transform.position[1]), 0);
                }
                else if (orientation == RouteManager.Orientation.North)
                {
                    tile_position = new Vector3Int((int)(transform.position[0]), Mathf.RoundToInt(transform.position[1]), 0);
                }
                else if (orientation == RouteManager.Orientation.South)
                {
                    tile_position = new Vector3Int((int)(transform.position[0]), Mathf.RoundToInt(transform.position[1]) - 1, 0);
                }
                //print("Destination: " + destination_type);
                //if (destination_type.Equals("city"))
                //{
                //    gameObject.SetActive(false); // disable gameobject and components upon reaching the destination
                //    return;
                //}
                print("update " + gameObject.name + " position from " + prev_tile_position + " to " + tile_position);
                vehicle_manager.update_vehicle_board(gameObject, tile_position, prev_tile_position);
                Vector2 train_dest_xy = RouteManager.get_destination(this); // set the final orientation and destination
                Vector3 train_destination = new Vector3(train_dest_xy[0], train_dest_xy[1], z_pos);
                target_position = train_destination;
                if (orientation != final_orientation) // curved track
                    StartCoroutine(bezier_move(transform, orientation, final_orientation));
                else // straight track
                {
                    StartCoroutine(straight_move(transform.position, target_position));
                }
            }
            else // update position within a tile
            {
                transform.position = next_position;
            }
        } else // arrived at a city, or the end of the track. Save the final orientation 
        {
            // add train to city if arrived in city. update base
            destination_type = RouteManager.get_destination_type(tile_position); // get type of destination
            if (destination_type.Equals("city"))
            {
                if (gameObject.tag == "train") // if moving object is a train add it to the city it arrived at
                {
                    city_manager.add_train_to_board(tile_position, gameObject);
                    City city = CityManager.get_city(new Vector2Int(tile_position.x, tile_position.y)).GetComponent<City>();
                    gameObject.GetComponent<Train>().set_city(city);
                }
                //gameObject.SetActive(false); // disable gameobject and components upon reaching the destination
                return;
            }
            prev_orientation = orientation;
            //print("orientation in idle state is " + orientation);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        print("inside Moving Object class");
    }

    public void set_motion(bool is_in_motion)
    {
        in_motion = is_in_motion;
    }

    public bool is_in_motion()
    {
        return in_motion;
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
        } else if (orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.East)
        {
            bezier_position[1] = -bezier_position[1];
        } else if (orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.West)
        {
            bezier_position[0] = -bezier_position[0];
        } else if (orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.West)
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
            print("none of the orientations match");
        }
        return bezier_position;
    }

    float get_rotation(RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
    {
        // get angle of moving object along track based on start and end orientation
        if (orientation == RouteManager.Orientation.North && final_orientation==RouteManager.Orientation.East ||
            orientation == RouteManager.Orientation.South && final_orientation==RouteManager.Orientation.West ||
            orientation == RouteManager.Orientation.West && final_orientation==RouteManager.Orientation.North ||
            orientation == RouteManager.Orientation.East && final_orientation==RouteManager.Orientation.South)
        {
            return -90f; // clockwise
        }
        else
        {
            return 90f; // counterclockwise
        }
    }

    Vector2 bezier_equation(float t_param)
    {
        next_position = Mathf.Pow(1 - t_param, 3) * p0 +
            3 * Mathf.Pow(1 - t_param, 2) * t_param * p1 +
            3 * (1 - t_param) * Mathf.Pow(t_param, 2) * p2 +
            Mathf.Pow(t_param, 3) * p3;
        return next_position;
    }

    protected IEnumerator bezier_move(Transform location, RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
    {
        Vector2 position = location.position;
        float t_param;
        t_param = 1;
        bool final_step = false;
        float start_angle = location.eulerAngles[2]; // rotation about z axis
        float end_angle = start_angle + get_rotation(orientation, final_orientation);
        in_tile = true;
        while (!final_step)
        {
            if (!in_motion)
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
            next_position = bezier_equation(t_param);
            next_position = transform_curve(next_position, orientation, final_orientation) + position;
            location.eulerAngles = new Vector3(0, 0, angle);
            yield return new WaitForEndOfFrame();
        }
        in_tile = false;
        //print("angle after rotation is " + location.eulerAngles);
        //print("bezier position " + next_position.ToString("F6"));
    }

    protected IEnumerator straight_move(Vector2 start_position, Vector2 destination)
    {
        in_tile = true;
        next_position = start_position;
        // Move our position a step closer to the target.
        while (Vector2.Distance(next_position, destination) > tolerance)
        {
            if (!in_motion) yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
            float step = speed * Time.deltaTime; // calculate distance to move
            next_position = Vector2.MoveTowards(next_position, destination, step);
            yield return new WaitForEndOfFrame();
        }
        in_tile = false;
    }
}
