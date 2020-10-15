using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainDisplay : MonoBehaviour
{
    Train train;
    VehicleManager vehicle_manager;
    Vector3Int spawn_location;
    Button add_btn;
    Button sub_btn;
    Button go_btn;
    Text boxcar_count_text;
    City city;

    private void Awake()
    {
        vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
        add_btn = transform.Find("Add Button").GetComponent<Button>();
        sub_btn = transform.Find("Minus Button").GetComponent<Button>();
        go_btn = transform.Find("train background").Find("Go Btn").GetComponent<Button>();
        add_btn.onClick.AddListener(add_boxcar);
        sub_btn.onClick.AddListener(subtract_boxcar);
        go_btn.onClick.AddListener(depart_station);
        boxcar_count_text = transform.Find("boxcar background").Find("boxcar").Find("quantity").GetComponent<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        print("testing");
    }

    public void initialize_boxcar_text(int boxcar_count)
    {
        boxcar_count_text.text = boxcar_count.ToString();
    }

    public void set_train(Train train)
    {
        this.train = train;
    }

    public void set_city(City city)
    {
        this.city = city;
        spawn_location = city.get_location();
    }

    void add_boxcar()
    {
        GameObject add_btn = GameObject.Find("Add Button");
        Text boxcar_count = add_btn.GetComponentInChildren<Text>();
        vehicle_manager.create_boxcar(spawn_location);
        boxcar_count_text.text = train.get_boxcar_id().ToString(); // update number of boxcars
    }

    void subtract_boxcar()
    {
        GameObject sub_btn = GameObject.Find("Minus Button");
        Text boxcar_count = sub_btn.GetComponentInChildren<Text>();
        vehicle_manager.remove_boxcar(train);
        boxcar_count_text.text = train.get_boxcar_id().ToString(); // update number of boxcars
    }

    void depart_station()
    {
        print("departing station");
        train.change_motion();
        vehicle_manager.spawn_moving_object(train);
        city.remove_train_from_list(train);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
