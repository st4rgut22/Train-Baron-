using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building
{
    public int building_id;
    public Person[] person_list;
    public string building_type;

    public Building(int building_id, string building_type)
    {
        this.building_id = building_id;
        this.building_type = building_type;
    }

}
