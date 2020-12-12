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
    public GameObject outer_door;
    public GameObject primary_door;
    public GameObject unlocked_door; // door the person will arrive at 
    public float outer_door_rotation;
    public float primary_door_rotation;

    private void Awake()
    {
        occupied = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        spawn_door();
        if (building.is_room_hidden())
            gameObject.SetActive(false);
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
        if (outer_door_rotation != -1.0f)
        {
            outer_door = Instantiate(bottom_left_door);
            outer_door.transform.eulerAngles = new Vector3(0, 0, outer_door_rotation);
            outer_door.transform.position += new Vector3(offset_x, offset_y, 0);
            outer_door.transform.SetParent(this.transform);
        }
        if (primary_door_rotation != -1.0f)
        {
            primary_door = Instantiate(bottom_right_door);
            primary_door.transform.eulerAngles = new Vector3(0, 0, primary_door_rotation);
            primary_door.transform.position += new Vector3(offset_x, offset_y, 0);
            primary_door.transform.SetParent(this.transform);
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
        string building_lot_name = building.building_lot.id;
        RouteManager.Orientation orientation = City.initial_person_face_map[building_lot_name];
        person.set_orientation(orientation);
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
