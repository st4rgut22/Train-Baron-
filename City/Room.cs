using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    // room tile (end of house, beginning of house)
    // affiliated Building
    // affiliated Person
    // Building has a List<Room>
    Building building;
    public int id;
    public bool occupied;
    public GameObject person_go;
    public Vector2Int tile_position;

    // Start is called before the first frame update
    void Start()
    {
        occupied = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawn_person()
    {
        this.person_go = Instantiate(person_go);
        set_person_position();
    }

    public void set_person_position()
    {
        person_go.GetComponent<Person>().transform.position = RouteManager.track_tilemap.GetCellCenterWorld((Vector3Int)tile_position);
        person_go.GetComponent<Person>().set_tile_pos(tile_position);
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
