using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class Simple_Moving_Object : EventDetector
{
    public float speed = 2f; // Temporary. changed from 2f
    public float stopping_speed = 0;
    public float normal_speed = 2f;
    public float speed_multiplier = 1f;
    protected Vector2 next_position;
    public bool in_tile = false; // if an object is in a tile, it is already has a destination

    public RouteManager.Orientation orientation; // orientation before moving to a new tile
    public RouteManager.Orientation final_orientation; // orientation after moving to a new tile 
    public RouteManager.Orientation prev_orientation; // orientation during the last idle state
    public RouteManager.Orientation depart_city_orientation = RouteManager.Orientation.None;
    public Vector3Int tile_position;
    public Vector3Int prev_tile_position;
    public Vector2Int next_tilemap_position;
    protected const float z_pos = 0;

    public Vector3Int final_dest_tile_pos;
    public Vector3 final_dest_pos;
    public bool final_destination_reached = false;

    public bool arriving_in_city = false; // next tile position is a city. upon movement completion set in_city=true
    public bool in_city;
    public City city;
    public City prev_city; // used to check whether a city destination is not in fact the city youve just left
    protected CityManager city_manager;
    protected GameManager game_manager;

    public float angle;
    public float orient_angle; 
    public Station_Track station_track;
    public Vector2 offset = new Vector2(0, 0);
    public Dictionary<string, Vector2> offset_map;

    public string current_animation;
    Animator animator;

    protected void Awake()
    {
        animator = GetComponent<Animator>();
        current_animation = "player_idle_vert_down";
    }

    // Start is called before the first frame update
    public virtual void Start()
    {

    }

    public virtual void Update()
    {
        if (!in_tile) // Completed tile route. update destination to next tile. Prevents repeated calls to StartCoroutine()
        {
            orientation = final_orientation; // updating the orientation at every new tile
            tile_position = new Vector3Int(next_tilemap_position.x, next_tilemap_position.y, 0);
            PositionPair position_pair;
            final_destination_reached = is_destination_reached(RouteManager.cell_width / 2);
            if (final_destination_reached) // stop movement update once reached final destination
            {
                in_tile = true;
                return;
            }
            Vector3Int tile_coord = new Vector3Int(tile_position[0], tile_position[1], 0);
            string track_tile_name = station_track.tilemap.GetTile(tile_coord).name;
            offset = offset_map[track_tile_name];
            position_pair = RouteManager.get_destination(this, station_track.tilemap, offset); // set the final orientation and destination
            next_tilemap_position = position_pair.tile_dest_pos;
            Vector2 train_dest_xy = position_pair.abs_dest_pos;
            // stop the train here if track ends

            Vector3 train_destination = new Vector3(train_dest_xy[0], train_dest_xy[1], z_pos);

            if (orientation != final_orientation) // curved track
                StartCoroutine(bezier_move(transform, orientation, final_orientation)); // offset bool set to true
            else // straight track
            {
                StartCoroutine(straight_move(transform.position, train_destination));
            }
        }
    }

    public IEnumerator set_animation_clip(string animation_clip)
    {
        // change the sprite's animation clip
        animator.SetBool(animation_clip, true);
        animator.SetBool(current_animation, false);
        current_animation = animation_clip;
        yield return new WaitForSeconds(.3f);// wait a frame to set animation
    }

    public IEnumerator move_checkpoints(List<Checkpoint> checkpoint_list)
    {
        Person person = gameObject.GetComponent<Person>();
        ////print("begin moving " + gameObject.name + " to checkpoints");
        for (int i = 0; i < checkpoint_list.Count; i++)
        {
            Checkpoint cp = checkpoint_list[i];
            if (cp.animation_clip != "none") // animation has changed
            {
                ////print("set animation clip " + cp.animation_clip);
                // flip here
                if (cp.animation_clip == "player_idle_hor" || cp.animation_clip == "player_walk_hor")
                {
                    if (cp.end_orientation == RouteManager.Orientation.West) gameObject.GetComponent<SpriteRenderer>().flipX = true;
                    else if (cp.end_orientation == RouteManager.Orientation.East) gameObject.GetComponent<SpriteRenderer>().flipX = false;
                    else { throw new Exception("cant have a up or down orientation for hor animation");  }
                }
                yield return StartCoroutine(set_animation_clip(cp.animation_clip));
            }
            Vector2 checkpoint_position = cp.dest_pos;
            tile_position = (Vector3Int)cp.tile_position;
            angle = cp.final_angle;
            orientation = cp.end_orientation;
            final_orientation = orientation;
            yield return StartCoroutine(straight_move(transform.position, checkpoint_position));
            person.tile_position = tile_position; // update tile position
        }
        person.next_tilemap_position = (Vector2Int)person.tile_position;
    }

    public bool is_destination_reached(float min_dist)
    {
        //print("tile pos is " + tile_position + " final dest pos is " + final_dest_tile_pos + " and final dest vector3 is " + final_dest_pos);
        float dist = Vector2.Distance(final_dest_pos, transform.position);
        if (dist < min_dist) return true;
        else { return false; }
    }

    public virtual void set_initial_rotation(RouteManager.Orientation orientation)
    {
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                angle = 0;
                break;
            case RouteManager.Orientation.East:
                angle = -90;
                break;
            case RouteManager.Orientation.West:
                angle = 90;
                break;
            case RouteManager.Orientation.South:
                angle = 180;
                break;
        }
    }

    public void set_angle_using_orientation(RouteManager.Orientation orientation)
    {
        if (orientation == RouteManager.Orientation.North)
            orient_angle = 0;
        else if (orientation == RouteManager.Orientation.East)
            orient_angle = -90f;
        else if (orientation == RouteManager.Orientation.West)
            orient_angle = 90f;
        else if (orientation == RouteManager.Orientation.South)
            orient_angle = 180f;
        else { throw new Exception("unable to set angle because orientation not recognized"); }            
    }

    public virtual IEnumerator bezier_move(Transform location, RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
    {
        Vector2 position = location.position;
        float t_param;
        t_param = 1;
        bool final_step = false;
        in_tile = true;
        // change animation clip
        string animation_name = PersonRouteManager.get_animation_from_orientation(final_orientation, "walk");
        StartCoroutine(set_animation_clip(animation_name));
        while (!final_step)
        {
            if (PauseManager.game_is_paused)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }                
            t_param -= Time.deltaTime * speed;

            if (t_param < 0) // set t_param to 0 to get bezier coordinates closer to the destination (and be within tolerance)
            {
                t_param = 0;
                final_step = true;
            }
            next_position = bezier_equation(t_param, TrackManager.offset_right_angle_inner_curve);
            next_position = transform_curve(next_position, orientation, final_orientation) + position; //offset with current position
            transform.position = next_position;
            yield return new WaitForEndOfFrame();
        }
        set_angle_using_orientation(final_orientation);
        in_tile = false;
    }

    protected Vector2 bezier_equation(float t_param, List<Vector2> bezier_curve)
    {
        next_position = Mathf.Pow(1 - t_param, 3) * bezier_curve[0] +
            3 * Mathf.Pow(1 - t_param, 2) * t_param * bezier_curve[1] +
            3 * (1 - t_param) * Mathf.Pow(t_param, 2) * bezier_curve[2] +
            Mathf.Pow(t_param, 3) * bezier_curve[3];
        return next_position;
    }

    protected Vector2 transform_curve(Vector2 bezier_position, RouteManager.Orientation orientation, RouteManager.Orientation final_orientation)
    {
        // transform the coord of bezier curve so it matches shape of appropriate track
        if (orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.East)
        {
            return bezier_position;
        }
        else if ((orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.East) ||
                  final_orientation == RouteManager.Orientation.ne_SteepCurve || final_orientation == RouteManager.Orientation.ne_LessSteepCurve)
        {
            bezier_position[1] = -bezier_position[1];
        }
        else if ((orientation == RouteManager.Orientation.North && final_orientation == RouteManager.Orientation.West) ||
          final_orientation == RouteManager.Orientation.sw_SteepCurve || final_orientation == RouteManager.Orientation.sw_LessSteepCurve)
        {
            bezier_position[0] = -bezier_position[0];
        }
        else if ((orientation == RouteManager.Orientation.South && final_orientation == RouteManager.Orientation.West) ||
          final_orientation == RouteManager.Orientation.nw_SteepCurve || final_orientation == RouteManager.Orientation.nw_LessSteepCurve)
        {
            bezier_position[0] = -bezier_position[0];
            bezier_position[1] = -bezier_position[1];
        }
        else if (orientation == RouteManager.Orientation.East && final_orientation == RouteManager.Orientation.South)
        { // rotate curve 90 degrees counter clockwise
            float temp_x = bezier_position[0];
            bezier_position[0] = bezier_position[1];
            bezier_position[1] = -temp_x;
        }
        else if (orientation == RouteManager.Orientation.East && final_orientation == RouteManager.Orientation.North)
        {
            float temp_x = bezier_position[0];
            bezier_position[0] = bezier_position[1];
            bezier_position[1] = temp_x;
        }
        else if (orientation == RouteManager.Orientation.West && final_orientation == RouteManager.Orientation.South)
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
        }
        return bezier_position;
    }

    public void prepare_to_arrive_at_city(City city)
    {
        if (city == null) throw new Exception("city shouldn't be null");
        this.city = city;
        arriving_in_city = true;
    }

    public IEnumerator rotate(float end_angle, float start_angle)
    {
        end_angle -= 90f;
        start_angle -= 90f; // remove offset incorporated into rotation calculations to align person in right direction
        float t_param = 1;
        while (t_param > 0)
        {
            float interp = 1.0f - t_param;
            float angle = Mathf.LerpAngle(start_angle, end_angle, interp); // interpolate from [0,1]
            transform.eulerAngles = new Vector3(0, 0, angle);
            t_param -= Time.deltaTime * GameManager.speed;
            yield return new WaitForEndOfFrame();
        }
    }

    public virtual IEnumerator straight_move(Vector2 start_position, Vector2 destination)
    {
        float og_distance = Vector2.Distance(start_position, destination); // distance to destination
        float distance = og_distance;
        in_tile = true;
        next_position = start_position;
        while (distance > GameManager.tolerance)
        {
            if (PauseManager.game_is_paused)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }                
            float step;
            step = speed * Time.deltaTime; // calculate distance to move
            next_position = Vector2.MoveTowards(next_position, destination, step);
            transform.position = next_position;
            distance = Vector2.Distance(next_position, destination);
            yield return new WaitForEndOfFrame();
        }
        in_tile = false;
    }
}