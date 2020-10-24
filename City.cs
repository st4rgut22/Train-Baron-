using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class City: MonoBehaviour
{
    // track city control as a function of supplies, troops, artillery
    Vector3Int tilemap_position;
    List<GameObject> train_list; // list of trains inside a city

    public Vector3Int get_location()
    {
        return tilemap_position;
    }

    public List<GameObject> get_train_list()
    {
        return train_list;
    }

    public void add_train_to_list(GameObject train)
    {
        train_list.Add(train);
    }

    public void remove_train_from_list(Train train)
    {
        // remove a train that has departed the city
        int id = train.get_id();
        int trains_removed = 0;
        for (int t = 0; t < train_list.Count; t++)
        {
            int train_id = train_list[t].GetComponent<Train>().get_id();
            if (train_id == id)
            {
                train_list.RemoveAt(t);
                trains_removed++;
            }
        }
        print("trains removed = " + trains_removed);
        //if (trains_removed != 1)
        //    throw new Exception("Incorrect number of trains removed :" + trains_removed);
    }


    public void set_location(Vector3Int position)
    {
        tilemap_position = position;
    }

    public void is_train_turn_on(bool state)
    {
        // hide or show trains depending on whether I'm in a shipyard view
        foreach (GameObject train in train_list)
        {
            train.GetComponent<SpriteRenderer>().enabled = state;
        }
    }

    private void Start()
    {
        // must be a Gameobject for Start() Update() to run
        train_list = new List<GameObject>();
    }

    private void Update()
    {
       
    }
}
