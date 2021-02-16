using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : EventDetector
{
    // Start is called before the first frame update
    // tutorial of a basic train route from shopping to unloading a person

    // 1. Click Entrance
    // 2. Enter Store
    // 3. Buy Goods
    // 4. Drag Structure and Track
    // 5. Click Entrance
    // 6. Drag Train and Boxcar
    // 7. Board Person
    // 8. Exit Entrance
    // 9. Enter Apartment
    // 10. Unload Person
    // 11. Observe Goal

    // 1159, 675 click store 
    // 127, 447 (Train)  419, 449 (Boxcar) 356, 240, Apartment (417, 29)
    //1276, 663 exit store
    //827, 66 -> 740, 380 (dragtrack) 401,44 -> 658,394 (dragapartment)
    //850, 362 (click entrance)
    //929,45 -> 804, 638 (drag train) 216, 49 -> 804, 638 (drag boxcar)
    // 883, 629 (click person) BOARDING
    // 847, 713 (click boxcar)
    // 812, 671 (CLICK train)
    // 338, 408 (CLICK west exit
    //1276, 663 exit city
    // 639, 394 enter apartment
    // 847, 713 (click boxcar)
    // 883, 629 (click room) UNLOADING

    public Button real_store_btn;
    public Button real_buy_btn;
    public Button real_exit_city_btn;
    public Button real_purchase_train_btn;
    public Button real_purchase_apartment_btn;
    public Button real_purchase_boxcar_btn;
    public Button real_purchase_track_btn;

    public GameObject click_store_btn;
    public GameObject purchase_train_btn;
    public GameObject purchase_boxcar_btn;
    public GameObject purchase_apartment_btn;
    public GameObject purchase_track_btn;
    public GameObject buy_btn;
    public GameObject start_drag_track;
    public GameObject end_drag_track;
    public GameObject start_drag_apartment;
    public GameObject end_drag_apartment;
    public GameObject click_entrance;
    public GameObject start_drag_train;
    public GameObject end_drag_train;
    public GameObject start_drag_boxcar;
    public GameObject end_drag_boxcar;
    public GameObject click_person;
    public GameObject board_boxcar;
    public GameObject click_train;
    public GameObject click_exit_west;
    public GameObject exit_city_btn;
    public GameObject click_apartment;
    public GameObject click_boxcar_to_unload;
    public GameObject click_room_to_unload;

    public GraphicRaycaster graphic_raycaster;

    public static int active_tutorial_step_idx;
    public Text instruction_text;

    public static Step[] tutorial_step_list;

    public class Step
    {
        public GameObject step_mask;
        public GameObject end_step_mask; // only needed for endDrag
        public string instruction_text;
        Vector3 action_position;

        public ActionType.Action action_type; // click, button_press, drag

        public Step(GameObject step_mask, string instruction, Vector3 pos, ActionType.Action action_type, GameObject end_step_mask=null)
        {
            this.step_mask = step_mask;
            this.instruction_text = instruction;
            action_position = pos;
            this.action_type = action_type;
            this.end_step_mask = end_step_mask;
        }
    }

    public static Step get_current_step()
    {
        return tutorial_step_list[active_tutorial_step_idx];
    }

    public static GameObject get_current_gameobject()
    {
        return get_current_step().step_mask;
    }

    public Button block_ui_to_real_btn(GameObject action_go)
    {
        if (action_go == click_store_btn) return real_store_btn;
        else if (action_go == purchase_train_btn) return real_purchase_train_btn;
        else if (action_go == purchase_boxcar_btn) return real_purchase_boxcar_btn;
        else if (action_go == purchase_track_btn) return real_purchase_track_btn;
        else if (action_go == buy_btn) return real_buy_btn;
        else if (action_go == exit_city_btn) return real_exit_city_btn;
        else if (action_go == purchase_apartment_btn) return real_purchase_apartment_btn;
        else { return null; }
    }

    public bool is_button_active(GameObject clicked_btn)
    {
        // check if button matches currently active one
        if (!GameManager.is_tutorial_mode) return true;
        GameObject step_go = get_current_step().step_mask;
        Button real_btn = block_ui_to_real_btn(step_go);
        if (real_btn != null && real_btn.gameObject == clicked_btn)
            return true;
        else { return false; }
    }

    public void check_if_button_click(GameObject action_go)
    {
        Button real_btn = block_ui_to_real_btn(action_go);
        if (real_btn != null)
        {
            MenuManager.is_btn_active = true;
            real_btn.onClick.Invoke();
        }
    }

    public bool did_raycast_hit_blocking_mask()
    {
        //Code to be place in a MonoBehaviour with a GraphicRaycaster component
        GraphicRaycaster gr = this.GetComponent<GraphicRaycaster>();
        //Create the PointerEventData with null for the EventSystem
        PointerEventData ped = new PointerEventData(null);
        //Set required parameters, in this case, mouse position
        ped.position = Input.mousePosition;
        //Create list to receive all results
        List<RaycastResult> results = new List<RaycastResult>();
        //Raycast it
        gr.Raycast(ped, results);
        print(results);
        if (results.Count == 0) return false;
        else
        {
            return true;
        }
    }

    void Start()
    {
        graphic_raycaster = GetComponent<GraphicRaycaster>();
        Step click_store_step = new Step(click_store_btn, "Click on the store to buy a train", new Vector3(1159, 675), ActionType.Action.BTN_PRESS);
        Step add_train_step = new Step(purchase_train_btn, "Click plus button to add a train", new Vector3(127, 447), ActionType.Action.BTN_PRESS);
        Step add_boxcar_step = new Step(purchase_boxcar_btn, "Click plus button to add a boxcar", new Vector3(419, 499), ActionType.Action.BTN_PRESS);
        Step add_hor_track_step = new Step(purchase_track_btn, "Click plus button to add a track", new Vector3(274, 239), ActionType.Action.BTN_PRESS);
        Step add_apartment_step = new Step(purchase_apartment_btn, "Click plus button to add a apartment", new Vector3(417, 29), ActionType.Action.BTN_PRESS);
        Step close_store_step = new Step(buy_btn, "Buy the items", new Vector3(1261, 33), ActionType.Action.BTN_PRESS);
        Step start_drag_track_step = new Step(start_drag_track, "Click on the horizontal track and drag it to an adjacent structure", new Vector3(827, 66), ActionType.Action.DRAG, end_drag_track);
        Step start_drag_apartment_step = new Step(start_drag_apartment, "Drag the apartment next to the track to complete the route", new Vector3(401, 44), ActionType.Action.DRAG, end_drag_apartment);
        Step click_entrance_step = new Step(click_entrance, "Click the station", new Vector3(850, 362), ActionType.Action.CLICK);
        Step start_drag_train_step = new Step(start_drag_train, "Drag the train to a station with a passenger", new Vector3(929, 45), ActionType.Action.DRAG, end_drag_train);
        Step start_drag_boxcar_step = new Step(start_drag_boxcar, "Drag the boxcar to the track with the train to attach it", new Vector3(216, 49), ActionType.Action.DRAG, end_drag_boxcar);
        Step click_person_step = new Step(click_person, "Click the waiting person to begin the boarding process", new Vector3(883, 629), ActionType.Action.CLICK);
        Step board_boxcar_step = new Step(board_boxcar, "After clicking the passenger, click a boxcar of the same color as shown in the thought bubble, in this case a home boxcar", new Vector3(847, 713), ActionType.Action.CLICK);
        Step click_train_step = new Step(click_train, "Click the train to begin the departure sequence", new Vector3(812, 671), ActionType.Action.CLICK);
        Step click_exit_track = new Step(click_exit_west, "Click on the track you would like to depart from, in this case the westbound track", new Vector3(338, 408), ActionType.Action.CLICK);
        Step close_city_step = new Step(exit_city_btn, "Press the exit button to leave the city", new Vector3(1276, 663), ActionType.Action.BTN_PRESS);
        Step click_apartment_step = new Step(click_apartment, "Click on the apartment", new Vector3(639, 394), ActionType.Action.CLICK);
        Step click_train_unload_step = new Step(click_boxcar_to_unload, "Once the train has arrived click it to begin the unloading sequence", new Vector3(847, 713), ActionType.Action.CLICK);
        Step click_room_unload_step = new Step(click_room_to_unload, "Finally click the room to unload the passenger", new Vector3(883, 629), ActionType.Action.CLICK);

        active_tutorial_step_idx = 0;
        tutorial_step_list = new Step[] { click_store_step, add_train_step, add_boxcar_step, add_hor_track_step, add_apartment_step, close_store_step, start_drag_track_step, start_drag_apartment_step,
                                          click_entrance_step, start_drag_train_step, start_drag_boxcar_step, click_person_step, board_boxcar_step, click_train_step, click_exit_track,
                                          close_city_step, click_apartment_step, click_train_unload_step, click_room_unload_step
        };
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void backtrack_tutorial_step()
    {
        active_tutorial_step_idx--;
    }

    public void activate_next_tutorial_step()
    {
        GameObject currently_active_step = tutorial_step_list[active_tutorial_step_idx].step_mask;
        check_if_button_click(currently_active_step); // invoke button press
        currently_active_step.SetActive(false);
        active_tutorial_step_idx++;
        tutorial_step_list[active_tutorial_step_idx].step_mask.SetActive(true);
        instruction_text.text = tutorial_step_list[active_tutorial_step_idx].instruction_text;
        //TODO: mouse pointer image showing where to click (how to make draggable animation?)
    }
}
