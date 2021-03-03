using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Boxcar : MovingObject
{
    public Train train = null; // the train boxcar is attached to
    public int boxcar_id;
    public string boxcar_type;
    public bool departing = false;
    public bool receive_train_order = true;
    //Cargo cargo; // accomodate different cargo, people, vaccine, etc.
    public bool is_occupied;
    public GameObject passenger_go;
    Vector3 idling_position;
    public GameObject explosion;
    public GameObject explosion_go;
    public bool is_stopped;
    public bool is_being_boarded;

    private void Awake()
    {      
        base.Awake();
        is_stopped = false;
        is_occupied = false;
        is_halt = true;
        is_being_boarded = false;
    }

    void Start()
    {
        base.Start();
        boxcar_type = gameObject.name.Replace("(Clone)", "");
        BoxCollider2D bc2d = GetComponent<BoxCollider2D>();
        bc2d.size = new Vector2(1f, .73f);
        stop_car_if_wait_tile();
        idling_position = new Vector3(-1, -1, -1);
        departing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_halt && !in_tile && !end_of_track)
            set_destination();
        if (!in_city || (in_city && receive_train_order)) // delay movement updates until train orders boxcar to depart
            base.Update();
    }

    public bool is_last_vehicle_in_list()
    {
        int boxcar_pos = train.get_boxcar_position(gameObject);
        if (boxcar_pos == 4) return true;
        else { return false; }
    }

    //public void stop_single_boxcar()
    //{
    //    if (!train.is_train_departed_for_turntable)
    //    {
    //        if (station_track.station.orientation == RouteManager.Orientation.South || station_track.station.orientation == RouteManager.Orientation.North)
    //        {
    //            int boxcar_position = gameObject.GetComponent<Boxcar>().train.get_boxcar_position(gameObject);
    //            if (station_track.inner == 0) // outer
    //            {
    //                if (boxcar_position == 4)
    //                {
    //                    //print("OUTER TRACk boxcar position 3. tile position is " + tile_position);
    //                    stop_single_car_if_wait_tile(false);
    //                }
    //            }
    //            else  // inner
    //            {
    //                if (boxcar_position == 4)
    //                {
    //                    //print("INNER TRACk boxcar position 4. tile position is " + tile_position);
    //                    stop_single_car_if_wait_tile(true);
    //                }
    //            }
    //        }
    //    }
    //}

    public void initialize_boxcar(int id)
    {
        boxcar_id = id;
        speed_multiplier = 1.03f; // when boxcar is created go a little faster so it can keep up with train
    }

    public void destroy_boxcar()
    {
        Destroy(explosion_go);
        train.boxcar_squad.Remove(gameObject);
        remove_vehicle_from_board();
        Destroy(gameObject);
    }


    public override void arrive_at_city()
    {
        base.arrive_at_city();
        if (is_occupied)
        {
            Person person = passenger_go.GetComponent<Person>();
            person.set_animation_clip("player_idle_front");
            Quaternion current_rotation = new Quaternion();
            bool is_last_boxcar = person.boxcar_go.GetComponent<Boxcar>().is_last_vehicle_in_list();
            if (station_track.station.orientation == RouteManager.Orientation.North || station_track.station.orientation == RouteManager.Orientation.West)
            {
                if (station_track.station.orientation == RouteManager.Orientation.North && is_last_boxcar && station_track.inner == 0)
                {
                    current_rotation.eulerAngles = new Vector3(0,0,180);
                }
                else { current_rotation.eulerAngles = new Vector3(0, 0, 90); }                
            }
            if (station_track.station.orientation == RouteManager.Orientation.East || station_track.station.orientation == RouteManager.Orientation.South)
            {
                if (station_track.station.orientation == RouteManager.Orientation.South && is_last_boxcar && station_track.inner == 0)
                {
                    current_rotation.eulerAngles = new Vector3(0, 0, 0);
                }
                else { current_rotation.eulerAngles = new Vector3(0, 0, -90); }                
            }
            passenger_go.transform.localRotation = current_rotation; // rotate to correct for occupant turning at bend
        }
    }

    public RouteManager.Orientation get_orientation()
    {
        return orientation;
    }

    public void set_depart_status(bool status)
    {
        departing = status;
    }

    public bool get_depart_status()
    {
        return departing;
    }

    public void attach_to_train(Train train)
    {
        this.train = train;
    }

    public void click_boxcar(PointerEventData eventData)
    {
        List<List<int[]>> boxcar_action_coord = new List<List<int[]>>();
        List<int[]> valid_parking_pos_list = new List<int[]>();
        List<int[]> valid_unloading_pos_list = new List<int[]>();
        if (train !=null && in_city && train.is_pause)
        {
            RouteManager.Orientation station_orientation = station_track.station.orientation;
            int is_inner = station_track.inner;
            List<List<int[]>> loading_coord = TrackManager.unloading_coord_map[station_orientation];
            if (!is_being_boarded)
                valid_unloading_pos_list = get_unloading_pos(loading_coord, is_inner);
            if (!train.is_any_boxcar_being_boarded())
                valid_parking_pos_list = get_parking_list();
            List<int[]> filtered_parking_pos_list = filter_available_parking_spot(valid_parking_pos_list);
            if (is_occupied) filtered_parking_pos_list.Clear(); // don't show available parking lots if boxcar is occupied
            // highlight the tiles for a second
            boxcar_action_coord.Add(valid_unloading_pos_list);
            boxcar_action_coord.Add(filtered_parking_pos_list);
            List<string> hint_context_list = new List<string>() { "unload", "park"};
            game_manager.mark_tile_as_eligible(boxcar_action_coord, hint_context_list, gameObject, true);
        }
    }

    public List<int[]> filter_available_parking_spot(List<int[]> parking_spots)
    {
        List<int[]> filtered_spots = new List<int[]>();
        foreach(int[] parking_spot in parking_spots)
        {
            Vector2Int park_tile_pos = new Vector2Int(parking_spot[0], parking_spot[1]);
            bool parking_available = city.is_parking_spot_available(park_tile_pos);
            if (parking_available)
            {
                filtered_spots.Add(parking_spot);
            }
        }
        return filtered_spots;
    }

    public List<int[]> get_parking_list()
    {
        RouteManager.Orientation orientation = station_track.station.orientation;
        int is_inner = station_track.inner; // 0 means no, 1 means yes
        List<int> parking_coord = TrackManager.parking_coord_map[orientation];
        int y = parking_coord[0];
        int start_x = parking_coord[1];
        int end_x = parking_coord[2];
        List<int[]> valid_parking_coord_list = new List<int[]>();
        for (int i = start_x; i < end_x; i++)
        {
            if (city.gameobject_board[i, y] == null) // parking spot is available
                valid_parking_coord_list.Add(new int[] { i, y });
        }
        return valid_parking_coord_list;
    }


    public List<int[]> get_unloading_pos(List<List<int[]>> valid_pos, int is_inner)
    {
        //unload people to home, eligible tile covers the entire building
        List<int[]> unloading_pos_list = new List<int[]>();
        if (!is_occupied)
            return unloading_pos_list;
        if (passenger_go.GetComponent<Person>().city == city)
        {
            //print("you cant unload to the same city you came from");
            return unloading_pos_list;
        }
        int building_length = valid_pos[is_inner].Count;
        List<int[]> temp_unloading_pos_list = new List<int[]>();
        for (int c = 0; c < building_length; c++)
        {
            int x = valid_pos[is_inner][c][0];
            int y = valid_pos[is_inner][c][1];
            temp_unloading_pos_list.Add(new int[] { x, y });
            Room room = city.city_room_matrix[x, y];
            if (room!=null && !room.booked) // room is not occupied
            {
                unloading_pos_list.Add(new int[] { x, y });
            }
        }
        //if (building_has_room)
        //    unloading_pos_list.AddRange(temp_unloading_pos_list);
        return unloading_pos_list;
    }
}
