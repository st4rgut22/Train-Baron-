public class Person
{
    public int health;
    public int wealth;

    public Person()
    {
        health = 100;
        wealth = 0;
    }

    public bool change_wealth(int delta)
    {
        if (wealth + delta >= 0)
        {
            wealth += delta;
            return true;
        } else
        {
            return false;
        }
    }

    public bool change_health(int delta)
    {
        if (health + delta >= 0)
        {
            health += delta;
            return true;
        }
        else
        {
            return false; // die
        }
    }
}
