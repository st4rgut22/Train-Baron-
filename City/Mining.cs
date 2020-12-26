using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mining : Building
{
    public override Room spawn_room()
    {
        Room room = base.spawn_room();
        room.spawn_person();
        return room;
    }
}