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
    protected bool reached_city = true; // train instantiated at home base. Update home base accordingly.

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
        if (!in_motion)
        {
            if (reached_city) // boolean flag. When a train reaches a city, add train to city once
            {
                destination_type = RouteManager.get_destination_type(tile_position); // get type of destination
                if (destination_type.Equals("city"))
                {
                    update_city(gameObject, tile_position); // add train to city if arrived in city. update base
                    prev_orientation = orientation;
                    reached_city = false;
                }
            }
        }
    }

    public void change_motion()
    {
        StartCoroutine(vehicle_manager.Make_All_Boxcars_Depart(boxcar_squad, this)); //TODO: hide train on city tile. Call coroutine from city manager
        in_motion = !in_motion; // if train is moving stop. If train is stopped move.
        reached_city = false; // whenever in motion, set reached flag to false until vehicle arrives in city
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            GameObject boxcar = boxcar_squad[i];
            boxcar.GetComponent<Boxcar>().set_motion(in_motion);
        }
    }

    public void update_city(GameObject gameObject, Vector3Int tile_position)
    {
        // if vehicle has arrived at a city, update the city with arrived vehicles and disable the vehicles
        city_manager.add_train_to_board(tile_position, gameObject);
        City city = CityManager.get_city(new Vector2Int(tile_position.x, tile_position.y)).GetComponent<City>();
        gameObject.GetComponent<Train>().set_city(city);
        //gameObject.SetActive(false); // disable gameobject and components upon reaching the destination
        return;
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
        boxcar_squad.RemoveAt(boxcar_squad.Count - 1); // remove last boxcar
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
