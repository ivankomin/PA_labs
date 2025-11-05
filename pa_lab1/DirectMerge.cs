using System.Text;
public static class DirectMerge
{
    public static void SplitFile(string input, string fileB, string fileC, int seriesLen)
    {
        using var reader = new StreamReader(input);
        using var writerB = new StreamWriter(fileB, false);
        using var writerC = new StreamWriter(fileC, false);

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

    public static void MergeSort(string fileB, string fileC, string inputFileName, string outputFileName)
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

    static bool MergeStep(string fileB, string fileC, string outputFile, int seriesLen)
    {
        using var readerB = new StreamReader(fileB);
        using var readerC = new StreamReader(fileC);
        using var writer = new StreamWriter(outputFile, false);

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
}