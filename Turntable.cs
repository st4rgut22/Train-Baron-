using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Turntable : MonoBehaviour
{

    public City city;
    public RouteManager.Orientation orientation;
    Queue<GameObject> train_queue;

    private void Awake()
    {
        orientation = RouteManager.Orientation.North;
    }

    // Start is called before the first frame update
    void Start()
    {
        train_queue = new Queue<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool is_train_turn(GameObject train)
    {
        if (train_queue.Count == 0) return false; // prevent empty queue exception
        if (train == train_queue.Peek()) return true;
        else
            return false;
    }

    public void remove_train_from_queue(GameObject train)
    {
        if (train != train_queue.Peek()) throw new Exception("train should be first in queue, and first to remove");
        train_queue.Dequeue();
    }

    public void add_train_to_queue(GameObject train)
    {
        train_queue.Enqueue(train);
    }

    public IEnumerator turn_turntable(GameObject train_object, RouteManager.Orientation end_orientation, bool depart_for_turntable)
    {
        print("end orientation of turntable is " + end_orientation);
        float turn_angle = get_turntable_rotation(end_orientation);
        float t_param = 0;
        float start_angle = transform.eulerAngles.z;
        float end_angle = start_angle + turn_angle;
        while (t_param < 1)
        {
            t_param += Time.deltaTime;
            float angle = Mathf.LerpAngle(start_angle, end_angle, t_param);
            transform.eulerAngles = new Vector3(0, 0, angle);
            yield return new WaitForEndOfFrame();
        }
        Train train = train_object.GetComponent<Train>();
        train.board_turntable(end_orientation, depart_for_turntable);
    }

    public float get_turntable_rotation(RouteManager.Orientation end_orientation)
    {
        float rotate = 0;
        if (((orientation == RouteManager.Orientation.ne_SteepCurve || orientation == RouteManager.Orientation.ne_LessSteepCurve) && end_orientation == RouteManager.Orientation.West) ||
            ((orientation == RouteManager.Orientation.nw_SteepCurve || orientation == RouteManager.Orientation.nw_LessSteepCurve) && end_orientation == RouteManager.Orientation.North) ||
            ((orientation == RouteManager.Orientation.sw_SteepCurve || orientation == RouteManager.Orientation.sw_LessSteepCurve) && end_orientation == RouteManager.Orientation.East) ||
            ((orientation == RouteManager.Orientation.se_SteepCurve || orientation == RouteManager.Orientation.se_LessSteepCurve) && end_orientation == RouteManager.Orientation.South))
        {
            rotate = -135; // cc is positive
        }
        else if (((orientation == RouteManager.Orientation.ne_SteepCurve || orientation == RouteManager.Orientation.ne_LessSteepCurve) && end_orientation == RouteManager.Orientation.North) ||
            ((orientation == RouteManager.Orientation.nw_SteepCurve || orientation == RouteManager.Orientation.nw_LessSteepCurve) && end_orientation == RouteManager.Orientation.East) ||
            ((orientation == RouteManager.Orientation.sw_SteepCurve || orientation == RouteManager.Orientation.sw_LessSteepCurve) && end_orientation == RouteManager.Orientation.South) ||
            ((orientation == RouteManager.Orientation.se_SteepCurve || orientation == RouteManager.Orientation.se_LessSteepCurve) && end_orientation == RouteManager.Orientation.West))
        {
            rotate = 135f;
        }
        else if (((orientation == RouteManager.Orientation.ne_SteepCurve || orientation == RouteManager.Orientation.ne_LessSteepCurve) && end_orientation == RouteManager.Orientation.South) ||
            ((orientation == RouteManager.Orientation.se_SteepCurve || orientation == RouteManager.Orientation.se_LessSteepCurve) && end_orientation == RouteManager.Orientation.East) ||
            ((orientation == RouteManager.Orientation.sw_SteepCurve || orientation == RouteManager.Orientation.sw_LessSteepCurve) && end_orientation == RouteManager.Orientation.North) ||
            ((orientation == RouteManager.Orientation.nw_SteepCurve || orientation == RouteManager.Orientation.nw_LessSteepCurve) && end_orientation == RouteManager.Orientation.West))
        {
            rotate = -45f;
        }
        else if (((orientation == RouteManager.Orientation.ne_SteepCurve || orientation == RouteManager.Orientation.ne_LessSteepCurve) && end_orientation == RouteManager.Orientation.East) ||
            ((orientation == RouteManager.Orientation.nw_SteepCurve || orientation == RouteManager.Orientation.nw_LessSteepCurve) && end_orientation == RouteManager.Orientation.South) ||
            ((orientation == RouteManager.Orientation.sw_SteepCurve || orientation == RouteManager.Orientation.sw_LessSteepCurve) && end_orientation == RouteManager.Orientation.West) ||
            ((orientation == RouteManager.Orientation.se_SteepCurve || orientation == RouteManager.Orientation.se_LessSteepCurve) && end_orientation == RouteManager.Orientation.North))
        {
            rotate = 45;
        }
        else if (orientation == RouteManager.Orientation.North || orientation == RouteManager.Orientation.South)
        {
            if (end_orientation == RouteManager.Orientation.nw_SteepCurve || end_orientation == RouteManager.Orientation.nw_LessSteepCurve ||
                end_orientation == RouteManager.Orientation.se_SteepCurve || end_orientation == RouteManager.Orientation.se_LessSteepCurve)
            {
                rotate = -45;
            }
            else if (end_orientation == RouteManager.Orientation.sw_SteepCurve || end_orientation == RouteManager.Orientation.sw_LessSteepCurve ||
                     end_orientation == RouteManager.Orientation.ne_SteepCurve || end_orientation == RouteManager.Orientation.ne_LessSteepCurve)
            {
                rotate = 45;
            }
            else
            {
                throw new Exception("not a valid orientation for turntable");
            }
        }
        else if (orientation == RouteManager.Orientation.West || orientation == RouteManager.Orientation.East)
        {
            if (end_orientation == RouteManager.Orientation.nw_SteepCurve || end_orientation == RouteManager.Orientation.nw_LessSteepCurve
                || end_orientation == RouteManager.Orientation.se_SteepCurve || end_orientation == RouteManager.Orientation.se_LessSteepCurve)
            {
                rotate = 45;
            }
            else if (end_orientation == RouteManager.Orientation.ne_SteepCurve || end_orientation == RouteManager.Orientation.ne_LessSteepCurve ||
                    end_orientation == RouteManager.Orientation.sw_SteepCurve || end_orientation == RouteManager.Orientation.sw_LessSteepCurve)
            {
                rotate = -45;
            }
            else
            {
                throw new Exception("not a valid orientation for turntable " + orientation);
            }
        }
        else
        {
            throw new Exception("not a valid orientation for turntable");
        }
        print("orientation is " + orientation + " end orientation is " + end_orientation + "rotate " + rotate);

        orientation = end_orientation;
        return rotate;
    }
}
