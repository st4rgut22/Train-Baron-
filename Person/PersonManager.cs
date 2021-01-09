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

    public class Notification
    {
        public int notification_count = 0;
        public GameObject notification_go;
        Text notification_count_ui;
        public Vector3 offset = new Vector3(.4f, .4f);

        public void add_notification()
        {
            notification_count += 1;
            notification_count_ui.text = notification_count.ToString();
            notification_go.SetActive(true);
        }

        public void subtract_notification()
        {
            notification_count -= 1;
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
            notification_count_ui = notification_go.GetComponentInChildren<Text>();
            notification_go.transform.parent = notification_canvas.transform;
            Vector3 offset_notification_pos = city_tilemap.GetCellCenterWorld(city_pos) + offset;
            notification_go.transform.position = GameManager.camera.WorldToScreenPoint(offset_notification_pos);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        notification_prefab = (GameObject)Resources.Load("UI/notification"); // note: not .prefab!
        notification_canvas = GameObject.Find("Notification Canvas");
        city_tilemap = GameObject.Find("Structure").GetComponent<Tilemap>();
        notification_matrix = new Notification[BoardManager.board_width, BoardManager.board_height];
    }

    // Update is called once per frame
    void Update()
    {

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
        print("reputation of all cities is " + reputation);
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
