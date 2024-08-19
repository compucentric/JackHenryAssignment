namespace JackHenry2.Models
{
    public class Batch
    {
        public string BatchDateTimeStamp { get; }
        public Statistics Statistics { get; set; } = new Statistics();
        public Dictionary<string, Statistics> SubredditStatistics { get; set; } = new Dictionary<string, Statistics>();

        public Batch(string batchDateTimeStamp)
        {
            BatchDateTimeStamp = batchDateTimeStamp;
        }
    }
}