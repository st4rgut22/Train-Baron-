using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public float board_rotation;
    public float unload_rotation;
    public float tile_rotation;
    public GameObject door_go;
    public Sprite unload_sprite; // same sprites but different pivot points
    public Sprite board_sprite;
    public GameObject door_sprite_go;

    private void Awake()
    {
        door_sprite_go = transform.GetChild(0).gameObject;
    }

    public IEnumerator rotate() // when object is not active, it never finishes?
    {
        float start_angle = door_sprite_go.transform.eulerAngles.z; // remove offset incorporated into rotation calculations to align person in right direction
        float end_angle = start_angle + 90;
        float t_param = 1;
        while (t_param > 0)
        {
            t_param -= Time.deltaTime;
            float interp = 1.0f - t_param;
            float angle = Mathf.LerpAngle(start_angle, end_angle, interp); // interpolate from [0,1]
            door_sprite_go.transform.eulerAngles = new Vector3(0, 0, angle);
            print("angle is " + door_sprite_go.transform.eulerAngles);
            t_param -= Time.deltaTime * GameManager.speed/4;
            yield return new WaitForEndOfFrame();
        }
        print("ooo");
    }
}
