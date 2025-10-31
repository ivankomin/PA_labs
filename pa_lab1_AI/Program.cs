using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class TwoWayExternalMergeSort
{
    private const int BUFFER_SIZE = 128 * 1024; // 128KB буфер для I/O
    private const long MAX_CHUNK_SIZE = 150 * 1024 * 1024; // 150MB для внутрішнього сортування
    
    public readonly struct DataRecord
    {
        public readonly long Key;
        public readonly string Line;
        
        public DataRecord(long key, string line)
        {
            Key = key;
            Line = line;
        }
    }

    // Оптимізований парсинг ключа
    private static long ExtractKey(ReadOnlySpan<char> line)
    {
        int spaceIndex = line.IndexOf(' ');
        if (spaceIndex == -1) return 0;
        return long.Parse(line.Slice(0, spaceIndex));
    }

    // Крок 1: Розбиття на відсортовані серії (chunk'и)
    public static int CreateInitialRuns(string inputFile)
    {
        Console.WriteLine("📝 Створення початкових відсортованих серій...");
        
        // Видалення старих файлів
        foreach (var file in Directory.GetFiles(".", "run_*.txt"))
            File.Delete(file);

        using var reader = new StreamReader(inputFile, Encoding.UTF8, true, BUFFER_SIZE);
        
        int runIndex = 0;
        var records = new List<DataRecord>(1_500_000);
        long currentSize = 0;

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (line == null) break;

            long key = ExtractKey(line.AsSpan());
            int lineSize = Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;

            if (currentSize + lineSize > MAX_CHUNK_SIZE && records.Count > 0)
            {
                WriteSortedRun(records, runIndex++);
                records.Clear();
                currentSize = 0;
            }

            records.Add(new DataRecord(key, line));
            currentSize += lineSize;
        }

        if (records.Count > 0)
        {
            WriteSortedRun(records, runIndex++);
        }

        Console.WriteLine($"   ✅ Створено {runIndex} початкових серій");
        return runIndex;
    }

    private static void WriteSortedRun(List<DataRecord> records, int runIndex)
    {
        // Внутрішнє сортування (спадання)
        records.Sort((a, b) => b.Key.CompareTo(a.Key));

        string runName = $"run_{runIndex:D4}.txt";
        using var writer = new StreamWriter(runName, false, Encoding.UTF8, BUFFER_SIZE);
        
        foreach (var record in records)
        {
            writer.WriteLine(record.Line);
        }
    }

    // Крок 2: Розподіл серій між файлами B і C
    private static void DistributeRuns(List<string> runs, string fileB, string fileC)
    {
        using var writerB = new StreamWriter(fileB, false, Encoding.UTF8, BUFFER_SIZE);
        using var writerC = new StreamWriter(fileC, false, Encoding.UTF8, BUFFER_SIZE);

        for (int i = 0; i < runs.Count; i++)
        {
            using var reader = new StreamReader(runs[i], Encoding.UTF8, true, BUFFER_SIZE);
            var writer = (i % 2 == 0) ? writerB : writerC;
            
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                writer.WriteLine(line);
            }
            
            // Маркер кінця серії (пустий рядок)
            writer.WriteLine();
        }
    }

    // Крок 3: Злиття двох серій в одну
    private static bool MergeTwoRuns(StreamReader readerB, StreamReader readerC, StreamWriter writer, ref long seriesLength)
    {
        string? lineB = readerB.ReadLine();
        string? lineC = readerC.ReadLine();
        
        // Перевірка на кінець файлів або пусті маркери
        if (string.IsNullOrEmpty(lineB) && string.IsNullOrEmpty(lineC))
            return false;
        
        bool hasDataB = !string.IsNullOrEmpty(lineB);
        bool hasDataC = !string.IsNullOrEmpty(lineC);
        
        if (!hasDataB && !hasDataC)
            return false;

        long keyB = hasDataB ? ExtractKey(lineB.AsSpan()) : long.MinValue;
        long keyC = hasDataC ? ExtractKey(lineC.AsSpan()) : long.MinValue;
        
        long count = 0;

        while (hasDataB && hasDataC)
        {
            if (keyB >= keyC)
            {
                writer.WriteLine(lineB);
                count++;
                lineB = readerB.ReadLine();
                
                if (string.IsNullOrEmpty(lineB))
                {
                    hasDataB = false;
                }
                else
                {
                    keyB = ExtractKey(lineB.AsSpan());
                }
            }
            else
            {
                writer.WriteLine(lineC);
                count++;
                lineC = readerC.ReadLine();
                
                if (string.IsNullOrEmpty(lineC))
                {
                    hasDataC = false;
                }
                else
                {
                    keyC = ExtractKey(lineC.AsSpan());
                }
            }
        }

        // Дописуємо залишок з B
        while (hasDataB)
        {
            writer.WriteLine(lineB);
            count++;
            lineB = readerB.ReadLine();
            hasDataB = !string.IsNullOrEmpty(lineB);
        }

        // Дописуємо залишок з C
        while (hasDataC)
        {
            writer.WriteLine(lineC);
            count++;
            lineC = readerC.ReadLine();
            hasDataC = !string.IsNullOrEmpty(lineC);
        }

        seriesLength = count;
        return count > 0;
    }

    // Крок 4: Один прохід 2-way merge (B + C → A)
    private static long MergePass(string fileB, string fileC, string fileA, long expectedSeriesLength)
    {
        using var readerB = new StreamReader(fileB, Encoding.UTF8, true, BUFFER_SIZE);
        using var readerC = new StreamReader(fileC, Encoding.UTF8, true, BUFFER_SIZE);
        using var writerA = new StreamWriter(fileA, false, Encoding.UTF8, BUFFER_SIZE);

        long mergedSeriesCount = 0;
        long actualSeriesLength = 0;

        while (MergeTwoRuns(readerB, readerC, writerA, ref actualSeriesLength))
        {
            mergedSeriesCount++;
            writerA.WriteLine(); // Маркер кінця об'єднаної серії
        }

        return mergedSeriesCount;
    }

    // Основний цикл 2-way external merge sort
    public static void TwoWayMergeSort(string outputFile = "output.txt")
    {
        Console.WriteLine("\n🔄 Початок 2-way злиття серій...");
        
        var runs = Directory.GetFiles(".", "run_*.txt").OrderBy(f => f).ToList();
        
        if (runs.Count == 0)
        {
            Console.WriteLine("❌ Немає серій для злиття!");
            return;
        }

        if (runs.Count == 1)
        {
            File.Move(runs[0], outputFile, true);
            Console.WriteLine("✅ Лише одна серія - сортування завершено!");
            return;
        }

        string fileA = "temp_A.txt";
        string fileB = "temp_B.txt";
        string fileC = "temp_C.txt";

        // Початковий розподіл серій
        Console.WriteLine($"   📂 Розподіл {runs.Count} серій між B і C...");
        DistributeRuns(runs, fileB, fileC);
        
        // Видаляємо початкові run файли
        foreach (var run in runs)
            File.Delete(run);

        int pass = 0;
        long seriesCount = (runs.Count + 1) / 2; // Кількість серій після злиття
        long currentSeriesLength = 1;

        // Продовжуємо доки не залишиться одна серія
        while (seriesCount > 1 || !File.Exists(fileA))
        {
            pass++;
            Console.WriteLine($"   🔄 Прохід {pass}: Злиття серій довжиною ~{currentSeriesLength * 2}...");

            if (pass % 2 == 1)
            {
                // B + C → A
                seriesCount = MergePass(fileB, fileC, fileA, currentSeriesLength);
                Console.WriteLine($"      ✓ Створено {seriesCount} серій у файлі A");
                
                if (seriesCount == 1)
                {
                    File.Move(fileA, outputFile, true);
                    break;
                }
                
                // Розподіляємо A → B, C
                DistributeRuns(new List<string> { fileA }, fileB, fileC);
            }
            else
            {
                // A → B, C (розподіл)
                // потім B + C → A
                seriesCount = MergePass(fileB, fileC, fileA, currentSeriesLength);
                Console.WriteLine($"      ✓ Створено {seriesCount} серій у файлі A");
                
                if (seriesCount == 1)
                {
                    File.Move(fileA, outputFile, true);
                    break;
                }
                
                DistributeRuns(new List<string> { fileA }, fileB, fileC);
            }
            
            currentSeriesLength *= 2;
        }

        // Очищення тимчасових файлів
        CleanupTempFiles();
        Console.WriteLine("   ✅ Тимчасові файли видалено");
    }

    private static void CleanupTempFiles()
    {
        foreach (var file in new[] { "temp_A.txt", "temp_B.txt", "temp_C.txt" })
        {
            try { if (File.Exists(file)) File.Delete(file); } catch { }
        }
        
        foreach (var file in Directory.GetFiles(".", "run_*.txt"))
        {
            try { File.Delete(file); } catch { }
        }
    }

    public static void Sort(string inputFile, string outputFile = "output.txt")
    {
        Console.WriteLine("╔════════════════════════════════════════════╗");
        Console.WriteLine("║   2-WAY EXTERNAL MERGE SORT               ║");
        Console.WriteLine("╚════════════════════════════════════════════╝");
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        var fileInfo = new FileInfo(inputFile);
        Console.WriteLine($"\n📁 Вхідний файл: {inputFile}");
        Console.WriteLine($"📊 Розмір: {fileInfo.Length / (1024.0 * 1024.0):F2} MB");
        Console.WriteLine($"💾 Розмір серії: {MAX_CHUNK_SIZE / (1024.0 * 1024.0):F0} MB\n");
        
        // Крок 1: Створення початкових відсортованих серій
        var step1 = System.Diagnostics.Stopwatch.StartNew();
        int runCount = CreateInitialRuns(inputFile);
        step1.Stop();
        Console.WriteLine($"   ⏱️  Час: {step1.Elapsed.TotalSeconds:F2} сек\n");
        
        // Крок 2: 2-way merge sort
        var step2 = System.Diagnostics.Stopwatch.StartNew();
        TwoWayMergeSort(outputFile);
        step2.Stop();
        Console.WriteLine($"   ⏱️  Час: {step2.Elapsed.TotalSeconds:F2} сек\n");
        
        sw.Stop();
        
        Console.WriteLine("╔════════════════════════════════════════════╗");
        Console.WriteLine($"║  ✨ ЗАВЕРШЕНО ЗА {sw.Elapsed.TotalMinutes:F2} ХВИЛИН       ");
        Console.WriteLine($"║     ({sw.Elapsed.TotalSeconds:F1} секунд)");
        Console.WriteLine("╚════════════════════════════════════════════╝");
        
        var outputInfo = new FileInfo(outputFile);
        Console.WriteLine($"\n📄 Результат: {outputFile}");
        Console.WriteLine($"📊 Розмір: {outputInfo.Length / (1024.0 * 1024.0):F2} MB");
        Console.WriteLine($"⚡ Швидкість: {fileInfo.Length / (1024.0 * 1024.0) / sw.Elapsed.TotalSeconds:F2} MB/сек");
    }
}

class Program
{
    static void Main(string[] args)
    {
        string inputFile = args.Length > 0 ? args[0] : "input.txt";
        string outputFile = args.Length > 1 ? args[1] : "output.txt";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"❌ Файл {inputFile} не знайдено!");
            return;
        }

        try
        {
            TwoWayExternalMergeSort.Sort(inputFile, outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Помилка: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}