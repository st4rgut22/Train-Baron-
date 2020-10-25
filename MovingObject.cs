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
    public bool idling = true;

    //bezier vertices for a SE turn
    Vector2 p0 = new Vector2(.5f, .5f);
    Vector2 p1 = new Vector2(.2f, .5f);
    Vector2 p2 = new Vector2(0, .3f);
    Vector2 p3 = new Vector2(0, 0);

    public RouteManager.Orientation orientation; // orientation before moving to a new tile
    public RouteManager.Orientation final_orientation; // orientation after moving to a new tile 
    public RouteManager.Orientation prev_orientation; // orientation during the last idle state
    public Vector3Int tile_position;
    public Vector2Int next_tilemap_position;
    protected const float z_pos = 0;

    public bool in_city;
    public City city;

    protected CityManager city_manager;
    protected GameManager game_manager;

    // Start is called before the first frame update
    public virtual void Start()
    {
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Vector2Int home_base = BoardManager.home_base_location;
        target_position = transform.position;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (in_motion)
        {
            if (!in_tile) // Completed tile route. update destination to next tile
            {
                orientation = final_orientation; // updating the orientation at every new tile
                Vector3Int prev_tile_position = tile_position;
                tile_position = new Vector3Int(next_tilemap_position.x, next_tilemap_position.y, 0);
                print("update " + gameObject.name + " position from " + prev_tile_position + " to " + tile_position);
                Vector2 train_dest_xy = new Vector3(-1, -1); // default value
                if (!in_city)
                {
                    GameManager.vehicle_manager.update_vehicle_board(gameObject, tile_position, prev_tile_position);                    
                } else {
                    city.update_city_board(gameObject, tile_position, prev_tile_position);
                }
                PositionPair position_pair = RouteManager.get_destination(this); // set the final orientation and destination
                train_dest_xy = position_pair.abs_dest_pos;
                next_tilemap_position = position_pair.tile_dest_pos;
                Vector3 train_destination = new Vector3(train_dest_xy[0], train_dest_xy[1], z_pos);
                print("Next tilemap position is " + next_tilemap_position);
                print("next train destination is " + train_destination);
                target_position = train_destination;
                if (orientation != final_orientation) // curved track
                    StartCoroutine(bezier_move(transform, orientation, final_orientation));
                else // straight track
                {
                    if (gameObject.tag == "boxcar")
                        print("break");
                    StartCoroutine(straight_move(transform.position, target_position));
                }
            }
            else // in_tile = true. update position within a tile
            {
                transform.position = next_position;
            }
        }
    }

    public void initialize_position(Vector3Int position)
    {
        this.tile_position = position;
    }

    public void initialize_orientation(RouteManager.Orientation orientation)
    {
        // when leaving a city, vehicle orientation be initialized to final orientation (which the user chooses)
        final_orientation = orientation;
    }

    public void arrive_at_city()
    {
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.enabled = false;
        idling = true;
        in_city = true;
        city = CityManager.get_city(new Vector2Int(tile_position.x, tile_position.y)).GetComponent<City>();   
        if (gameObject.tag == "train") GameManager.city_manager.add_train_to_board(tile_position, gameObject);
    }

    public void prepare_for_departure()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        in_city = false;
        in_tile = true; // allow vehicle to move to the border of tile before resuming its route
        idling = false; // indicates a vehicle is about to leave
        Vector3 vehicle_departure_point = RouteManager.get_city_boundary_location(tile_position, orientation);
        print("departure point is " + vehicle_departure_point);
        StartCoroutine(straight_move(transform.position, vehicle_departure_point));

    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        print("inside Moving Object class");
    }

    public void set_motion(bool is_in_motion)
    {
        // set true when train gameobject is clicked or train sets boxcars in motion
        in_motion = is_in_motion;
    }

    public bool is_in_motion()
    {
        return in_motion;
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
        if (gameObject.tag == "boxcar")
            print("Destination of Boxcar is " + destination);
        // Move our position a step closer to the target.
        while (Vector2.Distance(next_position, destination) > tolerance)
        {
            if (!in_motion) yield return new WaitForEndOfFrame(); //delay updating the position if vehicle is idling
            float step = speed * Time.deltaTime; // calculate distance to move
            next_position = Vector2.MoveTowards(next_position, destination, step);
            yield return new WaitForEndOfFrame();
        }
        if (gameObject.tag == "boxcar")
            print("Reached Destination of Boxcar is " + destination);
        in_tile = false;
    }
}
