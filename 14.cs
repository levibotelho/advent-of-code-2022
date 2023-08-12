using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Fourteen
    {
        static readonly Point start = new(500, 0);

        public static void Run()
        {
            var lines = GetLines();
            var fallOffSandCount = GetSandCountUntilFallOff(lines);
            Console.WriteLine($"Fall off sand count: {fallOffSandCount}");
            var fullSandCount = GetSandCountUntilFull(lines);
            Console.WriteLine($"Full sand count: {fullSandCount}");
        }

        static int GetSandCountUntilFull(IEnumerable<string> lines)
        {
            var grid = ConstructGrid(lines, true);
            var (count, result) = GetSandResult(grid);
            if (result == SandResult.OutOfBounds)
            {
                throw new InvalidOperationException("the sand fell out of bounds");
            }

            return count;
        }

        static int GetSandCountUntilFallOff(IEnumerable<string> lines)
        {
            var grid = ConstructGrid(lines, false);
            var (count, result) = GetSandResult(grid);
            if (result == SandResult.Full)
            {
                throw new InvalidOperationException("the sand source point is full");
            }

            return count;
        }

        static (int, SandResult) GetSandResult(Grid grid)
        {
            var counter = 0;
            var current = start;
            while (true)
            {
                var next = current.Down();
                if (!grid.Contains(next))
                {
                    return (counter, SandResult.OutOfBounds);
                }

                if (!grid[next])
                {
                    current = next;
                    continue;
                }

                next = current.DownLeft();
                if (!grid.Contains(next))
                {
                    return (counter, SandResult.OutOfBounds);
                }

                if (!grid[next])
                {
                    current = next;
                    continue;
                }

                next = current.DownRight();
                if (!grid.Contains(next))
                {
                    return (counter, SandResult.OutOfBounds);
                }

                if (!grid[next])
                {
                    current = next;
                    continue;
                }

                grid[current] = true;
                counter++;
                current = start;

                if (grid[start])
                {
                    return (counter, SandResult.Full);
                }
            }
        }

        static Grid ConstructGrid(IEnumerable<string> inputLines, bool addFloor)
        {
            var walls = inputLines
                .Select(x => x.Split("->").Select(x => new Point(x)).ToArray())
                .ToList();
            var points = walls.SelectMany(x => x).ToList();
            if (addFloor)
            {
                var floor = CreateFloor(points);
                walls.Add(floor);
                foreach (var point in floor)
                {
                    points.Add(point);
                }
            }

            var grid = new Grid(points);
            foreach (var wall in walls)
            {
                if (wall.Length < 2)
                {
                    throw new ArgumentException("each wall must be defined by at least two coordinates");
                }

                var a = wall[0];
                for (var i = 1; i < wall.Length; i++)
                {
                    var b = wall[i];
                    if (a.X == b.X)
                    {
                        // Vertical wall
                        var x = a.X;
                        var fromY = Math.Min(a.Y, b.Y);
                        var toY = Math.Max(a.Y, b.Y);
                        for (var y = fromY; y <= toY; y++)
                        {
                            grid[x, y] = true;
                        }
                    }
                    else if (a.Y == b.Y)
                    {
                        // Horizontal wall
                        var fromX = Math.Min(a.X, b.X);
                        var toX = Math.Max(a.X, b.X);
                        var y = a.Y;
                        for (var x = fromX; x <= toX; x++)
                        {
                            grid[x, y] = true;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("diagonal walls are not supported");
                    }

                    a = b;
                }
            }

            return grid;
        }

        static Point[] CreateFloor(IReadOnlyCollection<Point> points)
        {
            var maxY = points.Max(x => x.Y);
            var minX = points.Min(x => x.X);
            var maxX = points.Max(x => x.X);
            var floorY = maxY + 2;
            // Add the height to either side to be sure that we have space for the pile.
            var floorXStart = minX - floorY;
            var floorXEnd = maxX + floorY;
            return new[] { new Point(floorXStart, floorY), new Point(floorXEnd, floorY) };
        }

        enum SandResult
        {
            OutOfBounds,
            Full,
        }

        readonly struct Point
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Point(string coordinate)
            {
                var split = coordinate.Split(',');
                X = int.Parse(split[0].Trim());
                Y = int.Parse(split[1].Trim());
            }

            public int X { get; }
            public int Y { get; }

            public Point Down()
            {
                return new Point(X, Y + 1);
            }

            public Point DownLeft()
            {
                return new Point(X - 1, Y + 1);
            }

            public Point DownRight()
            {
                return new Point(X + 1, Y + 1);
            }
        }

        class Grid
        {
            readonly bool[,] grid;
            readonly int minX;
            readonly int maxX;
            readonly int minY;
            readonly int maxY;

            public Grid(IReadOnlyCollection<Point> points)
            {
                minX = points.Min(x => x.X);
                maxX = points.Max(x => x.X);
                minY = 0; // Input does not have negative points.
                maxY = points.Max(x => x.Y);

                var height = maxY + 1;
                var width = maxX - minX + 1;

                grid = new bool[width, height];
            }

            public bool this[int x, int y]
            {
                get => grid[x - minX, y];
                set => grid[x - minX, y] = value;
            }

            public bool this[Point point]
            {
                get => this[point.X, point.Y];
                set => this[point.X, point.Y] = value;
            }

            public bool Contains(Point point)
            {
                return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
            }
        }
    }
}
