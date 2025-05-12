namespace Sync_Data_API.Models
{
    public class Detail
    {
        public int Saldo { get; set; }
        public int Hutang { get; set; }

    }
    public class PersonDetail : Person
    {
        public Detail? Detail { get; set; }
    }
}
