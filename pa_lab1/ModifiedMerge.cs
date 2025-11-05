using System.Text;
public static class ModifiedMerge
{
    public static void SplitFile(string inputFile, long chunkSizeBytes)
    {
        string[] chunkFiles = Directory.GetFiles(".", "chunk_*.txt");
        foreach (var chunkFile in chunkFiles)
            File.Delete(chunkFile);
        using var reader = new StreamReader(inputFile);
        int fileIndex = 0;
        long writtenBytes = 0;
        StreamWriter? writer = null;

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (line == null) break;

            byte[] bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);

            if (writer == null || writtenBytes + bytes.Length > chunkSizeBytes)
            {
                writer?.Dispose();
                string chunkName = $"chunk_{fileIndex++:D2}.txt";
                writer = new StreamWriter(chunkName, false);
                writtenBytes = 0;
            }

            writer.WriteLine(line);
            writtenBytes += bytes.Length;
        }

        writer?.Dispose();
    }

    public static void SortChunks()
    {
        string[] chunkFiles = Directory.GetFiles(".", "chunk_*.txt");
        foreach (var chunkFile in chunkFiles)
        {
            List<string> lines = File.ReadAllLines(chunkFile).ToList();
            lines.Sort(
                (a, b) =>
                {
                    long keyA = long.Parse(a.Split(' ')[0]);
                    long keyB = long.Parse(b.Split(' ')[0]);
                    return keyB.CompareTo(keyA);
                }
            );
            File.WriteAllLines(chunkFile, lines);
        }
    }

    static void MergeTwoFiles(string fileA, string fileB, string output)
    {
        using var readerA = new StreamReader(fileA);
        using var readerB = new StreamReader(fileB);
        using var writer = new StreamWriter(output, false, Encoding.UTF8);

        string? lineA = readerA.ReadLine();
        string? lineB = readerB.ReadLine();

        while (lineA != null && lineB != null)
        {
            long keyA = long.Parse(lineA.Split(' ')[0]);
            long keyB = long.Parse(lineB.Split(' ')[0]);

            if (keyA >= keyB)
            {
                writer.WriteLine(lineA);
                lineA = readerA.ReadLine();
            }
            else
            {
                writer.WriteLine(lineB);
                lineB = readerB.ReadLine();
            }
        }

        while (lineA != null)
        {
            writer.WriteLine(lineA);
            lineA = readerA.ReadLine();
        }

        while (lineB != null)
        {
            writer.WriteLine(lineB);
            lineB = readerB.ReadLine();
        }
    }

    public static void MergeChunks()
    {
        var chunks = Directory.GetFiles(Directory.GetCurrentDirectory(), "chunk_*.txt").OrderBy(f => f).ToList();
        int pass = 0;

        while (chunks.Count > 1)
        {
            pass++;
            var newChunks = new List<string>();

            for (int i = 0; i < chunks.Count; i += 2)
            {
                if (i + 1 >= chunks.Count)
                {
                    string leftover = $"chunk_{newChunks.Count:D2}_pass{pass}.txt";
                    File.Copy(chunks[i], leftover, true);
                    newChunks.Add(leftover);
                    continue;
                }

                string mergedName = $"chunk_{newChunks.Count:D2}_pass{pass}.txt";
                MergeTwoFiles(chunks[i], chunks[i + 1], mergedName);
                newChunks.Add(mergedName);
            }

            chunks = newChunks;
        }
        if (chunks.Count == 1)
        {
            string finalFile = chunks[0];
            File.Move(finalFile, "output.txt", true);
        }

        var passChunkFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "chunk_*_pass*.txt").ToList();
        foreach (var chunkFile in passChunkFiles)
            File.Delete(chunkFile);
    }  
}