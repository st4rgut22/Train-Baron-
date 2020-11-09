using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxcar : MovingObject
{
    Train train = null; // the train boxcar is attached to
    public int boxcar_id;
    bool departing = false;
    public bool receive_train_order = true;

    private void Awake()
    {      
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (!in_city || (in_city && receive_train_order)) // delay movement updates until train orders boxcar to depart
            base.Update();
    }

    public override void arrive_at_city()
    {
        base.arrive_at_city();
        city.add_boxcar(gameObject);
    }

    public void set_boxcar_id(int id)
    {
        boxcar_id = id;
    }

    public RouteManager.Orientation get_orientation()
    {
        return orientation;
    }

    public void set_depart_status(bool status)
    {
        in_tile = true;
        departing = status;
    }

    public bool get_depart_status()
    {
        return departing;
    }

    public void attach_to_train(Train train)
    {
        this.train = train;
    }
}
