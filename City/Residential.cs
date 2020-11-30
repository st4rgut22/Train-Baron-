using UnityEngine;

public class Residential : Building
{
    // Start is called before the first frame update

    public override Room spawn_room(int room_id)
    {
        Room room = base.spawn_room(room_id);
        room.spawn_person();
        //add_occupant_to_available_room(room.person_go);
        return room;
    }

}
