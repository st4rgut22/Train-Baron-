﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Turntable : MonoBehaviour
{

    public City city;
    public RouteManager.Orientation orientation;

    private void Awake()
    {
        orientation = RouteManager.Orientation.North;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator turn_turntable(GameObject train_object, RouteManager.Orientation end_orientation, bool depart_for_turntable)
    {
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
                throw new Exception("not a valid orientation for turntable");
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
