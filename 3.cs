﻿using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
	public static class Three
	{
		public static void Run()
		{
            var lines = GetLines();
            var result = lines.Select(GetSharedItemPriority).Sum();
            Console.WriteLine($"Part 1 priority sum: {result}");
        }

        static int GetSharedItemPriority(string items)
        {
            var (a, b) = GetCompartments(items);
            var intersection = a.Intersect(b);
            return GetPriority(intersection.Single());
        }

        static (string, string) GetCompartments(string items)
        {
            var halfway = items.Length / 2;
            var a = items.Substring(0, halfway);
            var b = items.Substring(halfway, halfway);
            return (a, b);
        }

        static int GetPriority(char item)
        {
            return char.IsUpper(item) ? item - 65 + 27 : item - 96;
        }
    }
}
