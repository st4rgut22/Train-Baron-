using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxcar : MovingObject
{
    Train train = null; // the train boxcar is attached to
    int boxcar_id;
    bool departing = false;

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
        //if (!idling) // if in city, suspend regular update movement actions. resume when boxcar has completed departure (moving to city tile's edge)
            base.Update();
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
