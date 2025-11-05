using System.Text;
namespace pa_lab1;
static class Program
{
    const int strLength = 20;
    const int minNumber = 1;
    const int maxNumber = 1_000_000_000;
    const long fileSize = 100 * 1024 * 1024;
    const long chunkSize = 15 * 1024 * 1024;
    const string inputFileName = "input.txt";
    const string fileB = "fileB.txt";
    const string fileC = "fileC.txt";
    const string outputFileName = "output.txt";
    static Random random = new Random();
    static DateOnly startDate = new DateOnly(1970, 1, 1);
    
    static void Main(string[] args)
    {
        File.WriteAllText(fileB, string.Empty);
        File.WriteAllText(fileC, string.Empty);
        //File.WriteAllText(inputFileName, string.Empty);
        File.Delete(outputFileName);
        //WriteFile(inputFileName, fileSize);
        DirectMerge.SplitFile(inputFileName, fileB,fileC, 1);
        DirectMerge.MergeSort(fileB, fileC,inputFileName, outputFileName);
        /*ModifiedMerge.SplitFile(inputFileName, chunkSize);
        ModifiedMerge.SortChunks();
        ModifiedMerge.MergeChunks();*/
    }

    static string RandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[random.Next(chars.Length)]);
        }
        return sb.ToString();
    }
    static long RandomNumber(int min, int max)
    {
        return random.Next(min, max);
    }

    static DateOnly RandomDate(DateOnly startDate)
    {
        int range = DateOnly.FromDateTime(DateTime.Today).DayNumber - startDate.DayNumber;
        return startDate.AddDays(random.Next(range));
    }

    static void WriteFile(string fileName, long targetSizeBytes)
    {
        long writtenBytes = 0;
        using var writer = new StreamWriter(fileName, false);

        while (writtenBytes < targetSizeBytes)
        {
            string line = GenerateRecord();
            byte[] bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
            writer.WriteLine(line);
            writtenBytes += bytes.Length;
        }
    }

    static string GenerateRecord()
    {
        string randStr = RandomString(strLength);
        long randNum = RandomNumber(minNumber, maxNumber);
        DateOnly randDate = RandomDate(startDate);
        string record = $"{randNum} {randStr} {randDate}";
        return record;
    }
}