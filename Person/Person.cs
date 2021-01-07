using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Person : Simple_Moving_Object
{
    public Sprite egghead_sprite;
    public int rent;
    public int wealth;

    public GameObject boxcar_go;
    public bool is_board_boxcar = false;
    public bool is_enter_home = false;
    public bool is_exit_boxcar = false;
    public bool is_on_boxcar = false;
    public bool is_exit_home = false;
    public bool player_walk_hor = false;
    public bool arrived_at_room;
    public string location;

    public Vector2 thought_bubble_offset;
    public bool is_egghead_thinking;
    public GameObject eggheads_thought_bubble; // default to home
    public string desired_activity;
    public GameObject thought_bubble;
    public Sprite restaurant_thought_bubble;
    public Sprite home_thought_bubble;
    public Sprite work_thought_bubble;
    public Sprite vacation_thought_bubble;
    public SpriteRenderer thought_bubble_renderer;
    public Dictionary<string, int> activity_duration_map;
    public Dictionary<string, int> activity_likelihood_map;

    public float desire_timeout; // time until person gives up on activity. results in a 0 star review
    public float boarding_duration; // time person has to wait to board train
    public float trip_duration; // except for vacations, the longer this is, the lower the review
    public float board_start_time;
    public float trip_start_time;

    public Room room;

    public RouteManager.Orientation enter_home_orientation;

    public enum Review
    {
        No_Star,
        One_Star,
        Two_Star,
        Three_Star,
        Four_Star,
        Five_Star,
    }

    public void Awake()
    {
        thought_bubble_offset = new Vector2(2.6f, 2.6f);
        base.Awake();
    }

    public void Start()
    {
        wealth = 0;
        desire_timeout = 30; 
        in_tile = true;
        arrived_at_room = true;
        is_egghead_thinking = true;
        location = "station"; // person is spawned at station
        enter_home_orientation = RouteManager.Orientation.None; // initialized on enter home sequence
        final_dest_tile_pos = new Vector3Int(-1, -1, 0);
        thought_bubble = Instantiate(eggheads_thought_bubble);
        thought_bubble_renderer = thought_bubble.GetComponent<SpriteRenderer>();
        desired_activity = thought_bubble.GetComponent<SpriteRenderer>().sprite.name;
        thought_bubble.transform.parent = gameObject.transform;
        thought_bubble.transform.localPosition = thought_bubble_offset;
        board_start_time = Time.time;
        gameObject.SetActive(false);
        thought_bubble.SetActive(true);
    }

    public void initialize_egghead()
    {

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

    public void finish_trip(City city)
    {
        trip_duration = Time.time - trip_start_time;
        leave_review(city);
    }

    public void board_train()
    {
        // call from PersonRouteMan.board_train()
        boarding_duration = Time.time - board_start_time;
        trip_start_time = Time.time;
    }

    public void leave_review(City city, Review review)
    {
        int delta_review = review - Review.Three_Star;
        city.change_reputation(delta_review);
        PersonManager.change_reputation(delta_review);
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

    public void activiate_thought_bubble()
    {
        eggheads_thought_bubble.SetActive(is_egghead_thinking);
    }

    public void leave_review(City city)
    {
        string destination_name = city.gameObject.name;
        Review review = 0;
        // if destination is incorrect, leave the lowest review
        if (destination_name == "Diner" || destination_name == "Restaurant")
            if (desired_activity != "restaurant_thought_bubble") review = Review.One_Star;
        if (destination_name == "Factory")
        {
            if (desired_activity != "work_thought_bubble") review = Review.One_Star;
        }
        if (destination_name == "Poor" || destination_name == "Wealthy")
        {
            if (desired_activity != "home_thought_bubble") review = Review.One_Star;
        }
        if (review == Review.One_Star)
        {
            leave_review(city, review);
            return;
        }
        float trip_rating = 1.0f - (boarding_duration + trip_duration) / desire_timeout / 2; // One minus the average rating
        int star_rating = Math.Max((int)(trip_rating * 5), 1);
        star_rating = Math.Min(5, star_rating);
        review = (Review)star_rating;
        print("FINISHED TRIP. Board duration was " + boarding_duration + " trip duration was " + trip_duration + "review was " + review);
        leave_review(city, review);
    }

    public bool is_boxcar_match_desired_activity(string boxcar_type)
    {
        if (boxcar_type == "food boxcar" && desired_activity == "restaurant_thought_bubble"
            || boxcar_type == "home boxcar" && desired_activity == "home_thought_bubble"
            || boxcar_type == "work boxcar" && desired_activity == "work_thought_bubble"
            || boxcar_type == "vacation boxcar" && desired_activity == "vacation_thought_bubble")
        {
            print("boxcar type " + boxcar_type + " matches desired activity " + desired_activity);
            return true;
        }            
        else
        {
            print("boxcar type " + boxcar_type + " doesn't match desired activity " + desired_activity);
            return false;
        }
    }

    public void render_thought_bubble()
    {
        eggheads_thought_bubble.SetActive(true);
        switch (desired_activity)
        {
            case "work_thought_bubble":
                thought_bubble_renderer.sprite = work_thought_bubble;
                break;
            case "home_thought_bubble":
                thought_bubble_renderer.sprite = home_thought_bubble;
                break;
            case "restaurant_thought_bubble":
                thought_bubble_renderer.sprite = restaurant_thought_bubble;
                break;
            case "vacation_thought_bubble":
                thought_bubble_renderer.sprite = vacation_thought_bubble;
                break;
            default:
                throw new Exception("not a valid thought bubble");
        }
    }

    public IEnumerator schedule_activity()
    {
        // when a person has arrived at a destination, perform action for a specified time
        thought_bubble.SetActive(false);
        int duration = activity_duration_map[desired_activity];
        yield return new WaitForSeconds(duration);
        desired_activity = pick_next_activity(); // when activity is over pick next activity
        render_thought_bubble();
        thought_bubble.SetActive(true);
    }

    public string pick_next_activity()
    {
        int rand_num = UnityEngine.Random.Range(0, 100);
        int num_counter = 0;
        foreach (KeyValuePair<string, int> entry in activity_likelihood_map)
        {
            num_counter += entry.Value;
            if (rand_num < num_counter) return entry.Key;
        }
        throw new Exception("couldnt pick next activity");
    }
}
