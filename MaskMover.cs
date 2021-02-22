using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskMover : MonoBehaviour
{

    public RectTransform rt1;
    public RectTransform rt2;
    public RectTransform rt3;
    public RectTransform rt4;
    public RectTransform rt5;
    public RectTransform rt6;

    public GameObject tutorial_go;
    public static List<Mask_Group> mask_group_list;

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
        public Mask_Group(Anchor_Pair ap1, Anchor_Pair ap2, Anchor_Pair ap3, Anchor_Pair ap4=null, Anchor_Pair ap5=null, Anchor_Pair ap6=null)
        {
            this.ap1 = ap1;
            this.ap2 = ap2;
            this.ap3 = ap3;
            this.ap4 = ap4;
            this.ap5 = ap5;
            this.ap6 = ap6;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mask_group_list = new List<Mask_Group>();

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
        Anchor_Pair buy_boxcar_ap_3 = new Anchor_Pair(0,0,1,.61413f);
        Anchor_Pair buy_boxcar_ap_4 = new Anchor_Pair(.31778f, .61413f, 1, .66913f);
        Mask_Group buy_boxcar_mg = new Mask_Group(buy_boxcar_ap_1, buy_boxcar_ap_2, buy_boxcar_ap_3, buy_boxcar_ap_4);

        Anchor_Pair buy_apartment_ap_1 = new Anchor_Pair(0,.10515f, 1, 1);
        Anchor_Pair buy_apartment_ap_2 = new Anchor_Pair(-.00078f,.49f,.294f,.10515f);
        Anchor_Pair buy_apartment_ap_3 = new Anchor_Pair(0,0,1,.049f);
        Anchor_Pair buy_apartment_ap_4 = new Anchor_Pair(.31771f,.049f, 1, .108f);
        Mask_Group buy_apartment_mg = new Mask_Group(buy_apartment_ap_1, buy_apartment_ap_2, buy_apartment_ap_3, buy_apartment_ap_4);

        Anchor_Pair buy_track_ap_1 = new Anchor_Pair(0,.38461f,.99829f,1.0043f);
        Anchor_Pair buy_track_ap_2 = new Anchor_Pair(0,.33146f,.29449f,.38461f);
        Anchor_Pair buy_track_ap_3 = new Anchor_Pair(0,0,1,.33146f);
        Anchor_Pair buy_track_ap_4 = new Anchor_Pair(.31782f,.33146f,1,.38461f);
        Mask_Group buy_track_mg = new Mask_Group(buy_track_ap_1, buy_track_ap_2, buy_track_ap_3, buy_track_ap_4);

        Anchor_Pair buy_item_ap_1 = new Anchor_Pair(-.003f,.17686f,1,1.00043f);
        Anchor_Pair buy_item_ap_2 = new Anchor_Pair(0,0,1,.06145f);
        Anchor_Pair buy_item_ap_3 = new Anchor_Pair(.9575f,.06145f,1,.17686f);
        Anchor_Pair buy_item_ap_4 = new Anchor_Pair(.9575f,.06145f,1,.17686f);
        Mask_Group buy_item_mg = new Mask_Group(buy_item_ap_1, buy_item_ap_2, buy_item_ap_3, buy_item_ap_4);

        Anchor_Pair start_drag_track_ap_1 = new Anchor_Pair(0,.13751f,1,1);
        Anchor_Pair start_drag_track_ap_2 = new Anchor_Pair(0,0,.58737f,.13751f);
        Anchor_Pair start_drag_track_ap_3 = new Anchor_Pair(.655f,0,1,.13751f);
        Mask_Group start_drag_track_mg = new Mask_Group(start_drag_track_ap_1, start_drag_track_ap_2, start_drag_track_ap_3);

        Anchor_Pair end_drag_track_ap_1 = new Anchor_Pair(0,.60231f,1,1);
        Anchor_Pair end_drag_track_ap_2 = new Anchor_Pair(.58344f,0,1,.60231f);
        Anchor_Pair end_drag_track_ap_3 = new Anchor_Pair(0,0,.53132f,.60231f);
        Anchor_Pair end_drag_track_ap_4 = new Anchor_Pair(.53132f,0,.58344f,.5f);
        Mask_Group end_drag_track_mg = new Mask_Group(end_drag_track_ap_1, end_drag_track_ap_2, end_drag_track_ap_3, end_drag_track_ap_4);

        Anchor_Pair start_drag_apartment_ap_1 = new Anchor_Pair(0,.13751f,1,1);
        Anchor_Pair start_drag_apartment_ap_2 = new Anchor_Pair(0,0,.258f,.13751f);
        Anchor_Pair start_drag_apartment_ap_3 = new Anchor_Pair(.329f,0,1,.13751f);
        Mask_Group start_drag_apartment_mg = new Mask_Group(start_drag_apartment_ap_1, start_drag_apartment_ap_2, start_drag_apartment_ap_3);

        Anchor_Pair end_drag_apartment_ap_1 = new Anchor_Pair(0,.60231f,1,1);
        Anchor_Pair end_drag_apartment_ap_2 = new Anchor_Pair(.52721f,0,1,.60231f);
        Anchor_Pair end_drag_apartment_ap_3 = new Anchor_Pair(0,0,.47337f,.60231f);
        Anchor_Pair end_drag_apartment_ap_4 = new Anchor_Pair(.47337f,0,.52721f,.5f);
        Mask_Group end_drag_apartment_mg = new Mask_Group(end_drag_apartment_ap_1, end_drag_apartment_ap_2, end_drag_apartment_ap_3, end_drag_apartment_ap_4);

        Anchor_Pair click_entrance_ap_1 = new Anchor_Pair(0,0,.58737f,1);
        Anchor_Pair click_entrance_ap_2 = new Anchor_Pair(.64747f,0,1,1);
        Anchor_Pair click_entrance_ap_3 = new Anchor_Pair(.58737f,0,.64747f,.5f);
        Anchor_Pair click_entrance_ap_4 = new Anchor_Pair(.58737f,.60172f,.64747f,1);
        Mask_Group click_entrance_mg = new Mask_Group(click_entrance_ap_1, click_entrance_ap_2, click_entrance_ap_3, click_entrance_ap_4);

        Anchor_Pair start_drag_train_ap_1 = new Anchor_Pair(0, 0, .86649f, .96022f);
        Anchor_Pair start_drag_train_ap_2 = new Anchor_Pair(.91222f, 0, 1, .96022f);
        Anchor_Pair start_drag_train_ap_3 = new Anchor_Pair(.86649f, 0, .91222f, .90931f);
        Anchor_Pair start_drag_train_ap_4 = new Anchor_Pair(0, .96022f, 1, 1);
        Mask_Group store_drag_train_mg = new Mask_Group(start_drag_train_ap_1, start_drag_train_ap_2, start_drag_train_ap_3, start_drag_train_ap_4);

        Anchor_Pair end_drag_train_ap_1 = new Anchor_Pair(0,.75544f,.59626f,1);
        Anchor_Pair end_drag_train_ap_2 = new Anchor_Pair(.64032f,0,1,.93087f);
        Anchor_Pair end_drag_train_ap_3 = new Anchor_Pair(0,0,.64032f,.75544f);
        Mask_Group end_drag_train_mg = new Mask_Group(end_drag_train_ap_1, end_drag_train_ap_2, end_drag_train_ap_3);

        Anchor_Pair start_drag_boxcar_1 = new Anchor_Pair(0,.117f,1,1);
        Anchor_Pair start_drag_boxcar_2 = new Anchor_Pair(.203f,.01192f,1,.117f);
        Anchor_Pair start_drag_boxcar_3 = new Anchor_Pair(0,.01192f,.145f,.117f);
        Anchor_Pair start_drag_boxcar_4 = new Anchor_Pair(0,0,1,.01192f);
        Mask_Group start_drag_store_mg = new Mask_Group(start_drag_boxcar_1, start_drag_boxcar_2, start_drag_boxcar_3, start_drag_boxcar_4);

        Anchor_Pair end_drag_boxcar_ap_1 = new Anchor_Pair(0,.75544f,.59726f,1);
        Anchor_Pair end_drag_boxcar_ap_2 = new Anchor_Pair(.64032f,0,1,.93087f);
        Anchor_Pair end_drag_boxcar_ap_3 = new Anchor_Pair(0,0,.64032f,.75544f);
        Mask_Group end_drag_boxcar_mg = new Mask_Group(end_drag_boxcar_ap_1, end_drag_boxcar_ap_2, end_drag_boxcar_ap_3);

        Anchor_Pair click_person_ap_1 = new Anchor_Pair(0,0,.64671f,1);
        Anchor_Pair click_person_ap_2 = new Anchor_Pair(.70482f,0,1,1);
        Anchor_Pair click_person_ap_3 = new Anchor_Pair(.64671f,0,.70482f,.81036f);
        Anchor_Pair click_person_ap_4 = new Anchor_Pair(.64671f,.917f,.70482f,1);
        Mask_Group click_person_mg = new Mask_Group(click_person_ap_1, click_person_ap_2, click_person_ap_3, click_person_ap_4);

        Anchor_Pair click_boxcar_ap_1 = new Anchor_Pair(0,0,.61929f,1);
        Anchor_Pair click_boxcar_ap_2 = new Anchor_Pair(.67124f,0,1,1);
        Anchor_Pair click_boxcar_ap_3 = new Anchor_Pair(.61929f,0,.67124f,.94836f);
        Anchor_Pair click_boxcar_ap_4 = new Anchor_Pair(.61929f,.98251f,.67124f,1);
        Mask_Group click_boxcar_mg = new Mask_Group(click_boxcar_ap_1, click_boxcar_ap_2, click_boxcar_ap_3, click_boxcar_ap_4);

        Anchor_Pair click_train_ap_1 = new Anchor_Pair(0,0,.611f,1);
        Anchor_Pair click_train_ap_2 = new Anchor_Pair(.62476f,0,1,1);
        Anchor_Pair click_train_ap_3 = new Anchor_Pair(.611f,0,.62476f,.86272f);
        Anchor_Pair click_train_ap_4 = new Anchor_Pair(.611f,.96451f,.62476f,1);
        Mask_Group click_train_mg = new Mask_Group(click_train_ap_1, click_train_ap_2, click_train_ap_3, click_train_ap_4);

        Anchor_Pair click_exit_west_ap_1 = new Anchor_Pair(0,.60349f,1,1);
        Anchor_Pair click_exit_west_ap_2 = new Anchor_Pair(0,0,1,.5f);
        Anchor_Pair click_exit_west_ap_3 = new Anchor_Pair(.356486f,0,1,1);
        Mask_Group click_exit_west_mg = new Mask_Group(click_exit_west_ap_1, click_exit_west_ap_2, click_exit_west_ap_3);

        Anchor_Pair exit_city_btn_ap_1 = new Anchor_Pair(-.0003f,.97868f,1,1.00043f);
        Anchor_Pair exit_city_btn_ap_2 = new Anchor_Pair(0,0,.95532f, .97868f);
        Anchor_Pair exit_city_btn_ap_3 = new Anchor_Pair(.95532f,0,1,.91723f);
        Anchor_Pair exit_city_btn_ap_4 = new Anchor_Pair(.99033f,.91723f,1,.97868f);
        Mask_Group exit_city_mg = new Mask_Group(exit_city_btn_ap_1, exit_city_btn_ap_2, exit_city_btn_ap_3, exit_city_btn_ap_4);

        Anchor_Pair click_apartment_ap_1 = new Anchor_Pair(0,0,.46724f,1);
        Anchor_Pair click_apartment_ap_2 = new Anchor_Pair(.52929f,0,1,1);
        Anchor_Pair click_apartment_ap_3 = new Anchor_Pair(.46724f,0,.52929f,.5f);
        Anchor_Pair click_apartment_ap_4 = new Anchor_Pair(.46724f,.60172f,.52929f,1);
        Mask_Group click_apartment_mg = new Mask_Group(click_apartment_ap_1, click_apartment_ap_2, click_apartment_ap_3, click_apartment_ap_4);

        Anchor_Pair click_boxcar_to_unload_ap_1 = new Anchor_Pair(0,0,.61929f,1);
        Anchor_Pair click_boxcar_to_unload_ap_2 = new Anchor_Pair(.67124f,0,1,1);
        Anchor_Pair click_boxcar_to_unload_ap_3 = new Anchor_Pair(.61929f,0,.67124f,.94936f);
        Anchor_Pair click_boxcar_to_unload_ap_4 = new Anchor_Pair(.61929f,.983f,.67124f,1);
        Mask_Group click_boxcar_to_unload_mg = new Mask_Group(click_boxcar_to_unload_ap_1, click_boxcar_to_unload_ap_2, click_boxcar_to_unload_ap_3, click_boxcar_to_unload_ap_4);

        Anchor_Pair click_room_to_unload_mask_1 = new Anchor_Pair(0,0,.64671f,1);
        Anchor_Pair click_room_to_unload_mask_2 = new Anchor_Pair(.70482f,0,1,1);
        Anchor_Pair click_room_to_unload_mask_3 = new Anchor_Pair(.64671f,0,.70482f,.81036f);
        Anchor_Pair click_room_to_unload_mask_4 = new Anchor_Pair(.64671f,.917f,.70482f,1);
        Mask_Group click_room_to_unload_mg = new Mask_Group(click_room_to_unload_mask_1, click_room_to_unload_mask_2, click_room_to_unload_mask_3, click_room_to_unload_mask_4);

        mask_group_list.Add(store_btn_mg);
        mask_group_list.Add(buy_train_mg);
        mask_group_list.Add(buy_boxcar_mg);
        mask_group_list.Add(buy_apartment_mg);
        mask_group_list.Add(buy_track_mg);
        mask_group_list.Add(buy_item_mg);
        mask_group_list.Add(start_drag_track_mg);
        mask_group_list.Add(end_drag_track_mg);
        mask_group_list.Add(start_drag_apartment_mg);
        mask_group_list.Add(end_drag_apartment_mg);
        mask_group_list.Add(click_entrance_mg);
        mask_group_list.Add(store_drag_train_mg);
        mask_group_list.Add(end_drag_train_mg);
        mask_group_list.Add(start_drag_store_mg);
        mask_group_list.Add(end_drag_boxcar_mg);
        mask_group_list.Add(click_person_mg);
        mask_group_list.Add(click_boxcar_mg);
        mask_group_list.Add(click_train_mg);
        mask_group_list.Add(click_exit_west_mg);
        mask_group_list.Add(exit_city_mg);
        mask_group_list.Add(click_apartment_mg);
        mask_group_list.Add(click_boxcar_to_unload_mg);
        mask_group_list.Add(click_room_to_unload_mg);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
