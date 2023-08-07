using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
	public static class One
	{
		public static void Run()
		{
            var calories = CalculateCalories();
            Console.WriteLine($"Max calories: {calories.Max()}");
            calories.Sort();
            var sum = calories.TakeLast(3).Sum();
            Console.WriteLine($"Sum of top 3 calories: {sum}. Press any key to exit.");
        }

        static List<int> CalculateCalories()
        {
            var lines = GetLines();
            Console.WriteLine("Calculating...");
            return lines.Aggregate(new List<int> { 0 }, (a, x) =>
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
        }
	}
}
