using UnityEngine;

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
    public string initial_building_lot_name;

    private void Awake()
    {
        initial_building_lot_name = "Building Lot West";
        occupant_id = 0;
        total_occupant = 0;
        room_id = 0;
    }

    public void Start()
    {
        print(gameObject.name);
        roomba = new GameObject[max_capacity];
        city.city_tilemap_go.SetActive(false); // after setting tile deactivate gameobject
        if (building_lot.id == initial_building_lot_name)
        {
            spawn_room();
        }
    }

    private void Update()
    {
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
                display_structure(room.primary_door, is_display);
            }
        }
    }

    public void remove_last_room()
    {
        Vector2Int room_pos = last_room_position;
        Room room = city.city_room_matrix[room_pos.x, room_pos.y];
        city.city_room_matrix[room_pos.x, room_pos.y] = null;
        roomba[room.id] = null;
        Vector2Int room_tile_pos = RouteManager.get_straight_next_tile_pos_multiple(building_orientation, offset_position, room.id-1);
        city.city_tilemap.SetTile((Vector3Int)room_pos, null);
        current_capacity -= 1;
    }

    public Vector2Int get_last_room_position()
    {
        for (int i = roomba.Length - 1; i >= 0; i--)
        {
            if (roomba[i] != null) return roomba[i].GetComponent<Room>().tile_position;
        }
        throw new System.Exception("there are no rooms left");
    }

    public int get_new_room_id()
    {
        for (int i=0; i<roomba.Length; i++)
        {
            GameObject room_go = roomba[i];
            if (room_go == null) return i;
        }
        throw new System.Exception("there are no rooms available");
    }

    public virtual Room spawn_room()
    {
        GameObject room_object = Instantiate(this.room);
        Room room = room_object.GetComponent<Room>();
        room.id = get_new_room_id();
        Vector2Int room_tile_pos = RouteManager.get_straight_next_tile_pos_multiple(building_orientation, offset_position, room.id);
        city.city_room_matrix[room_tile_pos.x, room_tile_pos.y] = room;
        room.tile_position = room_tile_pos;
        room.outer_door_prop = building_lot.outer_door;
        room.primary_door_prop = building_lot.primary_door;
        room.spawn_door_container();
        room.building = this;
        roomba[room.id] = room_object;
        current_capacity += 1;
        return room;
    }

    public bool is_room_hidden()
    {
        if (city != CityManager.Activated_City_Component) return true;
        else { return false; }
    }
}
