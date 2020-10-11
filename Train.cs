using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Train : MovingObject
{
    List<GameObject> boxcar_squad = new List<GameObject>(); // boxcars attached to this train
    City start_city;
    int id;

    public void set_id(int id)
    {
        this.id = id;
    }

    public int get_id()
    {
        return id;
    }

    public City get_city()
    {
        return start_city;
    }

    public void set_city(City city)
    { 
        start_city = city;
    }

    public void attach_boxcar(GameObject boxcar)
    {
        boxcar_squad.Add(boxcar);
    }

    public int get_boxcar_id()
    {
        return boxcar_squad.Count;       
    }

    public void change_motion()
    {
        StartCoroutine(vehicle_manager.Make_All_Boxcars_Depart(boxcar_squad, this)); //TODO: hide train on city tile. Call coroutine from city manager
        in_motion = !in_motion;
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            GameObject boxcar = boxcar_squad[i];
            boxcar.GetComponent<Boxcar>().set_motion(in_motion); 
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        tile_position = new Vector3Int(0, 0, 0);
        base.Start(); // train instantiated bottom left
        vehicle_manager.update_vehicle_board(gameObject, tile_position, new Vector3Int(-1, -1, -1));
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
