using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TrainMenuManager : MonoBehaviour
{

    GameObject train_menu;
    public Button close_btn;
    public GameObject ne_less_steep;
    public GameObject ne_steep;
    public GameObject nw_less_steep;
    public GameObject nw_steep;
    public GameObject se_less_steep;
    public GameObject se_steep;
    public GameObject sw_less_steep;
    public GameObject sw_steep;
    GameObject city_object;

    private void Awake()
    {
        train_menu = GameObject.Find("Exit Bck"); // just a blue background
    }

    // Start is called before the first frame update
    void Start()
    {
        close_btn.onClick.AddListener(hide_menu);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject get_city_object()
    {
        return city_object;
    }

    public void hide_menu()
    {
        // exiting the city view
        gameObject.SetActive(false);        
        GameManager.game_menu_state = true;
    }

    public void destroy_train_display()
    {
        // before creating a new display, remove everything from previous display
        foreach (Transform child in transform)
        {
            if (child.tag == "display")
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void zero_margins(RectTransform rectTransform)
    {
        // anchors are set correctly, but margins are off. zero all margins to match anchor location
        RectTransformExtensions.SetLeft(rectTransform, 0);
        RectTransformExtensions.SetBottom(rectTransform, 0);
        RectTransformExtensions.SetRight(rectTransform, 0);
        RectTransformExtensions.SetTop(rectTransform, 0);
    }

    public GameObject get_train_display(RouteManager.Orientation orientation)
    {
        GameObject train_display;
        //print("orientation is " + orientation);
        switch (orientation)
        {
            case RouteManager.Orientation.ne_SteepCurve:
                train_display = Instantiate(ne_steep);
                break;
            case RouteManager.Orientation.ne_LessSteepCurve:
                train_display = Instantiate(ne_less_steep);
                break;
            case RouteManager.Orientation.nw_SteepCurve:
                train_display = Instantiate(nw_steep);
                break;
            case RouteManager.Orientation.nw_LessSteepCurve:
                train_display = Instantiate(nw_less_steep);
                break;
            case RouteManager.Orientation.se_LessSteepCurve:
                train_display = Instantiate(se_less_steep);
                break;
            case RouteManager.Orientation.se_SteepCurve:
                train_display = Instantiate(se_steep);
                break;
            case RouteManager.Orientation.sw_SteepCurve:
                train_display = Instantiate(sw_steep);
                break;
            case RouteManager.Orientation.sw_LessSteepCurve:
                train_display = Instantiate(sw_less_steep);
                break;
            default:
                throw new Exception("should be a steep angle unless the train has departed, in which case it is already removed from queue");
        }
        return train_display;
    }

    public RectTransform create_train_display(GameObject train_object)
    {
        // instantiate new train display and assign it its train
        Train train = train_object.GetComponent<Train>();
        RouteManager.Orientation orientation = train.steep_angle_orientation;
        GameObject train_display = get_train_display(orientation);
        train_display.transform.SetParent(train_menu.transform);
        RectTransform rectTransform = train_display.GetComponent<RectTransform>();
        return rectTransform;
    }

    public void destroy_old_train_menu()
    {
        foreach (Transform t in train_menu.transform)
        {
            Destroy(t.gameObject);
        }
    }

    public void update_train_menu(City activated_city)
    {
        Queue<GameObject> train_queue = activated_city.turn_table.GetComponent<Turntable>().train_queue;
        List<GameObject> train_list = new List<GameObject>(train_queue); // cast queue to list to iterate over all elements
        float padding = .005f;
        float total_padding = 0;
        float offset_x = 0;
        float display_width = .12f;
        destroy_old_train_menu();
        for (int i = 0; i < train_list.Count; i++)
        {
            total_padding += padding; // padding between display items
            offset_x = i * display_width + total_padding;
            try
            {
                RectTransform rectTransform = create_train_display(train_list[i]);
                rectTransform.anchorMin = new Vector2(offset_x, 0f); // bottom left of parent transform
                rectTransform.anchorMax = new Vector2(offset_x + display_width, 1); // top right of parent transform
                zero_margins(rectTransform);
                rectTransform.localScale = new Vector2(1, 1); // scale ui to match anchors
                rectTransform.anchoredPosition = Vector2.zero; //move ui to anchors
            }
            catch (System.NullReferenceException e)
            {
                print("train has already departed, dont add train display");
            }
        }
    }
}
