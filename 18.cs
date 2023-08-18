using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Eighteen
    {
        public static void Run()
        {
            var lines = GetLines();
            var points = lines.Select(x =>
            {
                var split = x.Split(',');
                return new Point(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
            }).ToArray();

            var sizeX = points.Max(x => x.X) + 1;
            var sizeY = points.Max(x => x.Y) + 1;
            var sizeZ = points.Max(x => x.Z) + 1;

            var xy = new List<int>[sizeX, sizeY];
            var xz = new List<int>[sizeX, sizeZ];
            var yz = new List<int>[sizeY, sizeZ];

            foreach (var point in points)
            {
                (xy[point.X, point.Y] ??= new List<int>()).Add(point.Z);
                (xz[point.X, point.Z] ??= new List<int>()).Add(point.Y);
                (yz[point.Y, point.Z] ??= new List<int>()).Add(point.X);
            }

            var sharedSideCount = 0;
            foreach (var collection in new[] { xy, xz, yz })
            {
                foreach (var list in collection)
                {
                    if (list == null)
                    {
                        continue;
                    }

                    list.Sort();
                    for (var i = 1; i < list.Count; i++)
                    {
                        var last = list[i - 1];
                        var current = list[i];
                        if (current == last + 1)
                        {
                            // Two cubes sharing a face removes two faces from the count.
                            sharedSideCount += 2;
                        }
                    }
                }
            }

            var totalSurfaceArea = points.Length * 6 - sharedSideCount;
            Console.WriteLine($"Total surface area: {totalSurfaceArea}");
        }

        readonly record struct Point(int X, int Y, int Z);
    }
}
