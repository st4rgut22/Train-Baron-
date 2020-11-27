public class Laboratory : Building
{
    public int vaccine_progress;

    public Laboratory(int id, string type) : base(id, type)
    {
        vaccine_progress = 0;
    }
}
