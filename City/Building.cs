using UnityEngine;
using System.Collections;

public class Building : Structure
{
    public RouteManager.Orientation building_orientation;
    public GameObject room;
    public GameObject[] roomba;
    public City city;
    public int building_id;
    public int room_id;
    public int occupant_id;
    public GameObject[] person_grid;
    public string building_type;
    public int max_capacity;
    public int current_capacity;
    public int total_occupant;
    public Vector2Int offset_position; // offset from bottom left of tilemap to get the true tile coordinates
    public Vector2Int last_room_position;
    public BuildingLot building_lot;
    public Stack last_room_position_stack;

    private void Awake()
    {
        last_room_position_stack = new Stack();
        occupant_id = 0;
        total_occupant = 0;
        room_id = 0;
    }

    public void Start()
    {
        roomba = new GameObject[max_capacity];
        city.city_tilemap_go.SetActive(false); // after setting tile deactivate gameobject
        if (building_lot.id == city.initial_building_lot_name) 
        {
            Room room = spawn_room();
            //print(city.city_type + " increment total room by 1. Total room is NOW " + CityManager.total_room);
            //print("bldg name is " + gameObject.name);
            if (gameObject.name.Contains("Station"))
            {
                Person person = room.spawn_person(true);
                person.initialize_egghead(false, false);
            }
        }
    }

    private void Update()
    {
    }

    public bool is_building_empty()
    {
        for (int i = 0; i < roomba.Length; i++)
        {
            if (roomba[i] != null)
            {
                return false;
            }
        }
        return true;
    }

    public int get_occupancy_count()
    {
        int occupancy_count = 0;
        for (int i = 0; i < roomba.Length; i++)
        {
            if (roomba[i] != null)
            {
                Room room = roomba[i].GetComponent<Room>();
                if (room.booked) occupancy_count += 1;
            }
        }
        return occupancy_count;
    }

    public int get_vacancy_count()
    {
        int vacancy_count = 0;
        for (int i = 0; i < roomba.Length; i++)
        {
            if (roomba[i] != null)
            {
                Room room = roomba[i].GetComponent<Room>();
                if (!room.booked) vacancy_count += 1;
            }
        }
        return vacancy_count;
    }

    public void add_occupant_to_available_room(GameObject person_go_instance)
    {
        foreach (GameObject room_go in roomba)
        {
            Room room = room_go.GetComponent<Room>();
            if (room!= null && !room.booked)
            {
                room.add_occupant(person_go_instance);
                Person person = person_go_instance.GetComponent<Person>();
                //TODO: some movmenet script as a transition
                room.set_person_position(person);
                break;
            }               
        }
    }

    public void reveal_building_rooms(bool is_display)
    {
        for (int i = 0; i < roomba.GetLength(0); i++)
        {
            GameObject room_go = roomba[i];
            if (room_go != null)
            {
                Room room = room_go.GetComponent<Room>();
                if (room.booked)
                    room.display_occupant(is_display);
                //todo: activate room, which also activates the door and everything inside it
                //room_go.SetActive(is_display);
                display_structure(room.outer_door, is_display);
                display_structure(room.inner_door, is_display);
            }
        }
    }

    public bool remove_last_room()
    {
        if (last_room_position_stack.Count == 0)
            return false;
        Vector2Int room_pos = (Vector2Int) last_room_position_stack.Pop();
        Room boarded_room = city.city_room_matrix[room_pos.x, room_pos.y];
        
        city.city_room_matrix[room_pos.x, room_pos.y] = null;
        if (boarded_room.has_person)
        {
            GameObject person_go = boarded_room.person_go_instance;
            Person person = person_go.GetComponent<Person>();
            if (person.desired_activity != "") // not in a period of waiting for activity
                PersonManager.add_notification_for_city(person.city.tilemap_position, false); // remove notification
            string review_summary = "I'll go somewhere that can stick to the train schedule.";
            person.leave_review(person.city, Person.Review.One_Star);
            person.update_review_page(review_summary, (int)Person.Review.One_Star);
            DestroyImmediate(person_go); // otherwise get error from coroutine
            CityManager.update_total_people(-1);
        }
        else if (boarded_room.booked)// person has not arrived yet, but the room is booked. Don't remove this room. 
        {
            return false;
        }
        DestroyImmediate(boarded_room.inner_door_container);
        DestroyImmediate(boarded_room.outer_door_container);
        roomba[boarded_room.id] = null;
        city.city_tilemap.SetTile((Vector3Int)room_pos, city.boarded_up_tile);
        current_capacity -= 1;
        return true;
    }

    public Vector2Int get_room_pos(int room_count)
    {
        // populate the first available room
        Vector2Int room_tile_pos = new Vector2Int(offset_position.x, offset_position.y);
        for (int i = 0; i < room_count; i++)
        {
            room_tile_pos.x += 1;
        }
        return room_tile_pos;
    }

    public int get_new_room_id()
    {
        // populate the first available room
        for (int i=0; i<roomba.Length; i++)
        {
            GameObject room_go = roomba[i];
            if (room_go == null) return i;
        }
        throw new System.Exception("there are no rooms available");
    }

    public virtual Room spawn_room()
    {
        CityManager.total_room += 1;
        city.total_room += 1;
        GameObject room_object = Instantiate(this.room);
        Room room = room_object.GetComponent<Room>();
        room.id = get_new_room_id();
        Vector2Int room_tile_pos = new Vector2Int(offset_position.x, offset_position.y);
        for (int i=0;i<room.id; i++)
        {
            room_tile_pos.x += 1;
        }
        city.city_room_matrix[room_tile_pos.x, room_tile_pos.y] = room;
        room.tile_position = room_tile_pos;
        room.outer_door_prop = BuildingLot.station_door_map[building_lot.orientation][1];
        room.outer_door_prop.is_outer = true;
        room.inner_door_prop = BuildingLot.station_door_map[building_lot.orientation][0];
        room.inner_door_prop.is_outer = false;
        room.spawn_door_container();
        room.building = this;
        roomba[room.id] = room_object;
        current_capacity += 1;
        last_room_position_stack.Push(room_tile_pos);
        return room;
    }

    public bool is_room_hidden()
    {
        if (city != CityManager.Activated_City_Component) return true;
        else { return false; }
    }
}
