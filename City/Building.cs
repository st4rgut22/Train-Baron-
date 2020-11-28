using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public RouteManager.Orientation building_orientation;
    public int building_id;
    public int occupant_id;
    public GameObject[] person_grid;
    public string building_type;
    public int max_capacity;
    public int current_capacity;
    public int total_occupant;
    public Vector2Int offset_position; // offset from bottom left of tilemap to get the true tile coordinates
    public GameObject person;

    private void Awake()
    {
        occupant_id = 0;
        total_occupant = 0;
        current_capacity = 1;
        person = (GameObject)Resources.Load("Person");
    }

    private void Start()
    {

    }

    private void Update()
    {
        
    }

    public void add_occupant(GameObject person_object)
    {
        for (int i=0; i < current_capacity; i++)
        {
            if (person_grid[i] == null)
            {
                person_grid[i] = person_object;
            }
        }
        total_occupant += 1;
    }

    public void spawn_person(Vector2Int tile_pos, RouteManager.Orientation orientation)
    {
        if (total_occupant < max_capacity)
        {
            GameObject new_occupant = Instantiate(person);
            print("tile pos is " + tile_pos);
            new_occupant.transform.position = RouteManager.track_tilemap.GetCellCenterWorld((Vector3Int)tile_pos);
            print("position is " + new_occupant.transform.position);
            add_occupant(new_occupant);
            new_occupant.GetComponent<Person>().set_tile_pos(tile_pos);
            new_occupant.GetComponent<Person>().orientation = orientation;
        }
    }

    public void get_position_in_building(int length, Vector2Int cell_pos)
    {
        for (int i = 0; i < length; i++) // get position in building using offset
        {
            cell_pos = RouteManager.get_straight_next_tile_pos(building_orientation, cell_pos);
        }
    }

    public void do_random_walk()
    {
        System.Random rand = new System.Random();
        // building occupants move from tile to tile without going in the same tile (if possible)
        for (int p = 0; p < person_grid.Length; p++)
        {
            GameObject person_object = person_grid[p];
            if (person_object != null)
            {
                Person person = person_object.GetComponent<Person>();
                RouteManager.Orientation orientation = person.orientation;
                RouteManager.Orientation final_orientation = orientation;
                Vector2Int building_cell_pos = offset_position;
                get_position_in_building(p, building_cell_pos); // p is the distance from offset tile
                                                                // respect the buidling boundaries
                if (p == 0) final_orientation = building_orientation;
                else if (p == person_grid.Length - 1) final_orientation = TrackManager.flip_straight_orientation(building_orientation);
                else
                {
                    int rand_int = rand.Next(0, 2);
                    if (rand_int == 0) // set person orientation to be random
                        final_orientation = TrackManager.flip_straight_orientation(orientation);
                }
                person.orientation = final_orientation;
                Vector2Int final_cell = RouteManager.get_straight_next_tile_pos(final_orientation, building_cell_pos);
                // initiate person move
                StartCoroutine(person.straight_move(final_cell));
            }
        }
    }
}
