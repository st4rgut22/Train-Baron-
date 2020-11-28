using UnityEngine;

public class Residential : Building
{
    // Start is called before the first frame update

    private void Start()
    {
        spawn_person(offset_position, building_orientation);
    }
}
