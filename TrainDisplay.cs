using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainDisplay : MenuManager
{ // inherit from MenuManager to get drag logic
    Train train;
    Vector3Int spawn_location;
    Button add_btn;
    Button sub_btn;
    Text boxcar_count_text;

    private void Awake()
    {
        //vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
        //camera = GameObject.Find("Camera").GetComponent<Camera>();
        //add_btn = transform.Find("Add Button").GetComponent<Button>();
        //sub_btn = transform.Find("Minus Button").GetComponent<Button>();
        //add_btn.onClick.AddListener(add_boxcar);
        //sub_btn.onClick.AddListener(subtract_boxcar);
        //boxcar_count_text = transform.Find("boxcar background").Find("boxcar").Find("quantity").GetComponent<Text>();
        //initialize_train_menu_manager();
    }

    // Start is called before the first frame update
    void Start()
    {
   
    }

    //public void initialize_boxcar_text(int boxcar_count)
    //{
    //    boxcar_count_text.text = boxcar_count.ToString();
    //}

    //public void set_train(GameObject train_thing)
    //{
    //    train_object = train_thing;
    //    train = train_object.GetComponent<Train>();
    //}

    //public void set_spawn_location(GameObject city_object)
    //{
    //    spawn_location = city_object.GetComponent<City>().get_location();
    //}

    //void add_boxcar()
    //{
    //    GameObject add_btn = GameObject.Find("Add Button");
    //    Text boxcar_count = add_btn.GetComponentInChildren<Text>();
    //    vehicle_manager.create_boxcar(spawn_location, train);
    //    boxcar_count_text.text = train.get_boxcar_id().ToString(); // update number of boxcars
    //}

    //void subtract_boxcar()
    //{
    //    GameObject sub_btn = GameObject.Find("Minus Button");
    //    Text boxcar_count = sub_btn.GetComponentInChildren<Text>();
    //    vehicle_manager.remove_boxcar(train);
    //    boxcar_count_text.text = train.get_boxcar_id().ToString(); // update number of boxcars
    //}

    // Update is called once per frame
    void Update()
    {

    }
}
