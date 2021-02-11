using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : Structure
{
    // room tile (end of house, beginning of house)
    // affiliated Building
    // affiliated Person
    // Building has a List<Room>
    public Building building;
    public int id;
    public bool booked;
    public GameObject person_go_instance;
    public Vector2Int tile_position;
    public GameObject bottom_left_door;
    public GameObject bottom_right_door;
    public GameObject outer_door;
    public GameObject inner_door;
    public GameObject unlocked_door; // door the person will arrive at
    public Door_Prop outer_door_prop;
    public Door_Prop inner_door_prop;
    public GameObject outer_door_container;
    public GameObject inner_door_container;
    public Vector2 right_door_offset; // offset to the opposite side of the house
    public Vector2 pivot_door_offset; // offset to the opposite side of the house
    public GameObject Door;
    public bool has_person;

    // child door go
    //TODO: create an offset dictionary for each type of door?

    private void Awake()
    {
        right_door_offset = new Vector2(.51f, 0f);
        booked = false;
        outer_door = null;
        inner_door = null;
        has_person = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (building.is_room_hidden())
        {
            display_structure(inner_door, false);
            display_structure(outer_door, false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void display_door()
    {

    }

    public bool is_sprite_right_door(Sprite sprite)
    {
        if (sprite == right_door_top_right)
        {
            return true;
        }
        else { return false; }
    }

    public void position_door(GameObject door_go, float offset_x, float offset_y, GameObject door_sprite_go)
    {
        Door door = door_go.GetComponent<Door>();
        float door_rotation = door.tile_rotation;
        door.transform.position += new Vector3(offset_x, offset_y, 0);
        if (is_sprite_right_door(door.door_sprite)) // right door pivot is not bottom left, so move it so it fits inside the room
        {
            door_sprite_go.transform.position += (Vector3)right_door_offset;
        }
        if (door.door_sprite == right_door_top_right)
        {
            pivot_door_offset = new Vector2(.37f, 0f);
            door_sprite_go.transform.position += (Vector3)pivot_door_offset;
        }
        else
        {
            pivot_door_offset = new Vector2(0f, -.07f);
            door_sprite_go.transform.position += (Vector3)pivot_door_offset;
        }
        door_go.transform.SetParent(transform);
        door_go.transform.eulerAngles = new Vector3(0, 0, door_rotation);
    }

    public GameObject spawn_door(Door_Prop door_prop)
    {
        GameObject door_go = Instantiate(Door);
        Door door = door_go.GetComponent<Door>();
        door.set_sprite(door_prop.pivot_door);
        door.door_rotation = door_prop.door_rotation;
        door.tile_rotation = door_prop.rotation;
        return door_go;
    }

    public void spawn_door_container()
    {
        float offset_x = RouteManager.cell_width * tile_position.x;
        float offset_y = RouteManager.cell_width * tile_position.y;
        if (outer_door_prop != null)
        {
            outer_door_container = spawn_door(outer_door_prop);
            outer_door_container.GetComponent<Door>().is_outer = outer_door_prop.is_outer;
            GameObject outer_door_sprite_go = outer_door_container.GetComponent<Door>().door_sprite_go;
            outer_door = outer_door_sprite_go;
            position_door(outer_door_container, offset_x, offset_y, outer_door_sprite_go);
        }
        if (inner_door_prop != null)
        {
            inner_door_container = spawn_door(inner_door_prop);
            GameObject primary_door_sprite_go = inner_door_container.GetComponent<Door>().door_sprite_go;
            inner_door = primary_door_sprite_go;
            position_door(inner_door_container, offset_x, offset_y, primary_door_sprite_go);
        }

    }

    public Person spawn_person(bool is_person_poor)
    {
        person_go_instance = GameManager.person_manager.GetComponent<PersonManager>().create_person(is_person_poor);
        has_person = true;
        Person person = person_go_instance.GetComponent<Person>();
        person.is_egghead_thinking = true;
        person.room = this;
        person.city = building.city;
        person.prev_city = person.city;
        person.prev_city = building.city;
        set_person_position(person);
        add_occupant(this.person_go_instance);
        return person;
    }

    public void set_person_position(Person person)
    {
        person.transform.position = RouteManager.track_tilemap.GetCellCenterWorld((Vector3Int)tile_position);
        person.set_tile_pos(tile_position);
        RouteManager.Orientation orientation = RouteManager.Orientation.South;
        person.set_orientation(orientation);
    }

    public void clear_room()
    {
        person_go_instance = null;
        booked = false;

    }

    public void add_occupant(GameObject person_object)
    {
        person_go_instance = person_object;
        booked = true;
    }

    public void display_occupant(bool display)
    {
        person_go_instance.GetComponent<SpriteRenderer>().enabled = display;
        person_go_instance.GetComponent<Person>().thought_bubble.GetComponent<SpriteRenderer>().enabled = display;
    }
}
