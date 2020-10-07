using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxcar : MovingObject
{
    Train train = null; // the train boxcar is attached to

    public void attach_to_train(GameObject train_object)
    {
        train = train_object.GetComponent<Train>();
        Vector3Int train_tile_position = train.tile_position;
        tile_position = new Vector3Int(train_tile_position.x, train_tile_position.y - 1, 0); // assume train is one unit above the boxcar. TODO: boxcars follow the track behind the train                              
        spawn_moving_object(tile_position, Orientation.North);
    }

    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
