public class Residential : Building
{
    // Start is called before the first frame update
    public Person person;

    public Residential(int id) : base(id)
    {
        person = new Person();
    }
}
