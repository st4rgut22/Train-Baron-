using UnityEngine;

public class Business: Building
{
    int pay_rate;

    public void pay_employee()
    {
        for (int i = 0; i < person_grid.Length; i++)
        {
            GameObject person_object = person_grid[i];
            person_object.GetComponent<Person>().change_wealth(pay_rate);
        }
    }
}
