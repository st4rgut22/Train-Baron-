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
    int boxcar_counter;
    public GameObject explosion;
    public List<GameObject> explosion_list;
    public bool is_train_departed_for_turntable;

    private void Awake()
    {
        boxcar_counter = 0;
        base.Awake();
        is_train_departed_for_turntable = false;
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
        if (!is_halt && !in_tile && !end_of_track)
        {
            set_destination();
            if (!in_city && gameObject.tag == "train" && is_end_of_track() && !is_pause)
            {
                StartCoroutine(gameObject.GetComponent<Train>().wait_for_track_placement(next_tilemap_position));
                gameObject.GetComponent<Train>().halt_train(false, true);
                end_of_track = true;
                return; // cancel any further movemnt updates
            }
        }
        base.Update();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // should still trigger when not visible in inspector
        // not updated in unity's inspector
        print("collided with " + collision.gameObject.name);        
        if (collision.gameObject.tag == "boxcar" || collision.gameObject.tag == "train")
        {
            bool collide_go_in_city = collision.gameObject.GetComponent<MovingObject>().in_city;
            if (in_city == collide_go_in_city)
            {
                if (in_city && collision.gameObject.GetComponent<MovingObject>().city != city) // collide with a train in another city
                    return;
                if (collision.gameObject.tag == "boxcar")
                {
                    if (boxcar_squad[0] == collision.gameObject) //ignore coll9isions with own lead boxcar
                        return;
                }
                if (!in_city && boxcar_squad.Contains(collision.gameObject))
                {
                    Tile city_tile = (Tile)RouteManager.city_tilemap.GetTile((Vector3Int)collision.gameObject.GetComponent<Boxcar>().tile_position);
                    if (city_tile != null) return; // colliding with boxcar that just left a city. technically not out of city yet. 
                }
                halt_train(false, true);
                GameObject explosion_go = Instantiate(explosion);
                print("explode");
                explode explosion_anim = explosion_go.transform.GetChild(0).gameObject.GetComponent<explode>();
                explosion_anim.exploded_train = this;
                explosion_list = new List<GameObject> { explosion_go };
                explosion_go.transform.position = transform.position;
                explosion_go.transform.localScale = new Vector3(.2f, .2f);
                foreach (GameObject boxcar_go in boxcar_squad)
                {
                    GameObject boxcar_explosion = Instantiate(explosion);
                    explosion_list.Add(boxcar_explosion);
                    boxcar_explosion.transform.position = boxcar_go.transform.position;
                }
                if (city != CityManager.Activated_City_Component) hide_explosion(explosion_list);
            }
        }
    }

    public void destroy_train()
    {
        // destroy train and collided train/boxcar
        // TODOED: remove car from city tilemap or vehicle tilemap
        print("destroy train " + id + " at " + Time.time);
        GameManager.train_list.Remove(gameObject);
        city.train_list.Remove(gameObject);
        foreach (GameObject boxcar_go in boxcar_squad)
        {
            Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
            if (boxcar.passenger_go != null)
            {
                Destroy(boxcar.passenger_go);
            }
            boxcar.remove_vehicle_from_board();
            Destroy(boxcar_go);
        }
        foreach (GameObject explosion in explosion_list)
        {
            Destroy(explosion);
        }
        remove_vehicle_from_board();
        Destroy(gameObject);
    }

    public void set_boxcar_turntable_flag()
    {
        foreach (GameObject boxcar in boxcar_squad)
        {
            boxcar.GetComponent<Boxcar>().depart_for_turntable = false;
            boxcar.GetComponent<Boxcar>().leave_turntable = true;
        }
    }

    public int get_boxcar_position(GameObject boxcar_go)
    {
        int position = 0;
        foreach (GameObject boxcar in boxcar_squad)
        {            
            position += 1;
            if (boxcar == boxcar_go) return position;
        }
        throw new Exception("the boxcar does not exist in train list");
    }

    public GameObject get_last_vehicle_added()
    {
        if (boxcar_squad.Count == 0) return gameObject;
        Boxcar boxcar = boxcar_squad[boxcar_squad.Count - 1].GetComponent<Boxcar>();
        return boxcar_squad[boxcar_squad.Count - 1];
    }

    public void turn_on_train(bool menu_state, bool is_game_menu)
    {

        StartCoroutine(switch_on_vehicle(menu_state));
        foreach (GameObject boxcar_object in boxcar_squad)
        {
            Boxcar boxcar = boxcar_object.GetComponent<Boxcar>();
            if (is_game_menu)
            {
                if (!boxcar.in_city && menu_state) 
                {
                    print("turn on train boxcar " + boxcar.boxcar_id + " NOT IN CITY and game mneu is on. Turn ON sprit e renderer");
                    StartCoroutine(boxcar.switch_on_vehicle(true));
                    continue;
                }
            }
            else // is city menu
            {
                if (boxcar.in_city && menu_state)
                {
                    print("not GAME MENU. TURN ON TRAIN boxcar " + boxcar.boxcar_id + " IN CITY and . Turn ON sprit e renderer");
                    StartCoroutine(boxcar.switch_on_vehicle(true));
                    continue;
                }
            }
            print("switch vehicle OFF");
            StartCoroutine(boxcar.switch_on_vehicle(false));
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
        //if (city == CityManager.Activated_City_Component) GameManager.train_menu_manager.update_train_menu(city);
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
        is_train_departed_for_turntable = false; //reset
        station_track = city.add_train_to_station(gameObject, orientation);
        //city.turn_table.GetComponent<Turntable>().add_train_to_queue(gameObject);
        if (station_track != null)
        {
            Vector3Int station_tile_position = station_track.start_location; // A NON-NULLABLE TYPE? ? ?
            GameManager.vehicle_manager.depart(gameObject, station_tile_position, city.city_board);
            assign_station_to_boxcar();
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
                return i + 1;
            }
        }
        throw new Exception("boxcar with id " + boxcar_id + " should belong to train");
    }

    public void board_turntable(RouteManager.Orientation orientation, bool depart_turntable)
    {
        if (depart_turntable)
        {
            start_all_boxcar_at_turntable();
            halt_train(true, false); //unhalt the boxcars
            halt_train(is_halt = false, is_pause = false); // unpause the train
            is_train_departed_for_turntable = true;
        }
        else // leaving the turntable
        {
            leave_city = true;
            foreach (GameObject boxcar_object in boxcar_squad)
            {
                Boxcar boxcar = boxcar_object.GetComponent<Boxcar>();
                boxcar.leave_city = true;
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
        if (is_halt)
            this.is_halt = state;
        else { is_pause = state; }
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            GameObject boxcar = boxcar_squad[i];
            if (is_halt) boxcar.GetComponent<Boxcar>().is_halt = state;
            else { boxcar.GetComponent<Boxcar>().is_pause = state; }
        }
    }

    public void set_boxcar_wait_flag (bool flag)
    {
        is_wait_for_turntable = flag;
        foreach (GameObject boxcar_go in boxcar_squad)
        {
            boxcar_go.GetComponent<Boxcar>().is_wait_for_turntable = flag;
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
        Tilemap tilemap = GameManager.track_manager.top_tilemap; // TODOED update the next tile pos if track is switched 
        string cur_tile_name = tilemap.GetTile(tile_position).name;
        while (true)
        {
            Tile next_track_tile = (Tile)tilemap.GetTile((Vector3Int)next_tile_pos);
            Tile cur_tile = (Tile)tilemap.GetTile(tile_position);
            if (cur_tile_name != cur_tile.name) // if  the current track has switched, update the next track
            {
                print("current track has switched from " + cur_tile_name + " to " + cur_tile.name);
                next_tile_pos = RouteManager.get_destination(this, tilemap, offset).tile_dest_pos;
                next_track_tile = (Tile)tilemap.GetTile((Vector3Int)next_tile_pos);
            }
            cur_tile_name = cur_tile.name;
            Tile city_tile = (Tile) RouteManager.city_tilemap.GetTile((Vector3Int)next_tile_pos);
            // also need to check if a city is placed in the next tile
            if (city_tile != null || (next_track_tile != null && TrackManager.is_track_a_path(orientation, next_track_tile.name, cur_tile.name))) // TODOED make sure the train can cross this track
            {
                next_tilemap_position = (Vector2Int)tile_position; // reset train's position & orientation
                final_orientation = orientation;
                break;
            }
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

    public int get_boxcar_by_id(int boxcar_id)
    {
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            GameObject boxcar_go = boxcar_squad[i];
            Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
            if (boxcar.boxcar_id == boxcar_id)
            {
                return i;
            }
        }
        throw new Exception("Could not find boxcar id " + boxcar_id + " to remove");
    }

    public void remove_boxcar(int remove_boxcar_id) 
    {
        int remove_boxcar_index = get_boxcar_by_id(remove_boxcar_id);
        GameObject boxcar_go = boxcar_squad[remove_boxcar_index];
        boxcar_squad.RemoveAt(remove_boxcar_index);
        Destroy(boxcar_go);
    }

    public void start_all_boxcar_at_turntable()
    {
        foreach (GameObject boxcar_go in boxcar_squad)
        {
            Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
            boxcar.speed = normal_speed;
        }
    }

    public void stop_single_boxcar_at_turntable(GameObject boxcar_go)
    {
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        Tilemap boxcar_tilemap = CityDetector.boxcar_orientation_to_offset_tilemap(boxcar.orientation);
        Vector3Int boxcar_cell_pos = boxcar_tilemap.WorldToCell(boxcar_go.transform.position);
        print("boxcar " + boxcar_go.name + " tile position " + boxcar.tile_position + " prev tile position " + boxcar.prev_tile_position);
        boxcar.prev_tile_position = boxcar.tile_position; // move up  one
        boxcar.tile_position = boxcar_cell_pos;
        GameManager.vehicle_manager.update_vehicle_board(city.city_board, boxcar_go, boxcar_cell_pos, boxcar.prev_tile_position); // nullify prev tile
        boxcar.speed = stopping_speed;
        boxcar.is_halt = true; // only call ONCE when first called
    }

    public void stop_all_boxcar_at_turntable()
    {
        foreach (GameObject boxcar_go in boxcar_squad)
        {
            stop_single_boxcar_at_turntable(boxcar_go);
        }
    }

    public bool is_boxcar_first(int boxcar_id)
    {
        if (boxcar_squad.Count > 0)
        {
            GameObject boxcar_go = boxcar_squad[0];
            if (boxcar_go.GetComponent<Boxcar>().boxcar_id == boxcar_id) return true;
        }
        return false;
    }

    public void attach_boxcar(GameObject boxcar)
    {
        boxcar_squad.Add(boxcar);
    }
}
