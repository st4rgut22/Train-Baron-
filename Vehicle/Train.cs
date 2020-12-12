using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Train : MovingObject
{
    public List<GameObject> boxcar_squad = new List<GameObject>(); // boxcars attached to this train
    GameObject clone_train; // used for dragging onto destination track
    public string exit_track_tile_type;
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

    public GameObject get_last_vehicle_added()
    {
        if (boxcar_squad.Count == 0) return gameObject;
        return boxcar_squad[boxcar_squad.Count - 1];
    }

    public void turn_on_train(bool is_train_on)
    {

        MovingObject.switch_sprite_renderer(gameObject, is_train_on);
        foreach (GameObject boxcar_object in boxcar_squad)
        {
            Boxcar boxcar = boxcar_object.GetComponent<Boxcar>();
            if (!boxcar.departing)
            {
                MovingObject.switch_sprite_renderer(boxcar_object, is_train_on);
            }
            else
            {
                MovingObject.switch_sprite_renderer(boxcar_object, !is_train_on); // if train is shown, hide boxcars. if train is hidden. show boxcars
            }
        }
    }

    public void click_train(PointerEventData eventData)
    {
        if (in_city)
        {
            // hlighlight unloading regions AND exit tracks
            // unloading regions


            // highlight exit track
            mark_exit_track();
        }
    }

    public void mark_exit_track()
    {
        bool north_exit_visible = CityManager.is_exit_route_shown(RouteManager.Orientation.North);
        bool east_exit_visible = CityManager.is_exit_route_shown(RouteManager.Orientation.East);
        bool west_exit_visible = CityManager.is_exit_route_shown(RouteManager.Orientation.West);
        bool south_exit_visible = CityManager.is_exit_route_shown(RouteManager.Orientation.South);
        List<List<int[]>> train_action_coord = new List<List<int[]>>();
        List<string> train_hint_list = new List<string>();
        if (north_exit_visible)
        {
            train_hint_list.Add("north exit");
            train_action_coord.Add(TrackManager.exit_track_map[RouteManager.Orientation.North]);
        }
        if (east_exit_visible)
        {
            train_hint_list.Add("east exit");
            train_action_coord.Add(TrackManager.exit_track_map[RouteManager.Orientation.East]);
        }
        if (west_exit_visible)
        {
            train_action_coord.Add(TrackManager.exit_track_map[RouteManager.Orientation.West]);
            train_hint_list.Add("west exit");
        }
        if (south_exit_visible)
        {
            train_action_coord.Add(TrackManager.exit_track_map[RouteManager.Orientation.South]);
            train_hint_list.Add("south exit");
        }
        game_manager.mark_tile_as_eligible(train_action_coord, train_hint_list, gameObject, true);
    }

    public void exit_city(string exit_track_tile_type)
    {
        exit_track_orientation = TrainRouteManager.get_destination_track_orientation(exit_track_tile_type);
        city.turn_table.GetComponent<Turntable>().add_train_to_queue(gameObject);
        set_boxcar_exit_track_orientation(exit_track_orientation);
        if (city == CityManager.Activated_City_Component) GameManager.train_menu_manager.update_train_menu(city);
    }


    public void set_boxcar_exit_track_orientation(RouteManager.Orientation orientation)
    {
        foreach (GameObject boxcar_object in boxcar_squad)
        {
            boxcar_object.GetComponent<Boxcar>().exit_track_orientation = orientation;
        }
    }

    public override void arrive_at_city()
    {
        base.arrive_at_city();
        //city.add_train_to_list(gameObject);
        station_track = city.add_train_to_station(gameObject, orientation);
        //city.turn_table.GetComponent<Turntable>().add_train_to_queue(gameObject);
        if (station_track != null)
        {
            Vector3Int station_tile_position = station_track.start_location; // A NON-NULLABLE TYPE? ? ?
            GameManager.vehicle_manager.depart(gameObject, station_tile_position, city.city_board);
            assign_station_to_boxcar();
        }
        //else
        //{
        //    print("This city has no open stations from " + gameObject.name + " direction");
        //}
        set_boxcar_to_depart();
        city.add_train_to_list(gameObject);
    }

    public void set_boxcar_to_depart()
    {
        foreach (GameObject boxcar_object in boxcar_squad)
        {
            boxcar_object.GetComponent<Boxcar>().departing = true;
        }
    }

    public bool is_all_car_reach_turntable()
    {
        foreach (GameObject boxcar_object in boxcar_squad)
        {
            if (boxcar_object.GetComponent<Boxcar>().exit_track_orientation == RouteManager.Orientation.None) return false;
        }
        return true;
    }

    public int get_distance_from_train(int boxcar_id)
    {
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            Boxcar boxcar = boxcar_squad[i].GetComponent<Boxcar>();
            if (boxcar.boxcar_id == boxcar_id)
            {
                //print("distance from train is " + (i + 1));
                return i + 1;
            }
        }
        throw new Exception("boxcar with id " + boxcar_id + " should belong to train");
    }

    public void board_turntable(RouteManager.Orientation orientation, bool depart_turntable)
    {
        if (depart_turntable)
        {
            halt_train(true, false); //unhalt the boxcars
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
                bool halt = boxcar.is_halt;
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
        if (is_halt) set_halt(state); 
        else { is_pause = state; }
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            GameObject boxcar = boxcar_squad[i];
            if (is_halt) boxcar.GetComponent<Boxcar>().set_halt(state); 
            else { boxcar.GetComponent<Boxcar>().is_pause = state; }
        }
    }

    public bool is_all_boxcar_boarded()
    {
        // train cannot leave until all passengers are boarded. 
        foreach (GameObject boxcar_go in boxcar_squad)
        {
            Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
            if (boxcar.is_occupied)
            {
                if (boxcar.passenger_go.GetComponent<Person>().is_board_boxcar) // passenger is in process of boarding boxcar
                    return false;
            }
        }
        return true;
    }

    public IEnumerator wait_for_track_placement(Vector2Int next_tile_pos)
    {
        while (true)
        {
            Tilemap tilemap = GameManager.track_manager.top_tilemap;
            Tile tile = (Tile) tilemap.GetTile((Vector3Int)next_tile_pos);
            if (tile != null && TrackManager.is_track_a_path(orientation, tile.name)) // make sure the train can cross this track
                break;
            else { yield return new WaitForEndOfFrame(); }
        }
        end_of_track = false;
        halt_train(false, false); // unpause the train
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

    public void remove_boxcar(int count = -1) 
    {
        if (count == -1) count = boxcar_squad.Count; // remove last boxcar by default
        if (count > 0)
        {
            GameObject boxcar = boxcar_squad[count - 1];
            boxcar_squad.RemoveAt(count - 1); // remove last boxcar
            Destroy(boxcar);
        }
    }

    public void attach_boxcar(GameObject boxcar)
    {
        boxcar_squad.Add(boxcar);
    }

    public int get_boxcar_id()
    {
        return boxcar_squad.Count+1;       
    }
}
