using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
	public static class Four
	{
		public static void Run()
		{
            var lines = GetLines();
            var ranges = lines.Select(GetPairRanges);
            var containCount = ranges.Count(HasContain);
            Console.WriteLine($"Contain count: {containCount}");
            var overlapCount = ranges.Count(HasOverlap);
            Console.WriteLine($"Overlap count: {overlapCount}");
        }

        static bool HasContain(((int, int), (int, int)) rangePair)
        {
            var a = rangePair.Item1;
            var b = rangePair.Item2;
            return (a.Item1 <= b.Item1 && a.Item2 >= b.Item2) || (a.Item1 >= b.Item1 && a.Item2 <= b.Item2);
        }

        static bool HasOverlap(((int, int), (int, int)) rangePair)
        {
            var a = rangePair.Item1;
            var b = rangePair.Item2;
            return (a.Item1 >= b.Item1 && a.Item1 <= b.Item2) || (b.Item1 >= a.Item1 && b.Item1 <= a.Item2);
        }

        static ((int, int), (int, int)) GetPairRanges(string assignment)
        {
            var split = assignment.Split(',');
            var a = GetPairRange(split[0]);
            var b = GetPairRange(split[1]);
            return (a, b);
        }

        static (int, int) GetPairRange(string range)
        {
            var split = range.Split('-');
            return (int.Parse(split[0]), int.Parse(split[1]));
        }
    }
}
