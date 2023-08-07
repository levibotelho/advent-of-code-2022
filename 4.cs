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

        static bool HasContain((Range, Range) rangePair)
        {
            var a = rangePair.Item1;
            var b = rangePair.Item2;
            return (a.From <= b.From && a.To >= b.To) || (a.From >= b.From && a.To <= b.To);
        }

        static bool HasOverlap((Range, Range) rangePair)
        {
            var a = rangePair.Item1;
            var b = rangePair.Item2;
            return (a.From >= b.From && a.From <= b.To) || (b.From >= a.From && b.From <= a.To);
        }

        static (Range, Range) GetPairRanges(string assignment)
        {
            var split = assignment.Split(',');
            var a = GetPairRange(split[0]);
            var b = GetPairRange(split[1]);
            return (a, b);
        }

        static Range GetPairRange(string range)
        {
            var split = range.Split('-');
            return new Range(int.Parse(split[0]), int.Parse(split[1]));
        }

        readonly record struct Range(int From, int To);
    }
}
