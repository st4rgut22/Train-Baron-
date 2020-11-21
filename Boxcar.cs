using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Boxcar : MovingObject
{
    public Train train = null; // the train boxcar is attached to
    Vector3 idling_position; // boxcar's default position while waiting for turntable to arrive
    public int boxcar_id;
    public bool departing = false;
    public bool receive_train_order = true;

    private void Awake()
    {      
        base.Awake();
    }

    void Start()
    {
        base.Start();
        idling_position = new Vector3(-1, -1, -1);
        departing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!in_city || (in_city && receive_train_order)) // delay movement updates until train orders boxcar to depart
            base.Update();
    }

    public void initialize_boxcar(int id)
    {
        boxcar_id = id;
        speed_multiplier = 1.03f; // when boxcar is created go a little faster so it can keep up with train
    }

    public override void arrive_at_city()
    {
        base.arrive_at_city();
        //departing = true;
        //city.add_boxcar(gameObject);
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

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (in_city && train.is_pause) // waiting for turntable to arrive
        {
            print("begin dragging boxcar in city");
            base.OnBeginDrag(eventData);
            idling_position = transform.position; // if train departs before placing a boxcar in parking spot, then boxcar departs too
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        print("on drag");
        if (in_city && train.is_pause) // waiting for turntable to arrive
        {
            Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
            transform.position = world_position;
        } else
        {
            // TODO: abort drag, if the train is departing
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        print("on end drag");
        // destroy the boxcar if it is placed in a parking spot
        if (in_city && train.is_pause)
        {
            Vector2Int selected_tile = GameManager.get_selected_tile(Input.mousePosition);
            print("selected tile is " + selected_tile);
            bool parking_available = city.is_parking_spot_available(selected_tile);
            if (parking_available)
            {
                city.place_boxcar_tile(gameObject, selected_tile);
                GameManager.vehicle_manager.boxcar_fill_void(train, boxcar_id, idling_position); // move boxcars behind this one forward
                train.remove_boxcar(boxcar_id);
                Destroy(gameObject);
            }
            else
            {
                transform.position = idling_position;
            }
        }
    }
}
