using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Person : Simple_Moving_Object
{
    public Sprite egghead_sprite;
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
    public GameObject eggheads_thought_bubble; // TODOED default to home
    public string desired_activity;
    public GameObject thought_bubble;

    public SpriteRenderer thought_bubble_renderer;
    public Dictionary<string, int> activity_duration_map;
    public Dictionary<string, int> ticket_cost_map;
    public Dictionary<string, int> activity_likelihood_map;

    public float board_desire_timeout; // time until person gives up on activity. results in a 0 star review
    public float trip_desire_timeout; 
    public float boarding_duration; // time person has to wait to board train
    public float trip_duration; // except for vacations, the longer this is, the lower the review
    public float board_start_time;
    public float trip_start_time;

    public bool trip_in_progress;
    public bool activity_in_progress;
    public bool is_selected; // selected to board a boxcar

    public int poor_wage;
    public int rich_wage;

    public Room room;

    public string name;

    public RouteManager.Orientation enter_home_orientation;

    public enum Review
    {
        Zero_Star,
        One_Star,
        Two_Star,
        Three_Star,
        Four_Star,
        Five_Star,
    }

    public void Awake()
    {
        thought_bubble_offset = new Vector2(2.6f, 2.6f);
        trip_in_progress = false;
        activity_in_progress = false;
        base.Awake();
        is_selected = false;
        poor_wage = 100;
        rich_wage = 1000;
    }

    public void Start()
    {
        board_desire_timeout = 90;
        trip_desire_timeout = 260;
        in_tile = true;
        arrived_at_room = true;
        is_egghead_thinking = true;
        location = "station"; // person is spawned at station
        enter_home_orientation = RouteManager.Orientation.None; // initialized on enter home sequence
        final_dest_tile_pos = new Vector3Int(-1, -1, 0);
        board_start_time = Time.time;
    }

    public void pop_thought_bubble()
    {
        thought_bubble.SetActive(false);
        PersonManager.add_notification_for_city(room.building.city.tilemap_position, false);
    }

    public void initialize_egghead(bool is_egghead_on, bool is_bubble_on)
    {
        if (gameObject.tag == "poor")
            wealth = 50;
        else
        {
            wealth = 200;
        }
        gameObject.GetComponent<SpriteRenderer>().enabled = is_egghead_on;
        thought_bubble = Instantiate(eggheads_thought_bubble);
        thought_bubble_renderer = thought_bubble.GetComponent<SpriteRenderer>();
        desired_activity = thought_bubble.GetComponent<SpriteRenderer>().sprite.name;
        print("desired activity is " + desired_activity);
        thought_bubble.transform.parent = gameObject.transform;
        thought_bubble.transform.localPosition = thought_bubble_offset; //GameManager.person_manager.GetComponent<PersonManager>().building_lot_to_thought_offset(room.building.building_lot);
        thought_bubble.GetComponent<SpriteRenderer>().enabled = is_bubble_on;
        PersonManager.add_notification_for_city(room.building.city.tilemap_position, true);
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
        boarding_duration = Time.time - board_start_time;
        if (boarding_duration >= board_desire_timeout && !trip_in_progress && !activity_in_progress && !is_selected) // waiting for a train
        {
            leave_review(room.building.city, Review.Zero_Star); // worst review is if person is not picked up
            if (room.building.city.city_type != "Entrance") // if city is entrance, then person keeps desiring to go home
            {
                pop_thought_bubble();
                StartCoroutine(schedule_activity());
            }
            else
            {
                board_start_time = Time.time; // reset timer until leave another bad review due to timeout
            }
        }
    }

    public void finish_trip()
    {
        trip_duration = Time.time - trip_start_time;
        leave_review(room.building.city);
    }

    public void board_train()
    {
        // call from PersonRouteMan.board_train()
        trip_in_progress = true;
        boarding_duration = Time.time - board_start_time;
        trip_start_time = Time.time;
    }

    public void leave_review(City city, Review review)
    {
        int reputation_change = (int)review;
        if (review == Review.Zero_Star) reputation_change = -3;
        if (review == Review.One_Star) reputation_change = -2;
        if (review == Review.Two_Star) reputation_change = -1;
        print("LEFT REVIEW " + review);
        city.change_star_count((int)review);
        city.change_reputation(reputation_change);
        PersonManager.change_reputation(reputation_change);
    }

    public void exit_home()
    {
        is_exit_home = true;
        //room.clear_room();
        //room = null;
    }

    public void set_orientation(RouteManager.Orientation orientation)
    {
        this.orientation = orientation;
    }

    public void set_tile_pos(Vector2Int update_tile_pos)
    {
        tile_position = (Vector3Int)update_tile_pos;
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
        //eggheads_thought_bubble.SetActive(is_egghead_thinking);
    }

    public string get_desired_activity_text()
    {
        switch (desired_activity)
        {
            case "restaurant_thought_bubble":
                return "eat";
            case "work_thought_bubble":
                return "work";
            case "home_thought_bubble":
                return "relax at home";
            default:
                throw new Exception("not a valid activity");
        }
    }

    public string review_board_trip_time(int rating)
    {
        string critique = "";
        switch (rating)
        {
            case 0:
                critique = "extremely slow";
                break;
            case 1:
                critique = "very slow";
                break;
            case 2:
                critique = "slow";
                break;
            case 3:
                critique = "decent";
                break;
            case 4:
                critique = "fast";
                break;
            case 5:
                critique = "insanely fast";
                break;
        }
        return critique;
    }

    public string is_class_insensitive()
    {
        if (gameObject.tag == "rich")
        {
            string city_name = city.city_building_list[0].name;
            if (city_name.Contains("Apartment"))
            {
                return "I expected to return to my mansion but got dumped in the slums.";
            }
            if (city_name.Contains("Factory"))
            {
                return "Terrible day at the mines today. I never want to labor again.";
            }
            if (city_name.Contains("Diner"))
            {
                return "Did they scrape my dinner from a dumpster? I only frequent 5 Star restaurants.";
            }
        }
        if (gameObject.tag == "poor")
        {
            string city_name = city.city_building_list[0].name;
            if (city_name.Contains("Restaurant"))
            {
                return "The food was great, but it's way too expensive.";
            }
        }
        return "";
    }

    public void leave_review(City city)
    {
        string destination_name = city.gameObject.name;
        Review review = 0;
        string review_summary = "";
        // if destination is incorrect, leave the lowest review
        if (city == prev_city) // person did not go anywhere
        {
            review = Review.Zero_Star;
        }
        else if (desired_activity != "work_thought_bubble" && (destination_name == "Factory" || destination_name == "Business") ||
            desired_activity != "home_thought_bubble" && (destination_name == "Apartment" || destination_name == "Mansion") ||
            desired_activity != "restaurant_thought_bubble" && (destination_name == "Diner" || destination_name == "Restaurant"))
        {
            review = Review.One_Star;
            review_summary = get_desired_activity_text();
        }
        else
        {
            float boarding_pause_duration = PauseManager.find_cumulative_pause(board_start_time);
            float trip_pause_duration = PauseManager.find_cumulative_pause(trip_start_time);
            float accurate_boarding_duration = boarding_duration - boarding_pause_duration;
            float accurate_trip_duration = trip_duration - trip_pause_duration;
            float boarding_rating = 1 - Math.Min(1, accurate_boarding_duration / board_desire_timeout);
            float trip_rating = 1 - Math.Min(1, accurate_trip_duration / trip_desire_timeout);
            float train_rating = (boarding_rating + trip_rating) / 2; // One minus the average rating of boarding and trip
            string trip_critique = review_board_trip_time((int)(trip_rating * 5));
            print("trip critique equals trip rating " + trip_rating + " times 5");
            string board_critique = review_board_trip_time((int)(boarding_rating * 5));
            print("board critique equals boarding rating " + boarding_rating + " times 5");
            review_summary = "The trip was " + trip_critique + " and boarding was " + board_critique;
            int star_rating = (int)(train_rating * 5) + 1;
            star_rating = Math.Min(5, star_rating);
            string class_critique = is_class_insensitive();
            if (class_critique != "") star_rating -= 1;
            review_summary = class_critique + review_summary;
            review = (Review)star_rating;
            print("boarding pause duration is " + boarding_pause_duration + " and trip pause duration is " + trip_pause_duration + "trip rating is " + train_rating);
            print("FINISHED TRIP. Board duration was " + accurate_boarding_duration + " trip duration was " + accurate_trip_duration + "review was " + review);
        }
        GameManager.convert_star_to_profit((int)review);
        leave_review(city, review);
        update_review_page(review_summary, (int)review);
    }

    public void update_review_page(string review_summary, int star_rating)
    {
        scrollScript.update_notification_count(1);
        GameManager.scroll_handler.GetComponent<scrollScript>().generateItem(review_summary, name, star_rating, city.name);
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
        thought_bubble.SetActive(true);
        // get thought bubble from PersonManager
        thought_bubble_renderer.sprite = GameManager.person_manager.GetComponent<PersonManager>().desired_activity_to_throught_sprite(desired_activity, room.building.building_lot);
    }

    public IEnumerator schedule_activity()
    {
        // when a person has arrived at a destination, perform action for a specified time        
        thought_bubble.SetActive(false);
        int duration = activity_duration_map[desired_activity];
        string save_desired_activity = desired_activity;
        desired_activity = ""; // so person cant board another matching boxcar while performing action
        trip_in_progress = false;
        activity_in_progress = true;
        yield return new WaitForSeconds(duration);
        if (save_desired_activity == "work_thought_bubble" && (room.building.name.Contains("Factory") || room.building.name.Contains("Business")))
        {
            if (gameObject.tag == "poor")
            {
                GameManager.person_manager.GetComponent<PersonManager>().change_wealth(gameObject, poor_wage);
                GameManager.update_game_money_text(poor_wage * 2); // profits you earn off your employee
            }                
            else
            {
                GameManager.person_manager.GetComponent<PersonManager>().change_wealth(gameObject, rich_wage);
                // you don't earn any profits off your rich employees (they work for themselves)
            }
        }
        else if (desired_activity == "restaurant_thought_bubble")
        {
            if (room.building.name.Contains("Restaurant"))
                GameManager.person_manager.GetComponent<PersonManager>().change_wealth(gameObject, CityManager.restaurant_cost);
            else if (room.building.name.Contains("Diner"))
                GameManager.person_manager.GetComponent<PersonManager>().change_wealth(gameObject, CityManager.diner_cost);
        }
        board_start_time = Time.time;
        activity_in_progress = false;
        if (room.building.city.city_type == "Entrance") desired_activity = "home_thought_bubble";
        else
        {
            desired_activity = pick_next_activity(); // when activity is over pick next activity
            print("next activity is " + desired_activity);
            bool is_city_same_as_activity = CityManager.does_activity_match_city(desired_activity, room.building.gameObject.name);
            print("is activity " + desired_activity + " same as " + room.building.gameObject.name + " " + is_city_same_as_activity);
            //if (is_city_same_as_activity) is_inventory POOPIES
            //{
            //    StartCoroutine(schedule_activity()); // if next activity is the same, dont need to board a train
            //    yield break; // dont render thought bubble if same action
            //}
        }
        PersonManager.add_notification_for_city(room.building.city.tilemap_position, true);
        render_thought_bubble();
    }

    public List<string> filter_activity_by_price()
    {
        List<string> feasible_activity = new List<string>();
        foreach (KeyValuePair<string, int> entry in activity_likelihood_map)
        {
            if (wealth > ticket_cost_map[entry.Key])
            {
                feasible_activity.Add(entry.Key);
            }
        }
        return feasible_activity;
    }

    public string pick_next_activity()
    {
        List<string> activity_list = filter_activity_by_price();
        if (activity_list.Count == 0)
        {
            GameManager.update_game_money_text(-ticket_cost_map["work_thought_bubble"]);
            print("STOLE FROM THE BOSS CUZ I GOT NO MONEY");
            return "work_thought_bubble";
        }
        else
        {
            int num_counter = 0;
            foreach (string activity in activity_list)
            {
                num_counter += activity_likelihood_map[activity];
            }
            int rand_num = UnityEngine.Random.Range(0, num_counter);
            int rand_num_score = 0;
            foreach (string activity in activity_list)
            {
                rand_num_score += activity_likelihood_map[activity];
                if (rand_num_score > rand_num)
                {
                    GameManager.person_manager.GetComponent<PersonManager>().change_wealth(gameObject, -ticket_cost_map[activity]);
                    //GameManager.update_game_money_text(ticket_cost_map[activity]);
                    return activity;
                }
            }
            throw new Exception("couldnt pick next activity");
        }
    }
}
