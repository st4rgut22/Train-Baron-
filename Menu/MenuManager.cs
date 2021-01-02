using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class MenuManager : MonoBehaviour
{
    public Button store_btn;
    public Button shipyard_exit_btn;

    protected StoreMenuManager store_menu_manager;
    protected GameMenuManager game_menu_manager;
    public static GameObject store_menu;
    public static GameObject game_menu;
    public static GameObject shipyard_exit_menu;
    public static GameObject game_icon_canvas;
    static List<GameObject> event_handler_list; // names of gameobjects that listen for events
    City city;

    static Camera camera;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        store_menu = GameObject.Find("Store Menu");
        game_menu = GameObject.Find("Game Menu");
        shipyard_exit_menu = GameObject.Find("Exit Bar");
        game_icon_canvas = GameObject.Find("Iconic Canvas");
        store_menu_manager = store_menu.GetComponent<StoreMenuManager>();
        game_menu_manager = game_menu.GetComponent<GameMenuManager>();
        camera = GameObject.Find("Camera").GetComponent<Camera>();
        event_handler_list = new List<GameObject>();
        event_handler_list.Add(store_menu);
        event_handler_list.Add(game_menu);
        event_handler_list.Add(shipyard_exit_menu);
        event_handler_list.Add(game_icon_canvas);
        activate_default_handler();
        store_btn.onClick.AddListener(delegate { activate_handler(new List<GameObject> { store_menu }); });
        shipyard_exit_btn.onClick.AddListener(turn_off_shipyard);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void turn_off_shipyard()
    {
        GameManager.city_manager.set_activated_city(); // hide all the trains in the city
        GameObject.Find("GameManager").GetComponent<GameManager>().switch_on_shipyard(false);
        activate_default_handler();
    }

    public static void activate_default_handler()
    {
        //activates handlers for game screen
        activate_handler(new List<GameObject> { game_menu, game_icon_canvas });
    }

    public static void activate_handler(List<GameObject> menu)
    {
        //open one menu, set listeners from all other screens off
        //is_open stands for activating a screen versus closing the active one
        if (menu[0] == game_menu) GameManager.game_menu_state = true;
        if (menu[0] == store_menu)
            GameManager.Structure.GetComponent<TilemapCollider2D>().enabled = false;
        else
        {
            GameManager.Structure.GetComponent<TilemapCollider2D>().enabled = true;
        }
        //else { GameManager.game_menu_state = true; }
        foreach (GameObject handler in event_handler_list)
        {
            if (menu.Contains(handler)) {
                handler.SetActive(true);
            }
            else { handler.SetActive(false); }
        }
    }

    public static Vector3 convert_screen_to_world_coord(Vector3 position)
    {
        Vector3 world_position = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, Mathf.Abs(camera.transform.position.z)));
        position.z = 0;
        return world_position;
    }

    public static void zero_margins(RectTransform rectTransform)
    {
        // anchors are set correctly, but margins are off. zero all margins to match anchor location
        RectTransformExtensions.SetLeft(rectTransform, 0);
        RectTransformExtensions.SetBottom(rectTransform, 0);
        RectTransformExtensions.SetRight(rectTransform, 0);
        RectTransformExtensions.SetTop(rectTransform, 0);
    }
}
