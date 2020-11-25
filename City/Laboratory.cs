public class Laboratory : Building
{
    public int vaccine_progress;

    public Laboratory(int id) : base(id)
    {
        vaccine_progress = 0;
    }
}
