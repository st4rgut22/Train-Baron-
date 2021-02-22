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

    //public Button real_store_btn; 
    //public Button real_buy_btn;
    //public Button real_exit_city_btn;
    //public Button real_purchase_train_btn;
    //public Button real_purchase_apartment_btn;
    //public Button real_purchase_boxcar_btn;
    //public Button real_purchase_track_btn;

    //public GameObject click_store_btn;
    //public GameObject purchase_train_btn;
    //public GameObject purchase_boxcar_btn;
    //public GameObject purchase_apartment_btn;
    //public GameObject purchase_track_btn;
    //public GameObject buy_btn;
    //public GameObject start_drag_track;
    //public GameObject end_drag_track;
    //public GameObject start_drag_apartment;
    //public GameObject end_drag_apartment;
    //public GameObject click_entrance;
    //public GameObject start_drag_train;
    //public GameObject end_drag_train;
    //public GameObject start_drag_boxcar;
    //public GameObject end_drag_boxcar;
    //public GameObject click_person;
    //public GameObject board_boxcar;
    //public GameObject click_train;
    //public GameObject click_exit_west;
    //public GameObject exit_city_btn;
    //public GameObject click_apartment;
    //public GameObject click_boxcar_to_unload;
    //public GameObject click_room_to_unload;

    public RectTransform rt1;
    public RectTransform rt2;
    public RectTransform rt3;
    public RectTransform rt4;
    public RectTransform rt5;
    public RectTransform rt6;

    public GameObject tutorial_go;
    public static List<Mask_Group> mask_group_list;

    public GraphicRaycaster graphic_raycaster;

    public static int active_tutorial_step_idx;
    public Text instruction_text;

    public static bool authorize_click;

    public static Step[] tutorial_step_list;

    public class Step
    {
        public string instruction_text;
        Vector3 action_position;
        public Mask_Group mg;
        public ActionType.Action action_type; // click, button_press, drag

        public Step(string instruction, Vector3 pos, ActionType.Action action_type, Mask_Group mg)
        {
            this.instruction_text = instruction;
            action_position = pos;
            this.action_type = action_type;
            this.mg = mg;
        }
    }

    public class Anchor_Pair
    {
        public Vector3 amin;
        public Vector3 amax;
        public Anchor_Pair(float amin_x, float amin_y, float amax_x, float amax_y)
        {
            this.amin = new Vector3(amin_x, amin_y);
            this.amax = new Vector3(amax_x, amax_y);
        }
    }

    public class Mask_Group
    {
        public Anchor_Pair ap1;
        public Anchor_Pair ap2;
        public Anchor_Pair ap3;
        public Anchor_Pair ap4;
        public Anchor_Pair ap5;
        public Anchor_Pair ap6;
        public Mask_Group(Anchor_Pair ap1, Anchor_Pair ap2, Anchor_Pair ap3, Anchor_Pair ap4 = null, Anchor_Pair ap5 = null, Anchor_Pair ap6 = null)
        {
            this.ap1 = ap1;
            this.ap2 = ap2;
            this.ap3 = ap3;
            this.ap4 = ap4;
            this.ap5 = ap5;
            this.ap6 = ap6;
        }
    }

    public static Step get_current_step()
    {
        return tutorial_step_list[active_tutorial_step_idx];
    }

    //public Button block_ui_to_real_btn(GameObject action_go)
    //{
    //    if (action_go == click_store_btn) return real_store_btn; // min (.8587501, .871001) max (.91875, .984458)
    //    else if (action_go == purchase_train_btn) return real_purchase_train_btn;
    //    else if (action_go == purchase_boxcar_btn) return real_purchase_boxcar_btn;
    //    else if (action_go == purchase_track_btn) return real_purchase_track_btn;
    //    else if (action_go == buy_btn) return real_buy_btn;
    //    else if (action_go == exit_city_btn) return real_exit_city_btn;
    //    else if (action_go == purchase_apartment_btn) return real_purchase_apartment_btn;
    //    else { return null; }
    //}

    //public bool is_button_active(GameObject clicked_btn)
    //{
    //    // check if button matches currently active one
    //    if (!GameManager.is_tutorial_mode) return true;
    //    GameObject step_go = get_current_step().step_mask;
    //    Button real_btn = block_ui_to_real_btn(step_go);
    //    if (real_btn != null && real_btn.gameObject == clicked_btn)
    //        return true;
    //    else { return false; }
    //}

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
        Anchor_Pair store_btn_ap_1 = new Anchor_Pair(0, 0, .86649f, .96022f);
        Anchor_Pair store_btn_ap_2 = new Anchor_Pair(.91222f, 0, 1, .96022f);
        Anchor_Pair store_btn_ap_3 = new Anchor_Pair(.86649f, 0, .91222f, .90931f);
        Anchor_Pair store_btn_ap_4 = new Anchor_Pair(0, .96022f, 1, 1);
        Mask_Group store_btn_mg = new Mask_Group(store_btn_ap_1, store_btn_ap_2, store_btn_ap_3, store_btn_ap_4);

        Anchor_Pair buy_train_ap_1 = new Anchor_Pair(0, .66775f, 1, 1);
        Anchor_Pair buy_train_ap_2 = new Anchor_Pair(0, 0, .06984f, .66692f);
        Anchor_Pair buy_train_ap_3 = new Anchor_Pair(.07055f, 0, 1, .61413f);
        Anchor_Pair buy_train_ap_4 = new Anchor_Pair(.0939f, .61413f, 1, .66775f);
        Mask_Group buy_train_mg = new Mask_Group(buy_train_ap_1, buy_train_ap_2, buy_train_ap_3, buy_train_ap_4);

        Anchor_Pair buy_boxcar_ap_1 = new Anchor_Pair(0, .66913f, 1, 1);
        Anchor_Pair buy_boxcar_ap_2 = new Anchor_Pair(0, .61413f, .29478f, .66913f);
        Anchor_Pair buy_boxcar_ap_3 = new Anchor_Pair(0, 0, 1, .61413f);
        Anchor_Pair buy_boxcar_ap_4 = new Anchor_Pair(.31778f, .61413f, 1, .66913f);
        Mask_Group buy_boxcar_mg = new Mask_Group(buy_boxcar_ap_1, buy_boxcar_ap_2, buy_boxcar_ap_3, buy_boxcar_ap_4);

        Anchor_Pair buy_apartment_ap_1 = new Anchor_Pair(0, .10515f, 1, 1);
        Anchor_Pair buy_apartment_ap_2 = new Anchor_Pair(0,.049f,.29275f,.10515f);
        Anchor_Pair buy_apartment_ap_3 = new Anchor_Pair(0, 0, 1, .049f);
        Anchor_Pair buy_apartment_ap_4 = new Anchor_Pair(.31771f, .049f, 1, .108f);
        Mask_Group buy_apartment_mg = new Mask_Group(buy_apartment_ap_1, buy_apartment_ap_2, buy_apartment_ap_3, buy_apartment_ap_4);

        Anchor_Pair buy_track_ap_1 = new Anchor_Pair(0, .38461f, .99829f, 1.0043f);
        Anchor_Pair buy_track_ap_2 = new Anchor_Pair(0,.33146f,.182f,.38461f);
        Anchor_Pair buy_track_ap_3 = new Anchor_Pair(0, 0, 1, .33146f);
        Anchor_Pair buy_track_ap_4 = new Anchor_Pair(.20525f, .33146f, 1, .38461f);
        Mask_Group buy_track_mg = new Mask_Group(buy_track_ap_1, buy_track_ap_2, buy_track_ap_3, buy_track_ap_4);

        Anchor_Pair buy_item_ap_1 = new Anchor_Pair(-.003f, .17686f, 1, 1.00043f);
        Anchor_Pair buy_item_ap_2 = new Anchor_Pair(0, 0, 1, .06145f);
        Anchor_Pair buy_item_ap_3 = new Anchor_Pair(.9575f, .06145f, 1, .17686f);
        Anchor_Pair buy_item_ap_4 = new Anchor_Pair(0,.06145f,.78f,.17686f);
        Mask_Group buy_item_mg = new Mask_Group(buy_item_ap_1, buy_item_ap_2, buy_item_ap_3, buy_item_ap_4);

        Anchor_Pair start_drag_track_ap_1 = new Anchor_Pair(0, .13751f, 1, 1);
        Anchor_Pair start_drag_track_ap_2 = new Anchor_Pair(0, 0, .58737f, .13751f);
        Anchor_Pair start_drag_track_ap_3 = new Anchor_Pair(.655f, 0, 1, .13751f);
        Mask_Group start_drag_track_mg = new Mask_Group(start_drag_track_ap_1, start_drag_track_ap_2, start_drag_track_ap_3);

        Anchor_Pair end_drag_track_ap_1 = new Anchor_Pair(0, .60231f, 1, 1);
        Anchor_Pair end_drag_track_ap_2 = new Anchor_Pair(.58344f, 0, 1, .60231f);
        Anchor_Pair end_drag_track_ap_3 = new Anchor_Pair(0, 0, .53132f, .60231f);
        Anchor_Pair end_drag_track_ap_4 = new Anchor_Pair(.53132f, 0, .58344f, .5f);
        Mask_Group end_drag_track_mg = new Mask_Group(end_drag_track_ap_1, end_drag_track_ap_2, end_drag_track_ap_3, end_drag_track_ap_4);

        Anchor_Pair start_drag_apartment_ap_1 = new Anchor_Pair(0, .13751f, 1, 1);
        Anchor_Pair start_drag_apartment_ap_2 = new Anchor_Pair(0, 0, .258f, .13751f);
        Anchor_Pair start_drag_apartment_ap_3 = new Anchor_Pair(.329f, 0, 1, .13751f);
        Mask_Group start_drag_apartment_mg = new Mask_Group(start_drag_apartment_ap_1, start_drag_apartment_ap_2, start_drag_apartment_ap_3);

        Anchor_Pair end_drag_apartment_ap_1 = new Anchor_Pair(0, .60231f, 1, 1);
        Anchor_Pair end_drag_apartment_ap_2 = new Anchor_Pair(.52721f, 0, 1, .60231f);
        Anchor_Pair end_drag_apartment_ap_3 = new Anchor_Pair(0, 0, .47337f, .60231f);
        Anchor_Pair end_drag_apartment_ap_4 = new Anchor_Pair(.47337f, 0, .52721f, .5f);
        Mask_Group end_drag_apartment_mg = new Mask_Group(end_drag_apartment_ap_1, end_drag_apartment_ap_2, end_drag_apartment_ap_3, end_drag_apartment_ap_4);

        Anchor_Pair click_entrance_ap_1 = new Anchor_Pair(0, 0, .58737f, 1);
        Anchor_Pair click_entrance_ap_2 = new Anchor_Pair(.64747f, 0, 1, 1);
        Anchor_Pair click_entrance_ap_3 = new Anchor_Pair(.58737f, 0, .64747f, .5f);
        Anchor_Pair click_entrance_ap_4 = new Anchor_Pair(.58737f, .60172f, .64747f, 1);
        Mask_Group click_entrance_mg = new Mask_Group(click_entrance_ap_1, click_entrance_ap_2, click_entrance_ap_3, click_entrance_ap_4);

        Anchor_Pair start_drag_train_ap_1 = new Anchor_Pair(0, 0, .6825f, .96022f);
        Anchor_Pair start_drag_train_ap_2 = new Anchor_Pair(.7484193f,0,1,.96022f);
        Anchor_Pair start_drag_train_ap_3 = new Anchor_Pair(.6825f,.1177772f,.7484193f,.96022f);
        Anchor_Pair start_drag_train_ap_4 = new Anchor_Pair(0,.96022f,1,1);
        Mask_Group start_drag_train_mg = new Mask_Group(start_drag_train_ap_1, start_drag_train_ap_2, start_drag_train_ap_3, start_drag_train_ap_4);

        Anchor_Pair end_drag_train_ap_1 = new Anchor_Pair(0, .8380001f, .59626f, 1);
        Anchor_Pair end_drag_train_ap_2 = new Anchor_Pair(.64032f, 0, 1, .93087f);
        Anchor_Pair end_drag_train_ap_3 = new Anchor_Pair(0, 0, .64032f, .8380001f);
        Mask_Group end_drag_train_mg = new Mask_Group(end_drag_train_ap_1, end_drag_train_ap_2, end_drag_train_ap_3);

        Anchor_Pair start_drag_boxcar_1 = new Anchor_Pair(0, .117f, 1, 1);
        Anchor_Pair start_drag_boxcar_2 = new Anchor_Pair(.203f, .01192f, 1, .117f);
        Anchor_Pair start_drag_boxcar_3 = new Anchor_Pair(0, .01192f, .145f, .117f);
        Anchor_Pair start_drag_boxcar_4 = new Anchor_Pair(0, 0, 1, .01192f);
        Mask_Group start_drag_boxcar_mg = new Mask_Group(start_drag_boxcar_1, start_drag_boxcar_2, start_drag_boxcar_3, start_drag_boxcar_4);

        Anchor_Pair end_drag_boxcar_ap_1 = new Anchor_Pair(0, .8380001f, .59626f, 1);
        Anchor_Pair end_drag_boxcar_ap_2 = new Anchor_Pair(.64032f, 0, 1, .93087f);
        Anchor_Pair end_drag_boxcar_ap_3 = new Anchor_Pair(0, 0, .64032f, .8380001f);
        Mask_Group end_drag_boxcar_mg = new Mask_Group(end_drag_boxcar_ap_1, end_drag_boxcar_ap_2, end_drag_boxcar_ap_3);

        Anchor_Pair click_person_ap_1 = new Anchor_Pair(0, 0, .64671f, 1);
        Anchor_Pair click_person_ap_2 = new Anchor_Pair(.70482f, 0, 1, 1);
        Anchor_Pair click_person_ap_3 = new Anchor_Pair(.64671f, 0, .70482f, .81036f);
        Anchor_Pair click_person_ap_4 = new Anchor_Pair(.64671f, .917f, .70482f, 1);
        Mask_Group click_person_mg = new Mask_Group(click_person_ap_1, click_person_ap_2, click_person_ap_3, click_person_ap_4);

        Anchor_Pair click_boxcar_ap_1 = new Anchor_Pair(0, 0, .61929f, 1);
        Anchor_Pair click_boxcar_ap_2 = new Anchor_Pair(.67124f, 0, 1, 1);
        Anchor_Pair click_boxcar_ap_3 = new Anchor_Pair(.61929f, 0, .67124f, .94836f);
        Anchor_Pair click_boxcar_ap_4 = new Anchor_Pair(.61929f, .98251f, .67124f, 1);
        Mask_Group click_boxcar_mg = new Mask_Group(click_boxcar_ap_1, click_boxcar_ap_2, click_boxcar_ap_3, click_boxcar_ap_4);

        Anchor_Pair click_train_ap_1 = new Anchor_Pair(0, 0, .611f, 1);
        Anchor_Pair click_train_ap_2 = new Anchor_Pair(.62476f, 0, 1, 1);
        Anchor_Pair click_train_ap_3 = new Anchor_Pair(.611f, 0, .62476f, .86272f);
        Anchor_Pair click_train_ap_4 = new Anchor_Pair(.611f, .96451f, .62476f, 1);
        Mask_Group click_train_mg = new Mask_Group(click_train_ap_1, click_train_ap_2, click_train_ap_3, click_train_ap_4);

        Anchor_Pair click_exit_west_ap_1 = new Anchor_Pair(0, .60349f, 1, 1);
        Anchor_Pair click_exit_west_ap_2 = new Anchor_Pair(0, 0, 1, .5f);
        Anchor_Pair click_exit_west_ap_3 = new Anchor_Pair(.356486f, 0, 1, 1);
        Mask_Group click_exit_west_mg = new Mask_Group(click_exit_west_ap_1, click_exit_west_ap_2, click_exit_west_ap_3);

        Anchor_Pair exit_city_btn_ap_1 = new Anchor_Pair(-.0003f, .97868f, 1, 1.00043f);
        Anchor_Pair exit_city_btn_ap_2 = new Anchor_Pair(0, 0, .95532f, .97868f);
        Anchor_Pair exit_city_btn_ap_3 = new Anchor_Pair(.95532f, 0, 1, .91723f);
        Anchor_Pair exit_city_btn_ap_4 = new Anchor_Pair(.99033f, .91723f, 1, .97868f);
        Mask_Group exit_city_mg = new Mask_Group(exit_city_btn_ap_1, exit_city_btn_ap_2, exit_city_btn_ap_3, exit_city_btn_ap_4);

        Anchor_Pair click_apartment_ap_1 = new Anchor_Pair(0, 0, .46724f, 1);
        Anchor_Pair click_apartment_ap_2 = new Anchor_Pair(.52929f, 0, 1, 1);
        Anchor_Pair click_apartment_ap_3 = new Anchor_Pair(.46724f, 0, .52929f, .5f);
        Anchor_Pair click_apartment_ap_4 = new Anchor_Pair(.46724f, .60172f, .52929f, 1);
        Mask_Group click_apartment_mg = new Mask_Group(click_apartment_ap_1, click_apartment_ap_2, click_apartment_ap_3, click_apartment_ap_4);

        Anchor_Pair click_boxcar_to_unload_ap_1 = new Anchor_Pair(0, 0, .61929f, 1);
        Anchor_Pair click_boxcar_to_unload_ap_2 = new Anchor_Pair(.67124f, 0, 1, 1);
        Anchor_Pair click_boxcar_to_unload_ap_3 = new Anchor_Pair(.61929f, 0, .67124f, .94936f);
        Anchor_Pair click_boxcar_to_unload_ap_4 = new Anchor_Pair(.61929f, .983f, .67124f, 1);
        Mask_Group click_boxcar_to_unload_mg = new Mask_Group(click_boxcar_to_unload_ap_1, click_boxcar_to_unload_ap_2, click_boxcar_to_unload_ap_3, click_boxcar_to_unload_ap_4);

        Anchor_Pair click_room_to_unload_mask_1 = new Anchor_Pair(0, 0, .64671f, 1);
        Anchor_Pair click_room_to_unload_mask_2 = new Anchor_Pair(.70482f, 0, 1, 1);
        Anchor_Pair click_room_to_unload_mask_3 = new Anchor_Pair(.64671f, 0, .70482f, .81036f);
        Anchor_Pair click_room_to_unload_mask_4 = new Anchor_Pair(.64671f, .917f, .70482f, 1);
        Mask_Group click_room_to_unload_mg = new Mask_Group(click_room_to_unload_mask_1, click_room_to_unload_mask_2, click_room_to_unload_mask_3, click_room_to_unload_mask_4);

        authorize_click = false;
        graphic_raycaster = GetComponent<GraphicRaycaster>();

        Step click_store_step = new Step("Click on the store to buy a train", new Vector3(1159, 675), ActionType.Action.BTN_PRESS, store_btn_mg);
        Step add_train_step = new Step("Click plus button to add a train", new Vector3(127, 447), ActionType.Action.BTN_PRESS, buy_train_mg);
        Step add_boxcar_step = new Step("Click plus button to add a boxcar", new Vector3(419, 499), ActionType.Action.BTN_PRESS, buy_boxcar_mg);
        Step add_hor_track_step = new Step("Click plus button to add a track", new Vector3(274, 239), ActionType.Action.BTN_PRESS, buy_track_mg);
        Step add_apartment_step = new Step("Click plus button to add a apartment", new Vector3(417, 29), ActionType.Action.BTN_PRESS, buy_apartment_mg);
        Step close_store_step = new Step("Buy the items", new Vector3(1261, 33), ActionType.Action.BTN_PRESS, buy_item_mg);
        Step start_drag_track_step = new Step("Drag the horizontal track next to the structure", new Vector3(827, 66), ActionType.Action.DRAG, start_drag_track_mg);
        Step end_drag_track_step = new Step("Drag the horizontal track next to the structure", new Vector3(827, 66), ActionType.Action.DRAG, end_drag_track_mg);
        Step start_drag_apartment_step = new Step("Drag the apartment next to the track to complete the route", new Vector3(401, 44), ActionType.Action.DRAG, start_drag_apartment_mg);
        Step end_drag_apartment_step = new Step("End Drag the apartment next to the track to complete the route", new Vector3(401, 44), ActionType.Action.DRAG, end_drag_apartment_mg);
        Step click_entrance_step = new Step("Click the station", new Vector3(850, 362), ActionType.Action.CLICK, click_entrance_mg);
        Step start_drag_train_step = new Step("Drag the train to a station with a passenger", new Vector3(929, 45), ActionType.Action.DRAG, start_drag_train_mg);
        Step end_drag_train_step = new Step("End Drag the train to a station with a passenger", new Vector3(929, 45), ActionType.Action.DRAG, end_drag_train_mg);
        Step start_drag_boxcar_step = new Step("Drag the boxcar to the track with the train to attach it", new Vector3(216, 49), ActionType.Action.DRAG, start_drag_boxcar_mg);
        Step end_drag_boxcar_step = new Step("End Drag the boxcar to the track with the train to attach it", new Vector3(216, 49), ActionType.Action.DRAG, end_drag_boxcar_mg);
        Step click_person_step = new Step( "Click the waiting person to begin the boarding process", new Vector3(883, 629), ActionType.Action.CLICK, click_person_mg);
        Step board_boxcar_step = new Step("After clicking the passenger, click a boxcar of the same color as shown in the thought bubble, in this case a home boxcar", new Vector3(847, 713), ActionType.Action.CLICK, click_boxcar_mg);
        Step click_train_step = new Step( "Click the train to begin the departure sequence", new Vector3(812, 671), ActionType.Action.CLICK, click_train_mg);
        Step click_exit_track = new Step("Click on the track you would like to depart from, in this case the westbound track", new Vector3(338, 408), ActionType.Action.CLICK, click_exit_west_mg);
        Step close_city_step = new Step("Press the exit button to leave the city", new Vector3(1276, 663), ActionType.Action.BTN_PRESS, exit_city_mg);
        Step click_apartment_step = new Step("Click on the apartment", new Vector3(639, 394), ActionType.Action.CLICK, click_apartment_mg);
        Step click_train_unload_step = new Step("Once the train has arrived click it to begin the unloading sequence", new Vector3(847, 713), ActionType.Action.CLICK, click_boxcar_to_unload_mg);
        Step click_room_unload_step = new Step("Finally click the room to unload the passenger", new Vector3(883, 629), ActionType.Action.CLICK, click_room_to_unload_mg);


        active_tutorial_step_idx = -1;
        tutorial_step_list = new Step[] { click_store_step, add_train_step, add_boxcar_step, add_hor_track_step, add_apartment_step, close_store_step, start_drag_track_step, end_drag_track_step, start_drag_apartment_step, end_drag_apartment_step,
                                          click_entrance_step, start_drag_train_step, end_drag_train_step, start_drag_boxcar_step, end_drag_boxcar_step, click_person_step, board_boxcar_step, click_train_step, click_exit_track,
                                          close_city_step, click_apartment_step, click_train_unload_step, click_room_unload_step
        };
        StartCoroutine(activate_next_tutorial_step());
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void backtrack_tutorial_step()
    {
        active_tutorial_step_idx--;
        Mask_Group mg = tutorial_step_list[active_tutorial_step_idx].mg;
        set_all_anchor_points(mg);
    }

    public void set_anchor_points(Anchor_Pair ap, RectTransform rt)
    {
        rt.anchorMin = ap.amin;
        rt.anchorMax = ap.amax;
    }

    public void set_all_anchor_points(Mask_Group mg)
    {
        if (mg.ap4 == null)
            rt4.gameObject.SetActive(false);
        else
        {
            rt4.gameObject.SetActive(true);
            set_anchor_points(mg.ap4, rt4);
        }
        if (mg.ap5 == null)
            rt5.gameObject.SetActive(false);
        else
        {
            rt5.gameObject.SetActive(true);
            set_anchor_points(mg.ap5, rt5);
        }
        if (mg.ap6 == null)
            rt6.gameObject.SetActive(false);
        else
        {
            rt6.gameObject.SetActive(true);
            set_anchor_points(mg.ap6, rt6);
        }
        set_anchor_points(mg.ap1, rt1);
        set_anchor_points(mg.ap2, rt2);
        set_anchor_points(mg.ap3, rt3);
    }

    public IEnumerator activate_next_tutorial_step(int delay = 0)
    {
        print("wait for delay " + delay);
        yield return new WaitForSeconds(delay);
        active_tutorial_step_idx++;
        if (active_tutorial_step_idx < tutorial_step_list.Length)
        {
            Step step = tutorial_step_list[active_tutorial_step_idx];
            instruction_text.text = tutorial_step_list[active_tutorial_step_idx].instruction_text;
            Mask_Group mg = step.mg;
            set_all_anchor_points(mg);            
        }
    }

    public bool is_click_in_wrong_place()
    {
        if (GameManager.is_tutorial_mode)
        {
            bool is_it_hit = GameManager.tutorial_manager.did_raycast_hit_blocking_mask();
            print("is it hit " + is_it_hit);
            if (is_it_hit)
            {
                backtrack_tutorial_step();
                return true; // if hit blocking mask stop
            }
            else
            {
                StartCoroutine(activate_next_tutorial_step());
                return false;
            }
        }
        return false;
    }
}
