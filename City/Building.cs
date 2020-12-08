using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Building : MonoBehaviour
{
    public RouteManager.Orientation building_orientation;
    public GameObject room;
    public Room[] roomba;
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
        roomba = new Room[max_capacity];
        spawn_room();
        city.city_tilemap_go.SetActive(false); // after setting tile deactivate gameobject
    }

    private void Update()
    {
        
    }

    public void add_occupant_to_available_room(GameObject person_go)
    {
        foreach (Room room in roomba)
        {
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

    public void show_occupants(bool is_occupant_displayed)
    {
        for (int i = 0; i < roomba.GetLength(0); i++)
        {
            Room room = roomba[i];
            if (room != null)
                if (room.occupied)
                    room.display_occupant(is_occupant_displayed);
        }
    }

    public virtual Room spawn_room()
    {
        GameObject room_object = Instantiate(this.room);
        Room room = room_object.GetComponent<Room>();
        room.id = room_id;
        room.building = this;
        roomba[room.id] = room;
        Vector2Int room_tile_pos = RouteManager.get_straight_next_tile_pos_multiple(building_orientation, offset_position, room.id);
        city.city_room_matrix[room_tile_pos.x, room_tile_pos.y] = room;
        room.tile_position = room_tile_pos;
        last_room_position = room_tile_pos;
        current_capacity += 1;
        room_id += 1;
        return room;
    }

    //public void do_random_walk()
    //{
    //    System.Random rand = new System.Random();
    //    // building occupants move from tile to tile without going in the same tile (if possible)
    //    for (int p = 0; p < person_grid.Length; p++)
    //    {
    //        GameObject person_object = person_grid[p];
    //        if (person_object != null)
    //        {
    //            Person person = person_object.GetComponent<Person>();
    //            RouteManager.Orientation orientation = person.orientation;
    //            RouteManager.Orientation final_orientation = orientation;
    //            Vector2Int building_cell_pos = offset_position;
    //            building_cell_pos = get_position_in_building(p, building_cell_pos); // p is the distance from offset tile
    //                                                            // respect the buidling boundaries
    //            if (p == 0) final_orientation = building_orientation;
    //            else if (p == person_grid.Length - 1) final_orientation = TrackManager.flip_straight_orientation(building_orientation);
    //            else
    //            {
    //                int rand_int = rand.Next(0, 2);
    //                if (rand_int == 0) // set person orientation to be random
    //                    final_orientation = TrackManager.flip_straight_orientation(orientation);
    //            }
    //            person.orientation = final_orientation;
    //            Vector2Int final_cell = RouteManager.get_straight_next_tile_pos(final_orientation, building_cell_pos);
    //            // initiate person move
    //            StartCoroutine(person.straight_move(final_cell));
    //        }
    //    }
    //}
}
