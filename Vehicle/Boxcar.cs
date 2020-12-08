using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Boxcar : MovingObject
{
    public Train train = null; // the train boxcar is attached to
    Vector3 idling_position; // boxcar's default position while waiting for turntable to arrive
    public int boxcar_id;
    public string boxcar_type;
    public bool departing = false;
    public bool receive_train_order = true;
    //Cargo cargo; // accomodate different cargo, people, vaccine, etc.
    public bool is_occupied;
    public GameObject passenger_go;


    private void Awake()
    {      
        base.Awake();
        is_occupied = false;

    }

    void Start()
    {
        base.Start();
        boxcar_type = gameObject.name.Replace("(Clone)", "");
        idling_position = new Vector3(-1, -1, -1);
        departing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!in_city || (in_city && receive_train_order)) // delay movement updates until train orders boxcar to depart
            base.Update();
    }

    public void initialize_boxcar(int id)
    {
        boxcar_id = id;
        speed_multiplier = 1.03f; // when boxcar is created go a little faster so it can keep up with train
    }

    public override void arrive_at_city()
    {
        base.arrive_at_city();
        //departing = true;
        //city.add_boxcar(gameObject);
    }

    public RouteManager.Orientation get_orientation()
    {
        return orientation;
    }

    public void set_depart_status(bool status)
    {
        in_tile = true;
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
        if (train !=null && in_city && train.is_pause)
        {
            print("set boxcar hint");
            RouteManager.Orientation station_orientation = station_track.station.orientation;
            int is_inner = station_track.inner;
            int[,,] loading_coord = TrackManager.unloading_coord_map[station_orientation];
            List<int[]> valid_unloading_pos_list = get_unloading_pos(loading_coord, is_inner);
            List<int[]> valid_parking_pos_list = get_parking_list();
            List<int[]> filtered_parking_pos_list = filter_available_parking_spot(valid_parking_pos_list);
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


    public List<int[]> get_unloading_pos(int[,,] valid_pos, int is_inner)
    {
        //unload people to home, eligible tile covers the entire building
        List<string> valid_structure_list = CityManager.cargo_to_structure[boxcar_type];
        List<int[]> unloading_pos_list = new List<int[]>();
        bool building_has_room = false;
        if (valid_structure_list.Contains(city.city_type)) 
        {

            List<int[]> temp_unloading_pos_list = new List<int[]>();
            for (int c = 0; c < valid_pos.GetLength(1); c++)
            {
                int x = valid_pos[is_inner, c, 0];
                int y = valid_pos[is_inner, c, 1];
                temp_unloading_pos_list.Add(new int[] { x, y });
                Room room = city.city_room_matrix[x, y];
                if (room!=null && !room.occupied) // room is not occupied
                {
                    building_has_room = true;
                }
            }
            if (building_has_room)
                unloading_pos_list.AddRange(temp_unloading_pos_list);
        }
        else
        {
            print("boxcar cargo type " + boxcar_type + " does not match building type " + city.city_type);
        }
        return unloading_pos_list;
    }

    //public override void OnBeginDrag(PointerEventData eventData)
    //{
    //    if (in_city && train.is_pause) // waiting for turntable to arrive
    //    {
    //        base.OnBeginDrag(eventData);
    //        idling_position = transform.position; // if train departs before placing a boxcar in parking spot, then boxcar departs too
    //    }
    //}

    //public override void OnDrag(PointerEventData eventData)
    //{
    //    if (in_city && train.is_pause) // waiting for turntable to arrive
    //    {
    //        Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
    //        transform.position = world_position;
    //    } else
    //    {
    //        // TODO: abort drag, if the train is departing

    //    }
    //}

    //public override void OnEndDrag(PointerEventData eventData)
    //{
    //    // destroy the boxcar if it is placed in a parking spot
    //    if (in_city && train.is_pause)
    //    {
    //        Vector2Int selected_tile = GameManager.get_selected_tile(Input.mousePosition);
    //        bool parking_available = city.is_parking_spot_available(selected_tile);
    //        if (parking_available)
    //        {
    //            city.place_boxcar_tile(gameObject, selected_tile);
    //            GameManager.vehicle_manager.boxcar_fill_void(train, boxcar_id, idling_position); // move boxcars behind this one forward
    //            train.remove_boxcar(boxcar_id);
    //            Destroy(gameObject);
    //        }
    //        else
    //        {
    //            transform.position = idling_position;
    //        }
    //    }
    //}
}
