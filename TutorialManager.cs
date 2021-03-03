using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : EventDetector
{
    public GameObject m1;
    public GameObject m2;
    public GameObject m3;
    public GameObject m4;
    public GameObject m5;
    public GameObject m6;

    public GameObject last_step_5;
    public GameObject last_step_6;
    public GameObject last_step_7;
    public GameObject last_step_8;

    public RectTransform rt1;
    public RectTransform rt2;
    public RectTransform rt3;
    public RectTransform rt4;
    public RectTransform rt5;
    public RectTransform rt6;

    public GraphicRaycaster gr1;
    public GraphicRaycaster gr2;
    public GraphicRaycaster gr3;
    public GraphicRaycaster gr4;
    public GraphicRaycaster gr5;
    public GraphicRaycaster gr6;

    public static bool board_flag = false;
    public static bool unload_flag = false;
    public static bool exit_flag = false;
    public static bool room_flag = false;
    public static bool boxcar_flag = true; // activate later
    public static bool train_flag = false;

    public GameObject tutorial_go;
    public static List<Mask_Group> mask_group_list;

    public GraphicRaycaster[] gr_list;
    public GraphicRaycaster graphic_raycaster;

    public static int active_tutorial_step_idx;
    public Text instruction_text;
    public GameObject instruction;

    public static Step[] tutorial_step_list;
    public static bool step_in_progress;

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


    public void toggle_raycast(bool on)
    {
        //rt1.gameObject.GetComponent<Image>().raycastTarget = on;
        //rt2.gameObject.GetComponent<Image>().raycastTarget = on;
        //rt3.gameObject.GetComponent<Image>().raycastTarget = on;
        //rt4.gameObject.GetComponent<Image>().raycastTarget = on;
        ////rt5.gameObject.GetComponent<Image>().raycastTarget = on;
        ////rt6.gameObject.GetComponent<Image>().raycastTarget = on;
    }

    public bool did_raycast_hit_blocking_mask()
    {
        //Code to be place in a MonoBehaviour with a GraphicRaycaster component
        //print("turn off raycast target");
        //GameManager.tutorial_manager.toggle_raycast(false);
        //Create the PointerEventData with null for the EventSystem
        PointerEventData ped = new PointerEventData(null);
        //Set required parameters, in this case, mouse position
        ped.position = Input.mousePosition;
        //Create list to receive all results
        List<RaycastResult> results = new List<RaycastResult>();
        //Raycast it
        for (int i = 0; i < gr_list.Length; i++)
        {
            GraphicRaycaster gr = gr_list[i];            
            gr.Raycast(ped, results);
            if (results.Count > 0) { return true; }
        }
        return false; // no mask hit by raycasters
    }

    void Start()
    {
        step_in_progress = true;

        gr_list = new GraphicRaycaster[] { gr1, gr2, gr3, gr4, gr5, gr6 };


        Anchor_Pair store_btn_ap_1 = new Anchor_Pair(0, 0, .86649f, .96022f);
        Anchor_Pair store_btn_ap_2 = new Anchor_Pair(.91222f, 0, 1, .96022f);
        Anchor_Pair store_btn_ap_3 = new Anchor_Pair(.86649f, 0, .91222f, .90931f);
        Anchor_Pair store_btn_ap_4 = new Anchor_Pair(0, .96022f, 1, 1);
        Mask_Group store_btn_mg = new Mask_Group(store_btn_ap_1, store_btn_ap_2, store_btn_ap_3, store_btn_ap_4);

        Anchor_Pair buy_train_ap_1 = new Anchor_Pair(0, .66775f, 1, 1);
        Anchor_Pair buy_train_ap_2 = new Anchor_Pair(0, 0, .07055f, .66775f);
        Anchor_Pair buy_train_ap_3 = new Anchor_Pair(.07055f, 0, 1, .61413f);
        Anchor_Pair buy_train_ap_4 = new Anchor_Pair(.1555f, .61413f, 1, .66775f);
        Mask_Group buy_train_mg = new Mask_Group(buy_train_ap_1, buy_train_ap_2, buy_train_ap_3, buy_train_ap_4);

        Anchor_Pair buy_boxcar_ap_1 = new Anchor_Pair(0, .66913f, 1, 1);
        Anchor_Pair buy_boxcar_ap_2 = new Anchor_Pair(0, 0, .29475f, .66913f);
        Anchor_Pair buy_boxcar_ap_3 = new Anchor_Pair(.49375f, 0, 1, .61413f);
        Anchor_Pair buy_boxcar_ap_4 = new Anchor_Pair(.379f, .61413f, 1, .66913f);
        Anchor_Pair buy_boxcar_ap_5 = new Anchor_Pair(.29475f, 0, .49375f, .1176667f);
        Anchor_Pair buy_boxcar_ap_6 = new Anchor_Pair(.29475f, .2915556f, .49375f, .61413f);
        Mask_Group buy_boxcar_mg = new Mask_Group(buy_boxcar_ap_1, buy_boxcar_ap_2, buy_boxcar_ap_3, buy_boxcar_ap_4, buy_boxcar_ap_5, buy_boxcar_ap_6);

        Anchor_Pair buy_apartment_ap_1 = new Anchor_Pair(0, .108f, 1, 1);
        Anchor_Pair buy_apartment_ap_2 = new Anchor_Pair(0, .049f, .29275f, .108f);
        Anchor_Pair buy_apartment_ap_3 = new Anchor_Pair(0, 0, 1, .049f);
        Anchor_Pair buy_apartment_ap_4 = new Anchor_Pair(.379f, .049f, 1, .108f);
        Mask_Group buy_apartment_mg = new Mask_Group(buy_apartment_ap_1, buy_apartment_ap_2, buy_apartment_ap_3, buy_apartment_ap_4);

        Anchor_Pair buy_track_ap_1 = new Anchor_Pair(0, .38461f, .99829f, 1.0043f);
        Anchor_Pair buy_track_ap_2 = new Anchor_Pair(0, .33146f, .182f, .38461f);
        Anchor_Pair buy_track_ap_3 = new Anchor_Pair(0, 0, 1, .33146f);
        Anchor_Pair buy_track_ap_4 = new Anchor_Pair(.2665f, .33146f, 1, .38461f);
        Mask_Group buy_track_mg = new Mask_Group(buy_track_ap_1, buy_track_ap_2, buy_track_ap_3, buy_track_ap_4);

        Anchor_Pair buy_item_ap_1 = new Anchor_Pair(-.003f, .17686f, 1, 1.00043f);
        Anchor_Pair buy_item_ap_2 = new Anchor_Pair(0, 0, 1, .06145f);
        Anchor_Pair buy_item_ap_3 = new Anchor_Pair(.9575f, .06145f, 1, .17686f);
        Anchor_Pair buy_item_ap_4 = new Anchor_Pair(0, .06145f, .78f, .17686f);
        Mask_Group buy_item_mg = new Mask_Group(buy_item_ap_1, buy_item_ap_2, buy_item_ap_3, buy_item_ap_4);

        Anchor_Pair start_drag_track_ap_1 = new Anchor_Pair(0, 0.1331073f, 1, 1);
        Anchor_Pair start_drag_track_ap_2 = new Anchor_Pair(0, 0, 0.58237f, 0.1331073f);
        Anchor_Pair start_drag_track_ap_3 = new Anchor_Pair(0.6625001f, 0, 1, 0.1331073f);
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
        Anchor_Pair start_drag_train_ap_2 = new Anchor_Pair(.7484193f, 0, 1, .96022f);
        Anchor_Pair start_drag_train_ap_3 = new Anchor_Pair(.6825f, .1177772f, .7484193f, .96022f);
        Anchor_Pair start_drag_train_ap_4 = new Anchor_Pair(0, .96022f, 1, 1);
        Mask_Group start_drag_train_mg = new Mask_Group(start_drag_train_ap_1, start_drag_train_ap_2, start_drag_train_ap_3, start_drag_train_ap_4);

        Anchor_Pair end_drag_train_ap_1 = new Anchor_Pair(0, .8380001f, .59626f, 1);
        Anchor_Pair end_drag_train_ap_2 = new Anchor_Pair(.64032f, 0, 1, .93087f);
        Anchor_Pair end_drag_train_ap_3 = new Anchor_Pair(0, 0, .64032f, .8380001f);
        Mask_Group end_drag_train_mg = new Mask_Group(end_drag_train_ap_1, end_drag_train_ap_2, end_drag_train_ap_3);

        Anchor_Pair start_drag_boxcar_1 = new Anchor_Pair(0, .117f, 1, 1);
        Anchor_Pair start_drag_boxcar_2 = new Anchor_Pair(.203f, .01192f, 1, .117f);
        Anchor_Pair start_drag_boxcar_3 = new Anchor_Pair(0, .01192f, 0.1375f, .117f);
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
        Anchor_Pair click_exit_west_ap_3 = new Anchor_Pair(0.354f, 0.5f, 1, 0.60349f);
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

        Anchor_Pair exit_shipyard_ap_1 = new Anchor_Pair(-.0003f, .97868f, 1, 1.00043f);
        Anchor_Pair exit_shipyard_ap_2 = new Anchor_Pair(0, 0.09321469f, .95532f, .97868f);
        Anchor_Pair exit_shipyard_ap_3 = new Anchor_Pair(.95532f, 0, 1, .91723f);
        Anchor_Pair exit_shipyard_ap_4 = new Anchor_Pair(.99033f, .91723f, 1, .97868f);
        Anchor_Pair exit_shipyard_ap_5 = new Anchor_Pair(0, 0, .405f, .09321f);
        Anchor_Pair exit_shipyard_ap_6 = new Anchor_Pair(.5975f, 0, .95532f, .09321469f);
        Mask_Group exit_shipyard_mg = new Mask_Group(exit_shipyard_ap_1, exit_shipyard_ap_2, exit_shipyard_ap_3, exit_shipyard_ap_4, exit_shipyard_ap_5, exit_shipyard_ap_6);

        Anchor_Pair exit_tutorial_btn_ap_1 = new Anchor_Pair(-.0003f, .97868f, 1, 1.00043f);
        Anchor_Pair exit_tutorial_btn_ap_2 = new Anchor_Pair(.64375f, 0, .95532f, .97868f);
        Anchor_Pair exit_tutorial_btn_ap_3 = new Anchor_Pair(.95532f, 0, 1, .91723f);
        Anchor_Pair exit_tutorial_btn_ap_4 = new Anchor_Pair(.99033f, .91723f, 1, .97868f);
        Anchor_Pair exit_tutorial_btn_ap_5 = new Anchor_Pair(0, 0, .51125f, .97868f);
        Anchor_Pair exit_tutorial_btn_ap_6 = new Anchor_Pair(.51125f, 0, .64375f, .865444f);
        Mask_Group exit_tutorial_mg = new Mask_Group(exit_tutorial_btn_ap_1, exit_tutorial_btn_ap_2, exit_tutorial_btn_ap_3, exit_tutorial_btn_ap_4, exit_tutorial_btn_ap_5, exit_tutorial_btn_ap_6);

        graphic_raycaster = GetComponent<GraphicRaycaster>();

        Step click_store_step = new Step("Click on the store to stock up on infrastructure and vehicles.", new Vector3(1159, 675), ActionType.Action.BTN_PRESS, store_btn_mg);
        Step add_train_step = new Step("Add a train to your inventory. Each train can pull up to four boxcars.", new Vector3(127, 447), ActionType.Action.BTN_PRESS, buy_train_mg);
        Step add_boxcar_step = new Step("Add a boxcar to your inventory. An orange boxcar will board passengers going to orange structures, in this case residential buildings.", new Vector3(419, 499), ActionType.Action.BTN_PRESS, buy_boxcar_mg);
        Step add_hor_track_step = new Step("Add a track to your inventory to get from point A to point B.", new Vector3(274, 239), ActionType.Action.BTN_PRESS, buy_track_mg);
        Step add_apartment_step = new Step("Add an apartment. When your passengers become wealthy, they'll prefer a fancier structure like a mansion, office building or restaurant.", new Vector3(417, 29), ActionType.Action.BTN_PRESS, buy_apartment_mg);
        Step close_store_step = new Step("Time to splash the cash. Don't worry you'll make your money back from ticket sales.", new Vector3(1261, 33), ActionType.Action.BTN_PRESS, buy_item_mg);
        Step start_drag_track_step = new Step("Drag the track west of the station. Each structure can have up to four outbound tracks.", new Vector3(827, 66), ActionType.Action.DRAG, start_drag_track_mg);
        Step end_drag_track_step = new Step("Tip: you can put tracks on top of each other and click on them to change direction.", new Vector3(827, 66), ActionType.Action.DRAG, end_drag_track_mg);
        Step start_drag_apartment_step = new Step("Drag the apartment west of the track to finish the route.", new Vector3(401, 44), ActionType.Action.DRAG, start_drag_apartment_mg);
        Step end_drag_apartment_step = new Step("Tip: Using fewer tracks for a route costs less and leads to shorter route times.", new Vector3(401, 44), ActionType.Action.DRAG, end_drag_apartment_mg);
        Step click_entrance_step = new Step("Click the station to see the vehicles you bought and newly arrived passengers.", new Vector3(850, 362), ActionType.Action.CLICK, click_entrance_mg);
        Step start_drag_train_step = new Step("Drag the train to a station with a passenger", new Vector3(929, 45), ActionType.Action.DRAG, start_drag_train_mg);
        Step end_drag_train_step = new Step("Tip: Rooms are initially on the eastern side of the city, so plan your route accordingly. ", new Vector3(929, 45), ActionType.Action.DRAG, end_drag_train_mg);
        Step start_drag_boxcar_step = new Step("Drag the boxcar to the track with the train to attach it", new Vector3(216, 49), ActionType.Action.DRAG, start_drag_boxcar_mg);
        Step end_drag_boxcar_step = new Step("Tip: You can park boxcars on the lots next to each building for future use.", new Vector3(216, 49), ActionType.Action.DRAG, end_drag_boxcar_mg);
        Step click_person_step = new Step("It looks like your passenger wants to board an orange boxcar. Click on the passenger to begin the boarding process.", new Vector3(883, 629), ActionType.Action.CLICK, click_person_mg);
        Step board_boxcar_step = new Step("Click a boxcar of the same color.", new Vector3(847, 713), ActionType.Action.CLICK, click_boxcar_mg);
        Step click_train_step = new Step("Click the train to depart the station. ", new Vector3(812, 671), ActionType.Action.CLICK, click_train_mg);
        Step click_exit_track = new Step("Click on the track you would like to depart from. The light is green which means the west-bound route has no obstacles.", new Vector3(338, 408), ActionType.Action.CLICK, click_exit_west_mg);
        Step close_city_step = new Step("Press the exit button to leave the city", new Vector3(1276, 663), ActionType.Action.BTN_PRESS, exit_city_mg);
        Step click_apartment_step = new Step("Click on the apartment.", new Vector3(639, 394), ActionType.Action.CLICK, click_apartment_mg);
        Step click_train_unload_step = new Step("The train has arrived. Click it to begin unloading the passenger.", new Vector3(847, 713), ActionType.Action.CLICK, click_boxcar_to_unload_mg);
        Step click_room_unload_step = new Step("Click an available room to book the guest. ", new Vector3(883, 629), ActionType.Action.CLICK, click_room_to_unload_mg);
        Step close_shipyard_step = new Step("Good reviews open new rooms and attract more customers. You can exit the apartment, but check back soon. Long boarding or trip times can shutter your business.", new Vector3(0, 0), ActionType.Action.CLICK, exit_shipyard_mg);
        Step close_tutorial_step = new Step("Your population size grew! Meet the population requirement to complete the level. Exit the tutorial and begin playing.", new Vector3(0, 0), ActionType.Action.CLICK, exit_tutorial_mg);
        // new Step for quota
        active_tutorial_step_idx = -1;
        tutorial_step_list = new Step[] { click_store_step, add_train_step, add_boxcar_step, add_hor_track_step, add_apartment_step, close_store_step, start_drag_track_step, end_drag_track_step, start_drag_apartment_step, end_drag_apartment_step,
                                          click_entrance_step, start_drag_train_step, end_drag_train_step, start_drag_boxcar_step, end_drag_boxcar_step, click_person_step, board_boxcar_step, click_train_step, click_exit_track,
                                          close_city_step, click_apartment_step, click_train_unload_step, click_room_unload_step, close_shipyard_step, close_tutorial_step
        };
        StartCoroutine(activate_next_tutorial_step());
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void backtrack_tutorial_step()
    {
        //print("backtracking tutorial step");
        active_tutorial_step_idx--;
        Mask_Group mg = tutorial_step_list[active_tutorial_step_idx].mg;     
        set_all_anchor_points(mg);
        instruction_text.text = tutorial_step_list[active_tutorial_step_idx].instruction_text;
    }

    public void set_anchor_points(Anchor_Pair ap, RectTransform rt)
    {
        rt.anchorMin = ap.amin;
        rt.anchorMax = ap.amax;
        rt.SetLeft(0);
        rt.SetRight(0);
        rt.SetTop(0);
        rt.SetBottom(0);
    }

    public void set_all_anchor_points(Mask_Group mg)
    {
        if (mg.ap4 == null)
            m4.SetActive(false);
        else
        {
            m4.SetActive(true);
            set_anchor_points(mg.ap4, rt4);
        }
        if (mg.ap5 != null)
        {
            m5.SetActive(true);
            set_anchor_points(mg.ap5, rt5);
        } else {
            m5.SetActive(false);
        }
        if (mg.ap6 != null)
        {
            m6.SetActive(true);
            set_anchor_points(mg.ap6, rt6);
        } else
        {
            m6.SetActive(false);
        }
        set_anchor_points(mg.ap1, rt1);
        set_anchor_points(mg.ap2, rt2);
        set_anchor_points(mg.ap3, rt3);
    }

    public void hide_mask(bool on)
    {
        m1.SetActive(on);
        m2.SetActive(on);
        m3.SetActive(on);
        m4.SetActive(on);
    }

    public IEnumerator activate_next_tutorial_step(int delay = 0)
    {
        //print("wait for delay " + delay);
        if (delay != 0)
        {
            //hide_mask(false); // hide the previous message, if waiting for new one
            instruction.SetActive(false);
        }
        step_in_progress = true;
        yield return new WaitForSeconds(delay);
        step_in_progress = false;
        //hide_mask(true);
        instruction.SetActive(true);
        active_tutorial_step_idx++;
        if (active_tutorial_step_idx < tutorial_step_list.Length)
        {
            if (active_tutorial_step_idx == tutorial_step_list.Length - 1)
            {
                last_step_5.SetActive(true);
                last_step_6.SetActive(true);
                last_step_7.SetActive(true);
                last_step_8.SetActive(true);
            }
            Step step = tutorial_step_list[active_tutorial_step_idx];
            instruction_text.text = tutorial_step_list[active_tutorial_step_idx].instruction_text;
            Mask_Group mg = step.mg;
            set_all_anchor_points(mg);
        }
    }

    public bool is_click_in_wrong_place(Vector2Int tile_pos, int delay = 0)
    {
        bool pos_in_bounds = GameManager.is_position_in_bounds(tile_pos);
        bool is_it_hit = GameManager.tutorial_manager.did_raycast_hit_blocking_mask();
        //print("is it hit " + is_it_hit);
        if (is_it_hit || !pos_in_bounds)
        {
            backtrack_tutorial_step();
            return true; // if hit blocking mask stop
        }
        else
        {
            StartCoroutine(activate_next_tutorial_step(delay));
            return false;
        }
    }
}
