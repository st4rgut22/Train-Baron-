using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public Building_Lot building_lot;
    
    private void Awake()
    {
        occupant_id = 0;
        total_occupant = 0;
        room_id = 0;
    }

    private void Start()
    {
        city.city_tilemap_go.SetActive(false); // after setting tile deactivate gameobject
        roomba = new GameObject[max_capacity];
        spawn_room();
    }

    private void Update()
    {
        
    }

    public void add_occupant_to_available_room(GameObject person_go)
    {
        foreach (GameObject room_go in roomba)
        {
            Room room = room_go.GetComponent<Room>();
            if (room!= null && !room.occupied)
            {
                room.add_occupant(person_go);
                Person person = person_go.GetComponent<Person>();
                //TODO: some movmenet script as a transition
                room.set_person_position(person);
                break;
            }               
        }
    }

    public void reveal_room(bool is_display)
    {
        for (int i = 0; i < roomba.GetLength(0); i++)
        {
            GameObject room_go = roomba[i];
            if (room_go != null)
            {
                Room room = room_go.GetComponent<Room>();
                if (room.occupied)
                    room.display_occupant(is_display);
                //todo: activate room, which also activates the door and everything inside it
                //room_go.SetActive(is_display);
                display_structure(room.outer_door, is_display);
                display_structure(room.primary_door, is_display);
            }
        }
    }

    public virtual Room spawn_room()
    {
        GameObject room_object = Instantiate(this.room);
        Room room = room_object.GetComponent<Room>();
        room.id = room_id;
        room.building = this;
        room.outer_door_container = building_lot.outer_door;
        room.primary_door_container = building_lot.primary_door;
        roomba[room.id] = room_object;
        Vector2Int room_tile_pos = RouteManager.get_straight_next_tile_pos_multiple(building_orientation, offset_position, room.id);
        city.city_room_matrix[room_tile_pos.x, room_tile_pos.y] = room;
        room.tile_position = room_tile_pos;
        last_room_position = room_tile_pos;
        current_capacity += 1;
        room_id += 1;
        return room;
    }

    public bool is_room_hidden()
    {
        if (city != CityManager.Activated_City_Component) return true;
        else { return false; }
    }
}
