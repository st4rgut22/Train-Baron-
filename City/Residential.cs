using UnityEngine;

public class Residential : Building
{
    // Start is called before the first frame update

    public override Room spawn_room()
    {
        Room room = base.spawn_room();
        room.spawn_person();
        return room;
    }

}
