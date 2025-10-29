using System.Text;
namespace pa_lab1;
static class Program
{
    const int strLength = 20;
    const int minNumber = 1;
    const int maxNumber = 1_000_000_000;
    const long fileSize = 100 * 1024 * 1024;
    const long chunkSize = 15 * 1024 * 1024;
    const int bufferSize = 8192;
    const string inputFileName = "input.txt";
    const string fileB = "fileB.txt";
    const string fileC = "fileC.txt";
    const string outputFileName = "output.txt";
    static Random random = new Random();
    static DateOnly startDate = new DateOnly(1970, 1, 1);
    
    static void Main(string[] args)
    {
        WriteFile(inputFileName, fileSize);
        SplitFile(inputFileName, fileB,fileC, 1);
        MergeSort(fileB, fileC, outputFileName);
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
        using var writer = new StreamWriter(fileName, false, Encoding.UTF8, bufferSize);

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

    static void SplitFile(string input, string fileB, string fileC, int seriesLen)
    {
        using var reader = new StreamReader(input);
        using var writerB = new StreamWriter(fileB, false, Encoding.UTF8);
        using var writerC = new StreamWriter(fileC, false, Encoding.UTF8);

        bool toB = true;
        while (!reader.EndOfStream)
        {
            for (int i = 0; i < seriesLen && !reader.EndOfStream; i++)
            {
                string? line = reader.ReadLine();
                if (toB) writerB.WriteLine(line);
                else writerC.WriteLine(line);
            }
            toB = !toB;
        }
    }

    static List<string> ReadSeries(StreamReader reader, int count)
    {
        var series = new List<string>(count);
        for (int i = 0; i < count && !reader.EndOfStream; i++)
        {
            series.Add(reader.ReadLine()!);
        }
        return series;
    }
    
    static List<string> MergeSeries(List<string> a, List<string> b)
    {
        int i = 0, j = 0;
        var result = new List<string>(a.Count + b.Count);

        while (i < a.Count && j < b.Count)
        {
            long keyA = long.Parse(a[i].Split(' ')[0]);
            long keyB = long.Parse(b[j].Split(' ')[0]);

            if (keyA > keyB) 
                result.Add(a[i++]);
            else
                result.Add(b[j++]);
        }

        while (i < a.Count) result.Add(a[i++]);
        while (j < b.Count) result.Add(b[j++]);

        return result;
    }

    static bool MergeStep(string fileB, string fileC, string outputFile, int seriesLen)
    {
        using var readerB = new StreamReader(fileB);
        using var readerC = new StreamReader(fileC);
        using var writer = new StreamWriter(outputFile, false, Encoding.UTF8);

        bool anyMerged = false;
        while (!readerB.EndOfStream || !readerC.EndOfStream)
        {
            var seriesB = ReadSeries(readerB, seriesLen);
            var seriesC = ReadSeries(readerC, seriesLen);

            if (seriesB.Count == 0 && seriesC.Count == 0)
                break;

            anyMerged = true;

            var merged = MergeSeries(seriesB, seriesC);
            foreach (var line in merged)
                writer.WriteLine(line);
        }
        return !anyMerged;
    }
    
    static void MergeSort(string fileB, string fileC, string output)
    {
        long totalLines = File.ReadLines(inputFileName).LongCount();
        int seriesLen = 1;

        SplitFile(inputFileName, fileB, fileC, seriesLen);

        while (seriesLen < totalLines)
        {
            MergeStep(fileB, fileC, outputFileName, seriesLen);
            SplitFile(outputFileName, fileB, fileC, seriesLen * 2);
            seriesLen *= 2;
        }
    }
}