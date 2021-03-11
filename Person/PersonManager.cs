using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PersonManager : MonoBehaviour
{
    public static int reputation = 0; // review score out of 100
    public static int max_reputation = 100;
    public static int min_reputation = 0;
    public static GameObject notification_canvas;
    public static Tilemap city_tilemap;
    public static Notification[,] notification_matrix;
    public static GameObject notification_prefab;
    public static string[] men_name_arr;
    public static string[] women_name_arr;
    public static int total_notification; 
    public GameObject poor_person_go;
    public GameObject rich_person_go;
    public Texture zero_star_texture;
    public Texture one_star_texture;
    public Texture two_star_texture;
    public Texture three_star_texture;
    public Texture four_star_texture;
    public Texture five_star_texture;
    public int how_to_become_rich; // below this levele, a rich man becomes a poor man
    public int how_to_become_poor;
    public Sprite restaurant_thought_bubble;
    public Sprite home_thought_bubble;
    public Sprite work_thought_bubble;
    public Sprite vacation_thought_bubble;
    public Sprite small_restaurant_thought_bubble;
    public Sprite small_home_thought_bubble;
    public Sprite small_work_thought_bubble;
    public Sprite small_vacation_thought_bubble;
    public int people_count;

    public class Notification
    {
        public int notification_count = 0;
        public GameObject notification_go;
        Text notification_count_ui;
        public Vector3 offset = new Vector3(.4f, .4f);

        public void add_notification()
        {
            notification_count += 1;
            total_notification += 1;
            notification_count_ui.text = notification_count.ToString();
            notification_go.SetActive(true);
        }

        public void subtract_notification()
        {
            notification_count -= 1;
            total_notification -= 1;
            notification_count_ui.text = notification_count.ToString();
            if (notification_count == 0) hide_notification();
        }

        public void hide_notification()
        {
            notification_go.SetActive(false);
        }

        public Notification(Vector3Int city_pos)
        {
            notification_go = Instantiate(notification_prefab);
            notification_go.transform.parent = notification_canvas.transform;

            notification_count_ui = notification_go.GetComponentInChildren<Text>();
            Vector3 offset_notification_pos = city_tilemap.GetCellCenterWorld(city_pos) + offset;
            notification_go.transform.position = offset_notification_pos;
            notification_go.transform.localScale = new Vector3(.02f, .02f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        people_count = 0;
        how_to_become_poor = 100;
        how_to_become_rich = 300;
        total_notification = 0;
        notification_prefab = (GameObject)Resources.Load("UI/notification"); // note: not .prefab!
        notification_canvas = GameObject.Find("People Manager");
        city_tilemap = GameObject.Find("Structure").GetComponent<Tilemap>();
        notification_matrix = new Notification[BoardManager.board_width, BoardManager.board_height];
        men_name_arr = new string[] { "Kyler", "Matt", "Lamar", "Josh", "Teddy", "Mitchell", "Brandon", "Baker", "Andy", "Drew", "Matthew", "Aaron", "Deshaun", "Philip", "Mike", "Chad", "Derek", "Justin", "Jared", "Tua", "Kirk", "Cam", "Drew", "Daniel", "Sam", "Jalen", "Mason", "C.J", "Russel", "Tom", "Ryan", "Alex" };
        women_name_arr = new string[] { "Lisa", "Veronica", "Karen", "Vicky", "Cookie", "Tonya", "Diane", "Lori", "Carla", "Marina", "Selena", "Katrina", "Sabrina", "Kim", "LaToya", "Tina", "Shelley", "Bridget", "Cathy", "Rasheeda", "Kelly", "Nicole", "Angel", "Juanita", "Stacy", "Tracie", "Rohna", "Ronda", "Donna", "Ulanda", "Tawana", "Wanda" };
    }

    // Update is called once per frame
    void Update()
    {
        if (total_notification == 0) // override wait period if no notifications avilable
        {
            if (CityManager.Activated_City_Component != CityManager.home_base) // dont spawn if in entnrace, or else it will look weird
            {
                CityManager.home_base.populate_entrance();
            }
        }
    }

    //public Vector2 building_lot_to_thought_offset(BuildingLot bldg_lot)
    //{
    //    Vector2 thought_bubble_offset = new Vector2(0, 0);
    //    switch (bldg_lot.orientation)
    //    {
    //        case RouteManager.Orientation.North:
    //            thought_bubble_offset = new Vector2(3.6f, 0);
    //            break;
    //        case RouteManager.Orientation.East:
    //            thought_bubble_offset = new Vector2(2.6f, 2.6f);
    //            break;
    //        case RouteManager.Orientation.South:
    //            thought_bubble_offset = new Vector2(-3.6f, 2.6f);
    //            break;
    //        case RouteManager.Orientation.West:
    //            thought_bubble_offset = new Vector2(2.6f, 2.6f);
    //            break;
    //    }
    //    return thought_bubble_offset;
    //}

    public Sprite desired_activity_to_throught_sprite(string desired_activity, BuildingLot bldg_lot)
    {
        if (desired_activity == "work_thought_bubble")
            if (bldg_lot.orientation == RouteManager.Orientation.West || bldg_lot.orientation == RouteManager.Orientation.East)
                return work_thought_bubble;
            else { return small_work_thought_bubble; }
        if (desired_activity == "home_thought_bubble")
            if (bldg_lot.orientation == RouteManager.Orientation.West || bldg_lot.orientation == RouteManager.Orientation.East)
                return home_thought_bubble;
            else { return small_home_thought_bubble;  }
        if (desired_activity == "restaurant_thought_bubble")
            if (bldg_lot.orientation == RouteManager.Orientation.West || bldg_lot.orientation == RouteManager.Orientation.East)
                return restaurant_thought_bubble;
            else { return small_restaurant_thought_bubble;  }
        if (desired_activity == "vacation_thought_bubble")
            if (bldg_lot.orientation == RouteManager.Orientation.West || bldg_lot.orientation == RouteManager.Orientation.East)
                return vacation_thought_bubble;
            else { return small_vacation_thought_bubble;  }
        throw new Exception("no sprite matches bubble");
    }


    public void reinitialize_person(GameObject person_go_instance, GameObject old_person_go_instance)
    {
        // update person sprite after changing wealth class
        Person person = person_go_instance.GetComponent<Person>();
        Person old_person = old_person_go_instance.GetComponent<Person>();
        person.schedule_activity();
        bool is_egghead_on = old_person_go_instance.GetComponent<SpriteRenderer>().enabled;

        person_go_instance.GetComponent<SpriteRenderer>().enabled = is_egghead_on;
        if (old_person.room != null)
        {
            person.room = old_person.room;
            person.name = old_person.name;
            person.room.person_go_instance = person_go_instance;
        } else
        {
            throw new Exception("person should ben in a room when becomes rich");
        }
        person_go_instance.transform.position = old_person_go_instance.transform.position;
        person.initialize_egghead(is_egghead_on, is_egghead_on);
        person.wealth = old_person_go_instance.GetComponent<Person>().wealth;
        Destroy(old_person_go_instance);
    }

    public void change_wealth(GameObject person_go, int delta_wealth)
    {
        if (person_go.GetComponent<Person>().wealth + delta_wealth < 0)
        {
            GameManager.update_game_money_text(delta_wealth);
            return;
        }
        person_go.GetComponent<Person>().wealth += delta_wealth;
        int wealth = person_go.GetComponent<Person>().wealth;
        if (person_go.tag == "poor")
        {
            //print("wealth is " + wealth);
            if (wealth >= how_to_become_rich)
            {
                //print("HOW TO BECOME RICH");
                GameObject rich_person = Instantiate(rich_person_go);
                reinitialize_person(rich_person, person_go);
            }
        }
        else
        {
            if (wealth <= how_to_become_poor)
            {
                //print("HOW TO BECOME POOR");
                GameObject poor_person = Instantiate(poor_person_go);
                reinitialize_person(poor_person, person_go);
            }
        }
    }

    public GameObject create_person(bool is_person_poor)
    {
        people_count += 1;
        GameObject person_go_instance;
        bool sex_is_male = is_sex_male();
        if (is_person_poor) // women or men
        {
            person_go_instance = Instantiate(poor_person_go);
        }
        else
        {
            person_go_instance = Instantiate(rich_person_go);
        }
        string name = get_random_name(sex_is_male);
        person_go_instance.GetComponent<Person>().name = name;
        return person_go_instance;
    }

    public static bool is_sex_male()
    {
        // true is male, false is female
        int rand_int = UnityEngine.Random.Range(0, 1);
        if (rand_int == 0) return true;
        else { return false; }
    }

    public Texture get_star_texture(int texture_id)
    {
        switch (texture_id)
        {
            case 0:
                return zero_star_texture;
            case 1:
                return one_star_texture;
            case 2:
                return two_star_texture;
            case 3:
                return three_star_texture;
            case 4:
                return four_star_texture;
            case 5:
                return five_star_texture;
            default:
                throw new Exception("not a valid texture: " + texture_id);
        }
    }

    public static string get_random_name(bool sex_is_male)
    {
        if (sex_is_male)
        {
            return men_name_arr[UnityEngine.Random.Range(0, men_name_arr.Length-1)];
        }
        else
        {
            return women_name_arr[UnityEngine.Random.Range(0, women_name_arr.Length - 1)];
        }
    }

    public static void show_notification(bool is_shown)
    {
        if (is_shown) notification_canvas.SetActive(true);
        else { notification_canvas.SetActive(false); }
    }

    public static void change_reputation(int reputation_change)
    {
        reputation += reputation_change;
        reputation = Mathf.Min(reputation, max_reputation);
        reputation = Mathf.Max(reputation, min_reputation);
        //print("reputation of all cities is " + reputation);
    }

    public static void add_notification_for_city(Vector3Int city_tile_position, bool is_notification)
    {
        Notification notification = notification_matrix[city_tile_position.x, city_tile_position.y];
        if (notification == null)
        {
            notification = new Notification(city_tile_position);
            notification_matrix[city_tile_position.x, city_tile_position.y] = notification;
        }
        if (is_notification) notification.add_notification();
        else { notification.subtract_notification(); }
    }
}
