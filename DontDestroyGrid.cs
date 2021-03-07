using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyGrid : MonoBehaviour
{
    public static DontDestroyGrid instance;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
