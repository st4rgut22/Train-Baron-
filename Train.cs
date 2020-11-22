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
            // todo: wait until boxcars have entered the city
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

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (in_city)
        {
            print("begin dragging train in city");
            float rotation = transform.eulerAngles.z;
            clone_train = Instantiate(GameManager.vehicle_manager.Train_Placeholder, transform.position, Quaternion.Euler(0,0, rotation));
            base.OnBeginDrag(eventData);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {   if (in_city)
        {
            print("dragging train in city");
            try
            {
                Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
                clone_train.transform.position = world_position;
                Vector2Int selected_tile = GameManager.get_selected_tile(Input.mousePosition);
                exit_track_tile_type = TrackManager.is_track_tile_exit(selected_tile);
                if (exit_track_tile_type != null)
                {
                    float rotation = TrackManager.get_exit_track_rotation(exit_track_tile_type);
                    clone_train.transform.eulerAngles = new Vector3(0, 0, rotation);
                }
            }
            catch (NullReferenceException e)
            {
                print("tried to drag something that is not draggable");
                print(e.Message);
            }
            catch (MissingReferenceException e)
            {
                print("Trying to access a destroyed object");
                print(e.Message);
            }
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (in_city)
        {
            print("finish dragging train");
            Destroy(clone_train);
            // queue up train
            if (exit_track_tile_type != null)
            {
                bool route_hidden = true;
                if (exit_track_tile_type == "Shipyard Track Exit North")
                    route_hidden = GameManager.city_manager.hide_exit_route(RouteManager.Orientation.North, city, RouteManager.exit_north_tilemap);
                else if (exit_track_tile_type == "Shipyard Track Exit East")
                    route_hidden = GameManager.city_manager.hide_exit_route(RouteManager.Orientation.East, city, RouteManager.exit_east_tilemap);
                else if (exit_track_tile_type == "Shipyard Track Exit West")
                    route_hidden = GameManager.city_manager.hide_exit_route(RouteManager.Orientation.West, city, RouteManager.exit_west_tilemap);
                else if (exit_track_tile_type == "Shipyard Track Exit South")
                    route_hidden = GameManager.city_manager.hide_exit_route(RouteManager.Orientation.South, city, RouteManager.exit_south_tilemap);
                if (!route_hidden)
                {
                    exit_track_orientation = RouteManager.get_destination_track_orientation(exit_track_tile_type);
                    print("train exit track orientation is " + exit_track_orientation);
                    city.turn_table.GetComponent<Turntable>().add_train_to_queue(gameObject);
                    set_boxcar_exit_track_orientation(exit_track_orientation);
                    if (city == CityManager.Activated_City_Component) GameManager.train_menu_manager.update_train_menu(city);
                }
                else
                {
                    print(" route " + exit_track_tile_type + " not available");
                }
            }
        }
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
        print("add train with orientation " + orientation + " to station");
        station_track = city.add_train_to_station(gameObject, orientation);
        //city.turn_table.GetComponent<Turntable>().add_train_to_queue(gameObject);
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
                print("distance from train is " + (i + 1));
                return i + 1;
            }
        }
        throw new Exception("boxcar with id " + boxcar_id + " should belong to train");
    }

    public void board_turntable(RouteManager.Orientation orientation, bool depart_turntable)
    {
        print("board turn table");
        //TODO: create dedicated function for adding boxcars
        if (depart_turntable)
        {
            //GameManager.vehicle_manager.add_all_boxcar_to_train(gameObject.GetComponent<Train>()); //TODO: allow user to select vehicle to add boxcars to 
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

    public IEnumerator wait_for_track_placement(Vector2Int next_tile_pos)
    {
        while (true)
        {
            Tilemap tilemap = GameManager.track_manager.get_toggled_tilemap(next_tile_pos);
            Tile tile = (Tile) tilemap.GetTile((Vector3Int)next_tile_pos);
            if (tile != null) break;
            else { yield return new WaitForEndOfFrame(); }
        }
        halt_train(false, false); // unhalt the train
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

    public void remove_boxcar(int count = -1) // TODO: remove a specific boxcar
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
