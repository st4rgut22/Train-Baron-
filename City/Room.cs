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
    public bool occupied;
    public GameObject person_go;
    public Vector2Int tile_position;
    public GameObject bottom_left_door;
    public GameObject bottom_right_door;
    public GameObject outer_door;
    public GameObject primary_door;
    public GameObject unlocked_door; // door the person will arrive at
    public Door_Prop outer_door_prop;
    public Door_Prop primary_door_prop;
    public GameObject outer_door_container;
    public GameObject primary_door_container;
    public Vector2 right_door_offset; // offset to the opposite side of the house
    public Vector2 pivot_door_offset; // offset to the opposite side of the house
    public GameObject Door;

    // child door go
    //TODO: create an offset dictionary for each type of door?

    private void Awake()
    {
        right_door_offset = new Vector2(.51f, 0f);
        occupied = false;
        outer_door = null;
        primary_door = null; 
    }

    // Start is called before the first frame update
    void Start()
    {
        if (building.is_room_hidden())
        {
            display_structure(primary_door, false);
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
        if (sprite == right_door_bottom_left ||
            sprite == right_door_bottom_right ||
            sprite == right_door_top_left ||
            sprite == right_door_top_right )
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
        if (door.door_sprite == right_door_top_right || door.door_sprite == left_door_top_right)
        {
            pivot_door_offset = new Vector2(.37f, 0f);
            door_sprite_go.transform.position += (Vector3)pivot_door_offset;
        }
        if (door.door_sprite == right_door_bottom_right)
        {
            pivot_door_offset = new Vector2(.37f, -.07f);
            door_sprite_go.transform.position += (Vector3)pivot_door_offset;
        }
        if (door.door_sprite == left_door_bottom_left)
        {
            pivot_door_offset = new Vector2(0f, -.07f);
            door_sprite_go.transform.position += (Vector3)pivot_door_offset;
        }
        door_go.transform.SetParent(transform);
        door_go.transform.eulerAngles = new Vector3(0, 0, door_rotation);
        //StartCoroutine(door.rotate());
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
            GameObject outer_door_sprite_go = outer_door_container.GetComponent<Door>().door_sprite_go;
            outer_door = outer_door_sprite_go;
            position_door(outer_door_container, offset_x, offset_y, outer_door_sprite_go);
        }
        if (primary_door_prop != null)
        {
            primary_door_container = spawn_door(primary_door_prop);
            GameObject primary_door_sprite_go = primary_door_container.GetComponent<Door>().door_sprite_go;
            primary_door = primary_door_sprite_go;
            position_door(primary_door_container, offset_x, offset_y, primary_door_sprite_go);
        }

    }

    public void spawn_person()
    {
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
        RouteManager.Orientation orientation = CityManager.initial_person_face_map[building_lot_name];
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

    public void display_contents(bool display)
    {
        person_go.GetComponent<SpriteRenderer>().enabled = display;
        display_structure(primary_door, display);
        display_structure(outer_door, display);
    }

    public void display_occupant(bool display)
    {
        person_go.SetActive(display);
    }
}
