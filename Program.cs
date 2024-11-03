using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Diagnostics;

class Program
{
    static readonly char[] chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    static List<string> md5Hashes = new List<string>();
    static List<string> sha256Hashes = new List<string>();

    static void Main(string[] args)
    {
        string filePath = @"..\..\..\Passwords.txt"; //файл в корневой директории проекта
        
        ReadHashesFromFile(filePath);

        while (true)
        {

            Console.WriteLine("Введите режим, в котором вы хотите работать: \n" +
                                "1 - однопоточный \n" +
                                "2 - многопоточный");
            
            string n = Console.ReadLine();
            Console.WriteLine();

            Stopwatch stopwatch = Stopwatch.StartNew();
            
            if (n == "1")
            {
                Console.WriteLine("Запуск в однопоточном режиме:");
                BruteForceSingleThread();
                stopwatch.Stop();
                Console.WriteLine($"Время выполнения: {stopwatch.Elapsed.TotalSeconds:F2} секунд");
                Console.WriteLine();
            }
            else if (n == "2")
            {
                Console.WriteLine("\nВведите количество потоков для многопоточного режима:");
                int numThreads;
                while (!int.TryParse(Console.ReadLine(), out numThreads) || numThreads <= 0)
                {
                    Console.WriteLine("Введите положительное число для количества потоков:");
                }
                
                Console.WriteLine($"\nЗапуск в многопоточном режиме с {numThreads} потоками:");
                stopwatch.Restart();
                RunMultiThreaded(numThreads);
                stopwatch.Stop();
                Console.WriteLine($"Время выполнения: {stopwatch.Elapsed.TotalSeconds:F2} секунд");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Неверный ввод");
                Console.WriteLine();

            }
        }


    }

    static void ReadHashesFromFile(string filePath)
    {
        
        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;

                int kount = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    kount++;
                    if (kount == 4) md5Hashes.Add(line.Trim());
                    else sha256Hashes.Add(line.Trim());

                }

            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }


    }

    static void BruteForceSingleThread()
    {
        foreach (var password in GenerateCombinations(5))
        {
            string md5Hash = GetMd5Hash(password);
            string sha256Hash = GetSha256Hash(password);

            if (md5Hashes.Contains(md5Hash))
                Console.WriteLine($"{md5Hash} - {password}");

            if (sha256Hashes.Contains(sha256Hash))
                Console.WriteLine($"{sha256Hash} - {password}");
        }
    }

    static void RunMultiThreaded(int numThreads)
    {
        Thread[] threads = new Thread[numThreads];

        for (int i = 0; i < numThreads; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() => BruteForceMultiThread(threadId, numThreads));
            threads[i].Start();
        }

        for (int i = 0; i < numThreads; i++)
        {
            threads[i].Join();
        }
    }

    static void BruteForceMultiThread(int threadId, int numThreads)
    {
        int i = 0;
        foreach (var password in GenerateCombinations(5))
        {
            if (i % numThreads == threadId)
            {
                string md5Hash = GetMd5Hash(password);
                string sha256Hash = GetSha256Hash(password);

                if (md5Hashes.Contains(md5Hash))
                    Console.WriteLine($"Поток {threadId}: {md5Hash} - {password}"); 

                if (sha256Hashes.Contains(sha256Hash))
                    Console.WriteLine($"Поток {threadId}: {sha256Hash} - {password}");
            }
            i++;
        }
    }

    static IEnumerable<string> GenerateCombinations(int length)
    {
        return GenerateCombinationsRecursive("", length);
    }

    static IEnumerable<string> GenerateCombinationsRecursive(string prefix, int length)
    {
        if (length == 0)
        {
            yield return prefix;
        }
        else
        {
            foreach (char c in chars)
            {
                foreach (var combination in GenerateCombinationsRecursive(prefix + c, length - 1))
                {
                    yield return combination;
                }
            }
        }
    }

    static string GetMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] data = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            return ConvertHashToString(data);
        }
    }

    static string GetSha256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] data = sha256.ComputeHash(Encoding.ASCII.GetBytes(input));
            return ConvertHashToString(data);
        }
    }

    static string ConvertHashToString(byte[] hash)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
