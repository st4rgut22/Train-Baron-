using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainMenuManager : MonoBehaviour
{

    GameObject train_menu;
    public GameObject train_graphic;
    public Button close_btn;

    private void Awake()
    {
        train_menu = GameObject.Find("Train Menu"); // just a blue background
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

    public void hide_menu()
    {
        gameObject.SetActive(false);
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

    public RectTransform create_train_display(GameObject train_object, City city)
    {
        // instantiate new train display and assign it its train
        Train train = train_object.GetComponent<Train>();
        GameObject train_display = Instantiate(train_graphic);
        TrainDisplay display = train_display.GetComponent<TrainDisplay>();
        display.set_city(city);
        display.set_train(train);
        int boxcar_count = train.get_boxcar_id();
        display.initialize_boxcar_text(boxcar_count);
        train_display.transform.parent = train_menu.transform;
        RectTransform rectTransform = train_display.GetComponent<RectTransform>();
        return rectTransform;
    }

    public void create_train_menu(GameObject city_object)
    {
        //TODO: call in coroutine to update menu as trains arrive
        City city = city_object.GetComponent<City>(); // update city
        List<GameObject> train_list = city.get_train_list();
        Vector3 train_display_position = new Vector3(0, 0, 0);
        float padding = .01f;
        float total_padding = 0;
        float offset_x = 0;
        float display_width = .237f;
        for (int i = 0; i < train_list.Count; i++)
        {
            total_padding += padding; // padding between display items
            offset_x = i * display_width + total_padding;
            RectTransform rectTransform = create_train_display(train_list[i], city);
            rectTransform.anchorMin = new Vector2(offset_x, .01f); // bottom left
            rectTransform.anchorMax = new Vector2(offset_x + display_width, .11f); // top right
            zero_margins(rectTransform);
            rectTransform.localScale = new Vector2(1, 1); // scale ui to match anchors
            rectTransform.anchoredPosition = Vector2.zero; //move ui to anchors
        }
    }
}
