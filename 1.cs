using System;
namespace AdventOfCode
{
	public static class One
	{
		public static void Run()
		{
            Console.WriteLine("Paste input and then press <Enter> three times...");

            var lines = new List<string>();

            while (lines.Count < 2 || (lines[^1] != "" || lines[^2] != ""))
            {
                var line = Console.ReadLine();
                if (line != null)
                {
                    lines.Add(line);
                }
            }

            Console.WriteLine("Calculating...");

            var calories = lines.Aggregate(new List<int> { 0 }, (a, x) =>
            {
                if (string.IsNullOrWhiteSpace(x))
                {
                    a.Add(0);
                }
                else
                {
                    var count = int.Parse(x);
                    a[^1] += count;
                }
                return a;
            });

            Console.WriteLine($"Max calories: {calories.Max()}. Press any key to exit.");
            Console.ReadLine();
        }
	}
}
