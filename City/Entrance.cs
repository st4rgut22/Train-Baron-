using UnityEngine;

public class Entrance : Building
{
    // Start is called before the first frame update

    public void populate_room()
    {
        foreach (GameObject room_go in roomba)
        {
            Room room = room_go.GetComponent<Room>();
            if (room.person_go == null)
            {
                room.spawn_person();
            }
        }
    }

    public override Room spawn_room()
    {
        Room room = base.spawn_room();
        room.spawn_person();
        return room;
    }

}
