using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    Vector2 target_position;
    protected float speed = 1f;
    protected float tolerance = .004f;
    protected Vector2 next_position;

    //bezier vertices for a SE turn
    Vector2 p0 = new Vector2(.5f, .5f);
    Vector2 p1 = new Vector2(.2f, .5f);
    Vector2 p2 = new Vector2(0, .3f);
    Vector2 p3 = new Vector2(0, 0);

    public enum Orientation
    {
        North,
        East,
        West,
        South
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

    Vector2 bezier_equation(float t_param)
    {
        next_position = Mathf.Pow(1 - t_param, 3) * p0 +
            3 * Mathf.Pow(1 - t_param, 2) * t_param * p1 +
            3 * (1 - t_param) * Mathf.Pow(t_param, 2) * p2 +
            Mathf.Pow(t_param, 3) * p3;
        return next_position;
    }

    protected IEnumerator bezier_move(Vector2 position, Orientation orientation, Orientation final_orientation)
    {
        float t_param;
        t_param = 1;
        bool final_step = false;
        while (!final_step)
        {
            t_param -= Time.deltaTime * speed;
            if (t_param < 0)
            {
                t_param = 0;
                final_step = true;
            }
            next_position = bezier_equation(t_param);
            next_position = transform_curve(next_position, orientation, final_orientation) + position;
            yield return new WaitForEndOfFrame();
        }
        print("next bezier position " + next_position.ToString("F6"));
    }

    protected IEnumerator straight_move(Vector2 start_position, Vector2 destination)
    {
        next_position = start_position;
        // Move our position a step closer to the target.
        float step = speed * Time.deltaTime; // calculate distance to move
        while (Vector2.Distance(next_position, destination) > tolerance)
        {
            next_position = Vector2.MoveTowards(next_position, destination, step);
            yield return new WaitForEndOfFrame();
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }
}
