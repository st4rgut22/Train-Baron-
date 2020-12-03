﻿using System.Collections;
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

    private void Awake()
    {
        occupied = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawn_person()
    {
        Room[,] occup = building.city.city_room_matrix;
        this.person_go = Instantiate(person_go);
        set_person_position();
        add_occupant(this.person_go);
    }

    public void set_person_position()
    {
        person_go.GetComponent<Person>().transform.position = RouteManager.track_tilemap.GetCellCenterWorld((Vector3Int)tile_position);
        person_go.GetComponent<Person>().set_tile_pos(tile_position);
        person_go.GetComponent<Person>().set_orientation(building.building_orientation);
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
