using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyCamera : MonoBehaviour
{
    static DontDestroyCamera instance;
    public GameObject top_panel_go;
    public GameObject bottom_panel_go;
    public static bool is_tall_phone;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        set_camera_dist();

    }

    void set_camera_dist()
    {
        float aspect_ratio = (float)Screen.height / (float)Screen.width;
        print(aspect_ratio);
        if (aspect_ratio > .6f) // 10:16 aspect ratio
        {
            is_tall_phone = true;
            Vector3 camera_pos = GameManager.camera.transform.position;
            GameManager.camera.transform.position = new Vector3(camera_pos.x, camera_pos.y, -8.88f);
        }
        else
        {
            // hide black panels if 9:16 aspect ratio
            is_tall_phone = false;
            top_panel_go.SetActive(false);
            bottom_panel_go.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
