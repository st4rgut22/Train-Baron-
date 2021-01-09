using UnityEngine;

public class Entrance : Building
{
    // Start is called before the first frame update

    public void Start()
    {
        base.Start();
    }

    public override Room spawn_room()
    {
        Room room = base.spawn_room();
        Person person = room.spawn_person();
        person.turn_on_sprite_renderer(false);
        return room;
    }
}
