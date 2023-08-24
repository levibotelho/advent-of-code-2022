using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static partial class TwentyTwo
    {
        public static void Run()
        {
            var lines = GetLines().ToArray();
            Part1(lines);
            Part2(lines);
        }
    }
}
