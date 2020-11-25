public class Business: Building
{
    bool occupied;
    int pay_rate;

    // Start is called before the first frame update
    public Business(int id) : base(id)
    {
        occupied = false;
        pay_rate = 1;
    }

    public void pay_employee()
    {
        foreach (Person person in person_list)
        {
            person.change_wealth(pay_rate);
        }
    }
}
