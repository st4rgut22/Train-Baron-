using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonManager : MonoBehaviour
{
    public static int reputation = 0; // review score out of 100
    public static int max_reputation = 100;
    public static int min_reputation = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void change_reputation(int reputation_change)
    {
        reputation += reputation_change;
        reputation = Mathf.Min(reputation, max_reputation);
        reputation = Mathf.Max(reputation, min_reputation);
        print("reputation of all cities is " + reputation);
    }
}
