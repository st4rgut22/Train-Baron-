using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Person: Simple_Moving_Object
{
    public Sprite egghead_sprite;
    public int rent;
    public int wealth;

    public GameObject boxcar_go;
    public bool is_board_boxcar=false;
    public bool is_enter_home = false;
    public bool is_exit_boxcar=false;
    public bool is_on_boxcar=false;
    public bool is_exit_home=false;
    public bool player_walk_hor = false;
    public string location;

    public Vector2 thought_bubble_offset; 
    public GameObject eggheads_thought_bubble; // default to home
    public GameObject restaurant_thought_bubble;
    public Dictionary<string, int> activity_duration_map;
    public Dictionary<string, int> activity_likelihood_map;

    public Room room;

    public RouteManager.Orientation enter_home_orientation;

    public enum Review
    {
        Five_Star,
        Four_Star,
        Three_Star,
        Two_Star,
        One_Star
    }

    private void Awake()
    {
        base.Awake();
        location = "station"; // person is spawned at station
        thought_bubble_offset = new Vector2(.2f, .2f);
    }

    public void Start()
    {
        wealth = 0;
        in_tile = true;
        enter_home_orientation = RouteManager.Orientation.None; // initialized on enter home sequence
        final_dest_tile_pos = new Vector3Int(-1, -1, 0);
        gameObject.SetActive(false);
        restaurant_thought_bubble = Instantiate(eggheads_thought_bubble);
        restaurant_thought_bubble.transform.position = (Vector2)transform.position + thought_bubble_offset;
        restaurant_thought_bubble.transform.parent = gameObject.transform;
    }

    public void Update()
    {
        if (is_exit_home || is_exit_boxcar) // execute followo track after person has completed exit home sequence
            base.Update(); // follow track coroutine
        if (final_destination_reached)
        {
            if (is_board_boxcar) // step on boxcar sequence
            {
                is_board_boxcar = false;
                is_on_boxcar = true;
                is_exit_home = false; //no longer follow the track
                if (boxcar_go == null) throw new System.Exception("boxcar should be assigned to person when person exited home");
                StartCoroutine(GameObject.Find("PersonRouteManager").GetComponent<PersonRouteManager>().step_on_boxcar(gameObject, boxcar_go));
            }
            else if (is_enter_home)
            {
                is_on_boxcar = false;
                is_enter_home = false; 
                is_exit_boxcar = false; //no longer follow the track
                StartCoroutine(GameObject.Find("PersonRouteManager").GetComponent<PersonRouteManager>().enter_home(gameObject, room));
            }
            final_destination_reached = false;
        }
    }

    public void leave_review(Review review)
    {
        switch (review)
        {
            case Review.Five_Star:
                PersonManager.change_reputation(2);
                break;
            case Review.Four_Star:
                PersonManager.change_reputation(1);
                break;
            case Review.Two_Star:
                PersonManager.change_reputation(-1);
                break;
            case Review.One_Star:
                PersonManager.change_reputation(-2);
                break;
        }
    }

    public void exit_home()
    {
        is_exit_home = true;
        room.clear_room();
        room = null;
    }

    public void set_orientation(RouteManager.Orientation orientation)
    {
        this.orientation = orientation;
        //set_initial_rotation(orientation);
    }

    public void set_tile_pos(Vector2Int update_tile_pos)
    {
        tile_position = (Vector3Int)update_tile_pos;
    } 

    public void pay_rent()
    {
        change_wealth(-rent);
    }


    public bool change_wealth(int delta)
    {
        if (wealth + delta >= 0)
        {
            wealth += delta;
            return true;
        } else
        {
            return false;
        }
    }
}
