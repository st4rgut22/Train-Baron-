using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building
{
    public int building_id;
    public Person[] person_list;

    public Building(int building_id)
    {
        this.building_id = building_id;
    }

}
