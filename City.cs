using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class City : BoardManager
{
    // track city control as a function of supplies, troops, artillery
    Vector3Int tilemap_position;
    List<GameObject> train_list; // list of trains inside a city
    public GameObject[,] city_board; // contains location of vehicles within city
    public List<Station> station_list;

    public static Vector2Int west_end_1 = new Vector2Int(4, 0);
    public static Vector2Int west_end_2 = new Vector2Int(4, 1);
    public static Vector3Int west_start_1 = new Vector3Int(-1, 0, 0);
    public static Vector3Int west_start_2 = new Vector3Int(-1, 1, 0);

    public static Vector2Int north_end_2 = new Vector2Int(4, 5);
    public static Vector2Int north_end_1 = new Vector2Int(4, 6);
    public static Vector3Int north_start_2 = new Vector3Int(0, 7, 0);
    public static Vector3Int north_start_1 = new Vector3Int(1, 7, 0);

    public static Vector2Int east_end_2 = new Vector2Int(10, 5);
    public static Vector2Int east_end_1 = new Vector2Int(10, 6);
    public static Vector3Int east_start_2 = new Vector3Int(15, 5, 0);
    public static Vector3Int east_start_1 = new Vector3Int(15, 6, 0);

    public static Vector2Int south_end_1 = new Vector2Int(10, 1);
    public static Vector2Int south_end_2 = new Vector2Int(10, 0);
    public static Vector3Int south_start_1 = new Vector3Int(14, -2, 0);
    public static Vector3Int south_start_2 = new Vector3Int(13, -2, 0);

    Station West_Station = new Station(west_start_1, west_start_2);
    Station North_Station = new Station(north_start_1, north_start_2);
    Station East_Station = new Station(east_start_1, east_start_2);
    Station South_Station = new Station(south_start_1, south_start_2);

    public GameObject Turntable;
    public GameObject Turntable_Circle;
    public GameObject turn_table;
    GameObject turn_table_circle;

    public RouteManager.Orientation destination_orientation;

    private void Start()
    {
        // must be a Gameobject for Start() Update() to run
        train_list = new List<GameObject>();
        city_board = new GameObject[board_width, board_height]; // zero out the negative tile coordinates
        turn_table = Instantiate(Turntable);
        turn_table.GetComponent<Turntable>().city = this;

        turn_table_circle = Instantiate(Turntable_Circle);
        destination_orientation = RouteManager.Orientation.None;
    }

    private void Update()
    {

    }

    public void set_destination_track(RouteManager.Orientation orientation)
    {
        destination_orientation = orientation;
    }

    public Vector3Int get_location()
    {
        return tilemap_position;
    }

    public List<GameObject> get_train_list()
    {
        return train_list;
    }

    public void add_train_to_list(GameObject train)
    {
        train_list.Add(train);
    }

    public void turn_turntable(GameObject train_object, RouteManager.Orientation orientation, bool depart_for_turntable=false)
    {
        Turntable t = turn_table.GetComponent<Turntable>();
        StartCoroutine(t.turn_turntable(train_object, orientation, depart_for_turntable));
    }

    public Station_Track add_train_to_station(GameObject train_object, RouteManager.Orientation orientation)
    {
        switch (orientation)
        {
            case RouteManager.Orientation.North:
                return South_Station.set_station_track(train_object);
            case RouteManager.Orientation.East:
                return West_Station.set_station_track(train_object);
            case RouteManager.Orientation.West:
                return East_Station.set_station_track(train_object);
            case RouteManager.Orientation.South:
                return North_Station.set_station_track(train_object);
            default:
                return null;
        }
    }

    public void remove_train_from_list(Train train)
    {
        // remove a train that has departed the city
        int id = train.get_id();
        int trains_removed = 0;
        for (int t = 0; t < train_list.Count; t++)
        {
            int train_id = train_list[t].GetComponent<Train>().get_id();
            if (train_id == id)
            {
                train_list.RemoveAt(t);
                trains_removed++;
            }
        }
        print("trains removed = " + trains_removed);
        //if (trains_removed != 1)
        //    throw new Exception("Incorrect number of trains removed :" + trains_removed);
    }


    public void set_location(Vector3Int position)
    {
        tilemap_position = position;
    }

    public void is_train_turn_on(bool state)
    {
        // hide or show trains depending on whether I'm in a shipyard view
        turn_table.SetActive(state);
        turn_table_circle.SetActive(state);
        foreach (GameObject train in train_list)
        {
            train.GetComponent<SpriteRenderer>().enabled = state;
        }
    }
}
