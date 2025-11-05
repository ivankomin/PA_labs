using System.Text;

public class ExternalMergeSort
{
    private const long MAX_CHUNK_SIZE = 150 * 1024 * 1024; // 100 MB
    private const int BUFFER_SIZE = 65536; // 64 KB буфер для читання/запису
    
    public record DataRecord(long Key, string Line)
    {
        public static DataRecord Parse(string line)
        {
            int spaceIndex = line.IndexOf(' ');
            long key = long.Parse(line.AsSpan(0, spaceIndex));
            return new DataRecord(key, line);
        }
    }

    // Розділення файлу на відсортовані чанки
    public static async Task SplitAndSortChunksAsync(string inputFile)
    {
        // Видалення старих чанків
        foreach (var file in Directory.GetFiles(".", "chunk_*.txt"))
            File.Delete(file);

        using var reader = new StreamReader(inputFile, Encoding.UTF8, true, BUFFER_SIZE);
        int chunkIndex = 0;
        var currentChunk = new List<DataRecord>();
        long currentSize = 0;

        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync();
            if (line == null) break;

            long lineSize = Encoding.UTF8.GetByteCount(line) + 2; // +2 для \r\n
            
            // Якщо додавання цього рядка перевищить ліміт, зберігаємо поточний чанк
            if (currentSize + lineSize > MAX_CHUNK_SIZE && currentChunk.Count > 0)
            {
                await SaveSortedChunkAsync(currentChunk, chunkIndex++);
                currentChunk.Clear();
                currentSize = 0;
            }

            currentChunk.Add(DataRecord.Parse(line));
            currentSize += lineSize;
        }

        // Зберігаємо останній чанк
        if (currentChunk.Count > 0)
        {
            await SaveSortedChunkAsync(currentChunk, chunkIndex);
        }
    }

    private static async Task SaveSortedChunkAsync(List<DataRecord> chunk, int index)
    {
        // Сортування в пам'яті (спадання за ключем)
        chunk.Sort((a, b) => b.Key.CompareTo(a.Key));

        string fileName = $"chunk_{index:D4}.txt";
        using var writer = new StreamWriter(fileName, false, Encoding.UTF8, BUFFER_SIZE);
        
        foreach (var record in chunk)
        {
            await writer.WriteLineAsync(record.Line);
        }
    }

    // Злиття двох файлів
    private static async Task MergeTwoFilesAsync(string fileA, string fileB, string output)
    {
        using var readerA = new StreamReader(fileA, Encoding.UTF8, true, BUFFER_SIZE);
        using var readerB = new StreamReader(fileB, Encoding.UTF8, true, BUFFER_SIZE);
        using var writer = new StreamWriter(output, false, Encoding.UTF8, BUFFER_SIZE);

        string? lineA = await readerA.ReadLineAsync();
        string? lineB = await readerB.ReadLineAsync();
        
        DataRecord? recordA = lineA != null ? DataRecord.Parse(lineA) : null;
        DataRecord? recordB = lineB != null ? DataRecord.Parse(lineB) : null;

        while (recordA != null && recordB != null)
        {
            if (recordA.Key >= recordB.Key)
            {
                await writer.WriteLineAsync(recordA.Line);
                lineA = await readerA.ReadLineAsync();
                recordA = lineA != null ? DataRecord.Parse(lineA) : null;
            }
            else
            {
                await writer.WriteLineAsync(recordB.Line);
                lineB = await readerB.ReadLineAsync();
                recordB = lineB != null ? DataRecord.Parse(lineB) : null;
            }
        }

        while (recordA != null)
        {
            await writer.WriteLineAsync(recordA.Line);
            lineA = await readerA.ReadLineAsync();
            recordA = lineA != null ? DataRecord.Parse(lineA) : null;
        }

        while (recordB != null)
        {
            await writer.WriteLineAsync(recordB.Line);
            lineB = await readerB.ReadLineAsync();
            recordB = lineB != null ? DataRecord.Parse(lineB) : null;
        }
    }

    // Багаторівневе злиття чанків
    public static async Task MergeChunksAsync(string outputFile = "output.txt")
    {
        var chunks = Directory.GetFiles(".", "chunk_*.txt")
            .OrderBy(f => f)
            .ToList();

        if (chunks.Count == 0)
        {
            Console.WriteLine("Немає чанків для злиття!");
            return;
        }

        if (chunks.Count == 1)
        {
            File.Move(chunks[0], outputFile, true);
            return;
        }

        int passNumber = 0;

        while (chunks.Count > 1)
        {
            passNumber++;
            var newChunks = new List<string>();

            // Паралельне злиття пар файлів
            var mergeTasks = new List<Task>();
            
            for (int i = 0; i < chunks.Count; i += 2)
            {
                int index = i;
                int mergeIndex = newChunks.Count;
                
                if (index + 1 >= chunks.Count)
                {
                    // Непарний файл - просто копіюємо
                    string leftover = $"merge_{passNumber:D2}_{mergeIndex:D4}.txt";
                    File.Copy(chunks[index], leftover, true);
                    newChunks.Add(leftover);
                }
                else
                {
                    string merged = $"merge_{passNumber:D2}_{mergeIndex:D4}.txt";
                    newChunks.Add(merged);
                    
                    // Обмежуємо кількість паралельних операцій
                    if (mergeTasks.Count >= 4)
                    {
                        await Task.WhenAny(mergeTasks);
                        mergeTasks.RemoveAll(t => t.IsCompleted);
                    }
                    
                    mergeTasks.Add(MergeTwoFilesAsync(chunks[index], chunks[index + 1], merged));
                }
            }

            await Task.WhenAll(mergeTasks);

            // Видалення старих чанків
            foreach (var chunk in chunks)
            {
                try { File.Delete(chunk); } catch { }
            }

            chunks = newChunks;
            Console.WriteLine($"Прохід {passNumber}: залишилось {chunks.Count} файлів");
        }

        // Фінальний файл
        if (chunks.Count == 1)
        {
            File.Move(chunks[0], outputFile, true);
        }
    }

    // Основна функція сортування
    public static async Task SortFileAsync(string inputFile, string outputFile = "output.txt")
    {
        Console.WriteLine("Початок сортування...");
        var startTime = DateTime.Now;

        Console.WriteLine("Етап 1: Розділення та сортування чанків...");
        await SplitAndSortChunksAsync(inputFile);

        Console.WriteLine("Етап 2: Злиття відсортованих чанків...");
        await MergeChunksAsync(outputFile);

        var elapsed = DateTime.Now - startTime;
        Console.WriteLine($"Сортування завершено за {elapsed.TotalMinutes:F2} хвилин");
        Console.WriteLine($"Результат збережено у файлі: {outputFile}");
    }
}

// Приклад використання
public class Program
{
    public static async Task Main(string[] args)
    {
        string inputFile = args.Length > 0 ? args[0] : "input.txt";
        string outputFile = args.Length > 1 ? args[1] : "output.txt";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Файл {inputFile} не знайдено!");
            return;
        }

        await ExternalMergeSort.SortFileAsync(inputFile, outputFile);
    }
}