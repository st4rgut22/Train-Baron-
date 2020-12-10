using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    // room tile (end of house, beginning of house)
    // affiliated Building
    // affiliated Person
    // Building has a List<Room>
    public Building building;
    public int id;
    public bool occupied;
    public GameObject person_go;
    public Vector2Int tile_position;
    public GameObject bottom_left_door;
    public GameObject bottom_right_door;
    public GameObject bl_door;
    public GameObject br_door;
    public GameObject unlocked_door; // door the person will arrive at 
    public float door_1_rotation;
    public float door_2_rotation;

    private void Awake()
    {
        occupied = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        spawn_door();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void rotate_door()
    {
        //todo
    }

    public void spawn_door()
    {
        float offset_x = RouteManager.cell_width * tile_position.x;
        float offset_y = RouteManager.cell_width * tile_position.y;
        if (door_1_rotation != -1.0f)
        {
            bl_door = Instantiate(bottom_left_door);
            bl_door.transform.eulerAngles = new Vector3(0, 0, door_1_rotation);
            bl_door.transform.position += new Vector3(offset_x, offset_y, 0);
            bl_door.SetActive(false);
        }
        if (door_2_rotation != -1.0f)
        {
            br_door = Instantiate(bottom_right_door);
            br_door.transform.eulerAngles = new Vector3(0, 0, door_2_rotation);
            br_door.transform.position += new Vector3(offset_x, offset_y, 0);
            br_door.SetActive(false);
        }
    }

    public void spawn_person()
    {
        Room[,] occup = building.city.city_room_matrix;
        person_go = Instantiate(person_go);
        Person person = person_go.GetComponent<Person>();
        person.room = this;
        set_person_position(person);
        add_occupant(this.person_go);
    }

    public void set_person_position(Person person)
    {
        person.transform.position = RouteManager.track_tilemap.GetCellCenterWorld((Vector3Int)tile_position);
        person.set_tile_pos(tile_position);
        person.set_orientation(building.building_orientation);
    }

    public void clear_room()
    {
        person_go = null;
        occupied = false;

    }

    public void add_occupant(GameObject person_object)
    {
        person_go = person_object;
        occupied = true;
    }

    public void display_occupant(bool display)
    {
        person_go.GetComponent<SpriteRenderer>().enabled = display;
    }
}
