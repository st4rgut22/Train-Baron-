using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grocery : Building
{
    public override Room spawn_room() //TODO: remove after testing!!! residential should only spawn people
    {
        Room room = base.spawn_room();
        room.spawn_person();
        return room;
    }
}
