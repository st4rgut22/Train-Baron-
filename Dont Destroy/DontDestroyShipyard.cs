using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyShipyard : MonoBehaviour
{
    static DontDestroyShipyard instance;

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
}
