using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Seventeen
    {
        public static void Run()
        {
            var lines = GetLines();
            var shifts = GetShifts(lines).GetEnumerator();
            var shapes = GetShapes();
            var chamber = new Chamber();

            foreach (var shape in shapes.Take(2022))
            {
                chamber.PlaceAtStart(shape);

                while (true)
                {
                    shifts.MoveNext();
                    var shift = shifts.Current;
                    shape.ShiftIf(shift, 0, chamber.IsInBounds);
                    var didDrop = shape.ShiftIf(0, -1, chamber.IsInBounds);
                    if (!didDrop)
                    {
                        chamber.FixShape(shape);
                        break;
                    }
                }
            }

            // +1 because height is 0-indexed
            Console.WriteLine($"Height after 2022 blocks: {chamber.GetHighestPoint() + 1}");
        }

        static IEnumerable<int> GetShifts(IEnumerable<string> line)
        {
            var i = 0;
            var firstLine = line.Single();
            while (true)
            {
                yield return firstLine[i++ % firstLine.Length] switch
                {
                    '>' => 1,
                    '<' => -1,
                    _ => throw new ArgumentOutOfRangeException(nameof(line))
                };
            }
        }

        static IEnumerable<Shape> GetShapes()
        {
            var shapes = new Shape[] {
                new Shape(new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0)),
                new Shape(new Point(1, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(1, 2)),
                new Shape(new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(2, 1), new Point(2, 2)),
                new Shape(new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3)),
                new Shape(new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1)),
            };
            var i = 0;
            while (true)
            {
                yield return shapes[i++ % shapes.Length].Clone();
            }
        }
    }

    class Chamber
    {
        const int maxX = 6;
        const int newOffsetX = 2;
        const int newOffsetY = 4;

        readonly HashSet<Point> points = new();

        public void PlaceAtStart(Shape shape)
        {
            var highest = GetHighestPoint();
            shape.Place(newOffsetX, highest + newOffsetY);
        }

        public bool IsInBounds(Shape shape)
        {
            return shape.Points.All(x => x.X >= 0 && x.X <= maxX && x.Y >= 0 && !points.Contains(x));
        }

        public void FixShape(Shape shape)
        {
            foreach (var point in shape.Points)
            {
                points.Add(point);
            }
        }

        public int GetHighestPoint() => points.Any() ? points.Max(x => x.Y) : -1;
    }

    readonly record struct Point(int X, int Y);

    class Shape
    {
        readonly Point[] absolutePoints;
        int offsetX;
        int offsetY;

        public Shape(params Point[] points)
        {
            absolutePoints = points;
        }

        public IEnumerable<Point> Points => absolutePoints.Select(p => new Point(p.X + offsetX, p.Y + offsetY));

        public Shape Clone()
        {
            return new Shape(absolutePoints);
        }

        public void Place(int x, int y)
        {
            offsetX = x;
            offsetY = y;
        }

        public bool ShiftIf(int x, int y, Func<Shape, bool> condition)
        {
            offsetX += x;
            offsetY += y;
            if (!condition(this))
            {
                offsetX -= x;
                offsetY -= y;
                return false;
            }

            return true;
        }
    }
}

