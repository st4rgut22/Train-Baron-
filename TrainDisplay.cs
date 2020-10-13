using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainDisplay : MonoBehaviour
{
    Train train;
    VehicleManager vehicle_manager;
    Vector3Int spawn_location;

    // Start is called before the first frame update
    void Start()
    {
        vehicle_manager = GameObject.Find("VehicleManager").GetComponent<VehicleManager>();
    }

    public void set_train(Train train)
    {
        this.train = train;
    }

    public void set_location(Vector3Int city_location)
    {
        spawn_location = city_location;
    }

    // Update is called once per frame
    void Update()
    {
        bool add_button_press = Input.GetButtonDown("Add Button");
        bool minus_button_press = Input.GetButtonDown("Minus Button");
        if (add_button_press)
        {
            GameObject add_btn = GameObject.Find("Add Button");
            Text boxcar_count = add_btn.GetComponentInChildren<Text>();
            vehicle_manager.create_boxcar(spawn_location);
            boxcar_count.text = train.get_boxcar_id().ToString(); // update number of boxcars
        }
        if (minus_button_press)
        {
            GameObject sub_btn = GameObject.Find("Minus Button");
            Text boxcar_count = sub_btn.GetComponentInChildren<Text>();
            vehicle_manager.remove_boxcar(train);
            boxcar_count.text = train.get_boxcar_id().ToString(); // update number of boxcars
        }
    }
}
