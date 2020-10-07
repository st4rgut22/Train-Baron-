using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovingObject : EventDetector
{
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

    public Orientation orientation;
    public Orientation final_orientation;
    public Vector3Int tile_position;
    protected const float z_pos = 0;
    protected bool reached_destination = false;

    protected GameManager game_manager;

    public enum Orientation
    {
        North,
        East,
        West,
        South
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        print("inside Moving Object class");
    }

    public void spawn_moving_object(Vector3Int tile_position, Orientation orientation)
    {
        this.tile_position = tile_position;
        Vector3 moving_object_position = (Vector3)tile_position;
        switch (orientation)
        {
            case Orientation.East:
                moving_object_position.x += 1;
                moving_object_position.y += .5f;
                break;
            case Orientation.West:
                moving_object_position.x -= 1;
                moving_object_position.y += .5f;
                break;
            case Orientation.North:
                moving_object_position.x += .5f;
                moving_object_position.y += 1;
                break;
            case Orientation.South:
                moving_object_position.x += .5f;
                moving_object_position.y -= 1;
                break;
            default:
                print("train orientation is not set. cannot set boxcar position");
                break;
        }
        transform.position = moving_object_position;
    }

    Vector2 transform_curve(Vector2 bezier_position, Orientation orientation, Orientation final_orientation)
    {
        // transform the coord of bezier curve so it matches shape of appropriate track
        if (orientation == Orientation.North && final_orientation == Orientation.East)
        {
            return bezier_position;
        } else if (orientation == Orientation.South && final_orientation == Orientation.East)
        {
            bezier_position[1] = -bezier_position[1];
        } else if (orientation == Orientation.North && final_orientation == Orientation.West)
        {
            bezier_position[0] = -bezier_position[0];
        } else if (orientation == Orientation.South && final_orientation == Orientation.West)
        {
            bezier_position[0] = -bezier_position[0];
            bezier_position[1] = -bezier_position[1];
        } else if (orientation == Orientation.East && final_orientation == Orientation.South)
        { // rotate curve 90 degrees counter clockwise
            float temp_x = bezier_position[0];
            bezier_position[0] = bezier_position[1];
            bezier_position[1] = -temp_x;
        } else if (orientation == Orientation.East && final_orientation == Orientation.North)
        {
            float temp_x = bezier_position[0];
            bezier_position[0] = bezier_position[1];
            bezier_position[1] = temp_x;
        } else if (orientation == Orientation.West && final_orientation == Orientation.South)
        {
            float temp_x = bezier_position[0];
            bezier_position[0] = -bezier_position[1];
            bezier_position[1] = -temp_x;
        }
        else if (orientation == Orientation.West && final_orientation == Orientation.North)
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

    float get_rotation(Orientation orientation, Orientation final_orientation)
    {
        // get angle of moving object along track based on start and end orientation
        if (orientation == Orientation.North && final_orientation==Orientation.East ||
            orientation == Orientation.South && final_orientation==Orientation.West ||
            orientation == Orientation.West && final_orientation==Orientation.North ||
            orientation == Orientation.East && final_orientation==Orientation.South)
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

    protected IEnumerator bezier_move(Transform location, Orientation orientation, Orientation final_orientation)
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

    // Start is called before the first frame update
    public virtual void Start()
    {
        target_position = transform.position;
        orientation = Orientation.North;
        game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }



    // Update is called once per frame
    public virtual void Update()
    {
        if (in_motion)
        {
            if (!in_tile) // update destination to next tile
            {
                orientation = final_orientation;
                Vector3Int prev_tile_position = tile_position;
                if (orientation == Orientation.East)
                {
                    tile_position = new Vector3Int(Mathf.RoundToInt(transform.position[0]), (int)(transform.position[1]), 0);
                }
                else if (orientation == Orientation.West)
                {
                    tile_position = new Vector3Int(Mathf.RoundToInt(transform.position[0]) - 1, (int)(transform.position[1]), 0);
                }
                else if (orientation == Orientation.North)
                {
                    tile_position = new Vector3Int((int)(transform.position[0]), Mathf.RoundToInt(transform.position[1]), 0);
                }
                else if (orientation == Orientation.South)
                {
                    tile_position = new Vector3Int((int)(transform.position[0]), Mathf.RoundToInt(transform.position[1]) - 1, 0);
                }
                print("update " + gameObject.name + " position from " + prev_tile_position + " to " + tile_position);
                game_manager.update_board_state(gameObject, tile_position, prev_tile_position);
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
        }
    }
}
