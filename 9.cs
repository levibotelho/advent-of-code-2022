using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Nine
    {
        public static void Run()
        {
            var lines = GetLines();
            var movements = lines.Select(x => new Movement(x));
        }

        class Movement
        {
            public Movement(string line)
            {
                var split = line.Split(' ');
                switch (split)
                {
                    case ["L", var distanceStr] when int.TryParse(distanceStr, out var distance):
                        X = -distance;
                        Y = 0;
                        break;
                    case ["R", var distanceStr] when int.TryParse(distanceStr, out var distance):
                        X = distance;
                        Y = 0;
                        break;
                    case ["U", var distanceStr] when int.TryParse(distanceStr, out var distance):
                        X = 0;
                        Y = distance;
                        break;
                    case ["D", var distanceStr] when int.TryParse(distanceStr, out var distance):
                        X = 0;
                        Y = -distance;
                        break;
                    default:
                        throw new ArgumentException($"invalid line: {line}");
                }
            }

            public int X { get; }
            public int Y { get; }
        }
    }
}
