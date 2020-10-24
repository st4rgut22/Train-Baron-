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
    string destination_type = ""; // get destination type. If city, then disable after reaching destination. 

    // Start is called before the first frame update
    void Start()
    {
        tile_position = new Vector3Int(0, 0, 0);
        base.Start(); // train instantiated bottom left
        GameManager.vehicle_manager.update_vehicle_board(gameObject, tile_position, new Vector3Int(-1, -1, -1));
        arrive_at_city(); // update home base with instantiated train
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public bool all_boxcar_arrived()
    {
        // check if train's boxcars have arrived before departing. If one boxcar is not idled, that means not all boxcars have arrived yet
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            Boxcar boxcar = boxcar_squad[i].GetComponent<Boxcar>();
            bool idle_status = boxcar.get_idle_status();
            if (!idle_status) return false;
        }
        return true;
    }

    public void change_motion()
    {
        StartCoroutine(GameManager.vehicle_manager.Make_All_Boxcars_Depart(boxcar_squad, this)); //TODO: hide train on city tile. Call coroutine from city manager
        in_motion = !in_motion; // if train is moving stop. If train is stopped move.
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            GameObject boxcar = boxcar_squad[i];
            boxcar.GetComponent<Boxcar>().set_motion(in_motion);
        }
    }

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

    public void remove_boxcar() // TODO: remove a specific boxcar
    {
        if (boxcar_squad.Count > 0)
        {
            GameObject boxcar = boxcar_squad[boxcar_squad.Count - 1];
            boxcar_squad.RemoveAt(boxcar_squad.Count - 1); // remove last boxcar
            Destroy(boxcar);
        }
    }

    public void attach_boxcar(GameObject boxcar)
    {
        boxcar_squad.Add(boxcar);
    }

    public int get_boxcar_id()
    {
        return boxcar_squad.Count;       
    }
}
