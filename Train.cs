using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Train : MovingObject
{
    List<GameObject> boxcar_squad = new List<GameObject>(); // boxcars attached to this train

    public void attach_boxcar(GameObject boxcar)
    {
        boxcar_squad.Add(boxcar);
    }

    public void change_motion()
    {
        in_motion = !in_motion;
        for (int i = 0; i < boxcar_squad.Count; i++)
        {
            Boxcar car = boxcar_squad[i].GetComponent<Boxcar>();
            car.in_motion = in_motion;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        tile_position = new Vector3Int(0, 0, 0);
        base.Start(); // train instantiated bottom left
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
