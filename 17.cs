using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Seventeen
    {
        public static void Run()
        {
            var lines = GetLines();
            var height2022 = GetHeightAfter(lines, 2022);
            Console.WriteLine($"Height after 2022 blocks: {height2022}");
            // var height1E12 = GetHeightAfter(lines, 1000000000000);
            // Console.WriteLine($"Height after 1E12 blocks: {height1E12}");
        }

        static long GetHeightAfter(IEnumerable<string> lines, long blockCount)
        {
            var shifts = GetShifts(lines).GetEnumerator();
            var shapes = GetShapes().GetEnumerator();
            var chamber = new Chamber();

            for (var i = 0L; i < blockCount; i++)
            {
                shapes.MoveNext();
                var shape = shapes.Current;

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

            return chamber.MaxY + 1;
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

        readonly PointStore points = new();

        public void PlaceAtStart(Shape shape)
        {
            shape.Place(newOffsetX, points.MaxY + newOffsetY);
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
                points.DeleteUnreachablePoints(point.Y);
            }
        }

        public long MaxY => points.MaxY;
    }

    class PointStore
    {
        readonly LinkedList<PointRow> rows = new();

        public PointStore()
        {
            rows.AddFirst(new PointRow(0));
        }

        public long MaxY => rows.FirstOrDefault(x => !x.IsEmpty)?.Y ?? -1;

        public void Add(Point point)
        {
            var didGetRow = TryGetRow(point.Y, out var row);
            Debug.Assert(didGetRow);
            row.Value.Add(point.X);
        }

        public bool Contains(Point point)
        {
            if (!TryGetRow(point.Y, out var row))
            {
                return false;
            }

            return row.Value.Contains(point.X);
        }

        public void DeleteUnreachablePoints(long fromY)
        {
            var didGetRow = TryGetRow(fromY, out var row);
            Debug.Assert(didGetRow);
            if (row.Value.IsFull)
            {
                // Shame that we can't just set next to null and cut the tail in one operation.
                while (rows.Last!.Value.Y < fromY)
                {
                    rows.RemoveLast();
                }
            }
        }

        bool TryGetRow(long y, out LinkedListNode<PointRow> row)
        {
            var rowNode = rows.First;
            while (true)
            {
                Debug.Assert(rowNode != null);
                if (rowNode.Value.Y < y)
                {
                    rowNode = rows.AddFirst(new PointRow(rowNode.Value.Y + 1));
                }
                else if (rowNode.Value.Y > y)
                {
                    var previous = rowNode;
                    rowNode = rowNode.Next;
                    if (rowNode == null)
                    {
                        row = previous; // Row is discarded. Set to previous to avoid an allocation.
                        return false;
                    }
                }
                else
                {
                    row = rowNode;
                    return true;
                }
            }
        }

        class PointRow
        {
            static readonly byte[] flags = new byte[]
            {
                1 << 0,
                1 << 1,
                1 << 2,
                1 << 3,
                1 << 4,
                1 << 5,
                1 << 6,
            };

            byte points;

            public PointRow(long y)
            {
                Y = y;
            }

            public long Y { get; }
            public bool IsFull => points == 0b0111_1111;
            public bool IsEmpty => points == 0;

            public void Add(int x)
            {
                var flag = flags[x];
                Debug.Assert(!Contains(x));
                points ^= flag;
            }

            public bool Contains(int x)
            {
                var flag = flags[x];
                return (points & flag) > 0;
            }
        }
    }

    readonly record struct Point(int X, long Y);

    class Shape
    {
        readonly Point[] absolutePoints;
        int offsetX;
        long offsetY;

        public Shape(params Point[] points)
        {
            absolutePoints = points;
        }

        public IEnumerable<Point> Points => absolutePoints.Select(p => new Point(p.X + offsetX, p.Y + offsetY));

        public Shape Clone()
        {
            return new Shape(absolutePoints);
        }

        public void Place(int x, long y)
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

