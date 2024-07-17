using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static Dictionary<string, ulong> initialCRC64s = new Dictionary<string, ulong>();
    static string baseDirectory = @"C:\2024\g\32\StandAloneComplexGen1\memory"; // Change this to your code directory path

    // Add or remove file extensions as needed
    static List<string> fileExtensionsToCheck = new List<string>
    {
        ".cs", ".js", ".html", ".css", ".ts", ".json", ".xml", ".config"
    };

    static void Main(string[] args)
    {
        Console.WriteLine("Initial CRC64 calculation:");
        CalculateAndStoreCRC64s();

        Console.WriteLine("\nPress any key after making changes to the codebase...");
        Console.ReadKey();

        Console.WriteLine("\nRecalculating CRC64 and comparing:");
        CompareAndShowChanges();
    }

    static void CalculateAndStoreCRC64s()
    {
        foreach (string file in EnumerateFilesWithExtensions(baseDirectory, fileExtensionsToCheck))
        {
            ulong crc64 = CalculateCRC64(file);
            initialCRC64s[file] = crc64;
            Console.WriteLine($"Calculated CRC64 for {file}: {crc64}");
        }
    }

    static void CompareAndShowChanges()
    {
        int changedFiles = 0;
        int totalFiles = 0;

        foreach (string file in EnumerateFilesWithExtensions(baseDirectory, fileExtensionsToCheck))
        {
            totalFiles++;
            ulong newCRC64 = CalculateCRC64(file);

            if (!initialCRC64s.TryGetValue(file, out ulong oldCRC64) || oldCRC64 != newCRC64)
            {
                changedFiles++;
                Console.WriteLine($"Changed file: {file}");
            }
        }

        double percentageChanged = (double)changedFiles / totalFiles * 100;
        Console.WriteLine($"\nPercentage of codebase changed: {percentageChanged:F2}%");
    }

    static IEnumerable<string> EnumerateFilesWithExtensions(string directory, List<string> extensions)
    {
        return Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(file => extensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
    }

    static ulong CalculateCRC64(string filePath)
    {
        using (FileStream fs = File.Open(filePath, FileMode.Open))
        using (BufferedStream bs = new BufferedStream(fs))
        {
            byte[] buffer = new byte[4096];
            ulong crc = 0;
            int bytesRead;
            while ((bytesRead = bs.Read(buffer, 0, buffer.Length)) > 0)
            {
                crc = Crc64.Compute(buffer, 0, bytesRead, crc);
            }
            return crc;
        }
    }
}

// CRC64 implementation (ECMA-182 polynomial)
public static class Crc64
{
    private static readonly ulong[] Table = new ulong[256];

    static Crc64()
    {
        const ulong poly = 0xC96C5795D7870F42;
        for (int i = 0; i < 256; ++i)
        {
            ulong crc = (ulong)i;
            for (int j = 0; j < 8; ++j)
            {
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ poly;
                else
                    crc >>= 1;
            }
            Table[i] = crc;
        }
    }

    public static ulong Compute(byte[] buffer, int offset, int length, ulong initial = 0)
    {
        ulong crc = initial;
        for (int i = offset; i < offset + length; ++i)
        {
            crc = Table[(byte)(crc ^ buffer[i])] ^ (crc >> 8);
        }
        return crc;
    }
}
