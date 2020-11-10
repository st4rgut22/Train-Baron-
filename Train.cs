using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Train : MovingObject
{
    public List<GameObject> boxcar_squad = new List<GameObject>(); // boxcars attached to this train
    string destination_type = ""; // get destination type. If city, then disable after reaching destination. 
    int id;

    private void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start(); // train instantiated bottom left
        GameManager.vehicle_manager.update_vehicle_board(city.city_board, gameObject, tile_position, new Vector3Int(-1, -1, -1));
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public void turn_on_train(bool is_train_on)
    {
        MovingObject.switch_sprite_renderer(gameObject, is_train_on); 
        foreach (GameObject boxcar_object in boxcar_squad)
        {
            MovingObject.switch_sprite_renderer(boxcar_object, is_train_on); 
        }
    }

    public override void arrive_at_city()
    {
        base.arrive_at_city();
        city.add_train_to_list(gameObject);
        print("add train with orientation " + orientation + " to station");
        station_track = city.add_train_to_station(gameObject, orientation);
        city.turn_table.GetComponent<Turntable>().add_train_to_queue(gameObject);
        if (station_track != null)
        {
            Vector3Int station_tile_position = station_track.start_location; // A NON-NULLABLE TYPE? ? ?
            print("station track start location is " + station_tile_position);
            GameManager.vehicle_manager.depart(gameObject, station_tile_position, city.city_board);
            assign_station_to_boxcar();
        } else
        {
            print("This city has no open stations from " + gameObject.name + " direction");
        }
    }

    public bool is_all_car_reach_turntable()
    {
        foreach (GameObject boxcar_object in boxcar_squad)
        {
            if (boxcar_object.GetComponent<Boxcar>().depart_city_orientation == RouteManager.Orientation.None) return false;
        }
        return true;
    }

    public void board_turntable(RouteManager.Orientation orientation, bool depart_turntable)
    {
        print("board turn table");
        if (depart_turntable)
        {
            halt_train(is_halt = false, is_pause = false); // unpause the train
        }
        else // leaving the turntable
        {
            this.orientation = city.destination_orientation;
            this.final_orientation = this.orientation; // dont overwrite orientation with shipyard orientation after departing city
            leave_city = true;
            foreach (GameObject boxcar_object in boxcar_squad)
            {
                Boxcar boxcar = boxcar_object.GetComponent<Boxcar>();
                boxcar.leave_city = true;
                boxcar.orientation = this.orientation;
                boxcar.final_orientation = this.orientation;
            }
        }
    }

    public void assign_station_to_boxcar()
    {
        foreach (GameObject boxcar_object in boxcar_squad)
        {
            boxcar_object.GetComponent<Boxcar>().station_track = station_track;
        }
    }

    public bool all_boxcar_arrived()
    {
        // check if train's boxcars have arrived before departing. If one boxcar is not idled, that means not all boxcars have arrived yet
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            Boxcar boxcar = boxcar_squad[i].GetComponent<Boxcar>();
            if (is_pause) return false;
        }
        return true;
    }

    public void halt_train(bool is_halt, bool state)
    {
        // stop the train. await user action or signal
        // halting the train stops it from going anywhere. Used to await new track.
        // if false, pause the train. meaning delay the action. Used to stop mid turn/mid motion
        if (is_halt) this.is_halt = state;
        else { is_pause = state; }
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            GameObject boxcar = boxcar_squad[i];
            if (is_halt) boxcar.GetComponent<Boxcar>().is_halt = state; 
            else { boxcar.GetComponent<Boxcar>().is_pause = state; }
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
        return city;
    }

    public void set_city(City city)
    { 
        this.city = city;
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
