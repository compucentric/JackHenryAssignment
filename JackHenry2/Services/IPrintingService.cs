namespace JackHenry2.Services
{
    public interface IPrintingService
    {
        //Print last batch
        void PrintStatistics();
        //Print specific batch
        void PrintStatistics(string batchDateTimeStamp);
        //Print all batches 
        void PrintAllBatchInfo();
        void PrintRedditRequestLimits();
    }
}
