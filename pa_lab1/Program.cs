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
    const string outputFileName = "output.txt";
    static Random random = new Random();
    static DateOnly startDate = new DateOnly(1970, 1, 1);
    
    static void Main(string[] args)
    {
        WriteFile(inputFileName, fileSize);
        SplitFile(inputFileName, chunkSize);
        SortChunks();
        MergeChunks(outputFileName);
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

    static void SplitFile(string inputFile, long chunkSize)
    {
        
        using var reader = new StreamReader(inputFile, Encoding.UTF8, true, bufferSize);
        int fileIndex = 0;
        long currentChunkSize = 0;

        StreamWriter? writer = null;
        
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
            
            if (writer == null || currentChunkSize + bytes.Length > chunkSize)
            {
                writer?.Dispose();
                writer = new StreamWriter($"chunk_{fileIndex++}.txt", false, Encoding.UTF8, bufferSize);
                currentChunkSize = 0;
            }
            writer.WriteLine(line);
            currentChunkSize += bytes.Length;
        }
        writer?.Dispose();
    }

    static void SortChunks()
    {
        string[] chunks = Directory.GetFiles("C:\\Users\\ivank\\projects\\pa\\pa_lab1\\pa_lab1\\bin\\Debug\\net9.0", "chunk_*.txt");
        foreach (var chunk in chunks)
        {
            var lines = File.ReadAllLines(chunk).ToList();
            lines.Sort((a, b) =>
            {
                long keyA = long.Parse(a.Split(' ')[0]);
                long keyB = long.Parse(b.Split(' ')[0]);
                return keyB.CompareTo(keyA);
            });
            File.WriteAllLines(chunk, lines);
        }
    }

    static void MergeChunks(string outputPath)
    {
        var chunkPaths = Directory.GetFiles("C:\\Users\\ivank\\projects\\pa\\pa_lab1\\pa_lab1\\bin\\Debug\\net9.0", "chunk_*.txt");
        var readers = chunkPaths.Select(path => new StreamReader(path, Encoding.UTF8, true, bufferSize)).ToList();
        using var writer = new StreamWriter(outputPath, false, Encoding.UTF8, bufferSize);
        var heap = new PriorityQueue<(int Index, string Line), long>();

        void EnqueueNextLine(int index)
        {
            var reader = readers[index];
            if (!reader.EndOfStream)
            {
                string line = reader.ReadLine()!;
                long key = long.Parse(line.Split(' ')[0]);
                heap.Enqueue((index, line), -key); 
            }
        }
        
        for (int i = 0; i < readers.Count; i++)
        {
            EnqueueNextLine(i);
        }
        while (heap.Count > 0)
        {
            var (index, line) = heap.Dequeue();
            writer.WriteLine(line);
            EnqueueNextLine(index);
        }
        foreach (var r in readers)
            r.Dispose();
    }
}