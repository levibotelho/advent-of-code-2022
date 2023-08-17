using System;
using System.Diagnostics;
using System.Net;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Seventeen
    {
        static readonly Shape[] shapes = new Shape[] {
            new Shape(new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0)),
            new Shape(new Point(1, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(1, 2)),
            new Shape(new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(2, 1), new Point(2, 2)),
            new Shape(new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3)),
            new Shape(new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1)),
        };

        public static void Run()
        {
            var lines = GetLines();
            var height2022 = CalculateHeightDirectly(lines, 2022);
            Console.WriteLine($"Height after 2022 blocks: {height2022}");
            var height1E12 = InferHeight(lines, 1000000000000);
            Console.WriteLine($"Height after 1E12 blocks: {height1E12}");
        }

        static long InferHeight(IEnumerable<string> lines, long blockCount)
        {
            const int shapeCount = 5;
            var segmentBlockCount = lines.First().Length * shapeCount;
            var segmentHeights = new List<long>();

            var shifts = GetShifts(lines).ToArray();
            var chamber = new Chamber();
            var lastTotalHeight = 0L;
            var iShape = 0;
            var iShift = 0;
            for (var i = 0L; i < blockCount; i++)
            {
                AddBlock(chamber, shifts, ref iShape, ref iShift);

                if ((i + 1) % segmentBlockCount == 0)
                {
                    var totalHeight = chamber.MaxY + 1;
                    var segmentHeight = totalHeight - lastTotalHeight;
                    segmentHeights.Add(segmentHeight);
                    lastTotalHeight = totalHeight;
                    if (segmentHeights.Count > 1 && segmentHeights.Count % 2 == 1)
                    {
                        var halfLength = (segmentHeights.Count - 1) / 2;
                        var firstHalf = segmentHeights.Skip(1).Take(halfLength).ToArray();
                        var secondHalf = segmentHeights.Skip(1 + halfLength).ToArray();
                        if (Enumerable.SequenceEqual(firstHalf, secondHalf))
                        {
                            var firstSegmentHeight = segmentHeights[0];
                            var firstSegmentBlocks = segmentBlockCount;
                            var repeatingSegmentHeight = firstHalf.Sum();
                            var repeatingSegmentBlocks = segmentBlockCount * halfLength;

                            var remainingBlocksAfterFirst = blockCount - firstSegmentBlocks;
                            var remainingBlocksAtEnd = remainingBlocksAfterFirst % repeatingSegmentBlocks;
                            var repeatingSegmentCount = remainingBlocksAfterFirst / repeatingSegmentBlocks;

                            var fullSegmentsHeight = firstSegmentHeight + (repeatingSegmentHeight * repeatingSegmentCount);
                            var fullSegmentsChamberHeight = chamber.MaxY;
                            for (var j = 0; j < remainingBlocksAtEnd; j++)
                            {
                                AddBlock(chamber, shifts, ref iShape, ref iShift);
                            }

                            var additionalHeight = chamber.MaxY - fullSegmentsChamberHeight;
                            return fullSegmentsHeight + additionalHeight;

                            // Console.WriteLine("First segment height: " + firstSegmentHeight);
                            // Console.WriteLine("First segment blocks: " + firstSegmentBlocks);
                            // Console.WriteLine("Repeating group height: " + repeatingSegmentHeight);
                            // Console.WriteLine("Repeating group blocks: " + repeatingSegmentBlocks);
                            // Console.WriteLine("Final height: " + finalHeight.ToString());
                            // break;
                        }
                    }
                    // Console.WriteLine("Height after segment: " + segmentHeight.ToString());
                }
            }

            return 0;
        }

        static long CalculateHeightDirectly(IEnumerable<string> lines, long blockCount)
        {
            var shifts = GetShifts(lines).ToArray();
            var chamber = new Chamber();
            var iShape = 0;
            var iShift = 0;
            for (var i = 0L; i < blockCount; i++)
            {
                AddBlock(chamber, shifts, ref iShape, ref iShift);
            }

            return chamber.MaxY + 1;
        }

        static void AddBlock(Chamber chamber, int[] shifts, ref int iShape, ref int iShift)
        {
            var shape = shapes[iShape++];
            iShape %= shapes.Length;

            chamber.PlaceAtStart(shape);
            while (true)
            {
                var shift = shifts[iShift++];
                iShift %= shifts.Length;

                shape.ShiftIf(shift, 0, chamber.IsInBounds);
                var didDrop = shape.ShiftIf(0, -1, chamber.IsInBounds);
                if (!didDrop)
                {
                    chamber.FixShape(shape);
                    break;
                }
            }
        }

        static IEnumerable<int> GetShifts(IEnumerable<string> line)
        {
            return line.Single().Select(x => x switch
            {
                '>' => 1,
                '<' => -1,
                _ => throw new ArgumentOutOfRangeException(nameof(line))
            });
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

        public long MaxY { get; private set; } = -1;

        public void Add(Point point)
        {
            var rowsToAdd = point.Y - MaxY;
            for (var i = 0; i < rowsToAdd; i++)
            {
                rows.AddFirst(new PointRow());
                MaxY++;
            }

            var didGetRow = TryGetRow(point.Y, out var row);
            Debug.Assert(didGetRow);
            row.Value = row.Value.Add(point.X);
        }

        public bool Contains(Point point)
        {
            return TryGetRow(point.Y, out var row) && row.Value.Contains(point.X);
        }

        public void DeleteUnreachablePoints(long fromY)
        {
            var didGetRow = TryGetRow(fromY, out var fullRow);
            Debug.Assert(didGetRow);
            if (fullRow.Value.IsFull)
            {
                // Shame that we can't just set next to null and cut the tail in one operation.
                while (rows.Last != fullRow)
                {
                    rows.RemoveLast();
                }
            }
        }

        bool TryGetRow(long y, out LinkedListNode<PointRow> row)
        {
            var rowY = MaxY;
            var rowNode = rows.First;
            if (rowNode == null)
            {
                // Only happens before locking in the first shape.
                row = new LinkedListNode<PointRow>(new PointRow());
                return false;
            }

            while (true)
            {
                if (rowY > y)
                {
                    // A row less than the current row was requested. Move back one.
                    rowNode = rowNode.Next;
                    rowY--;
                    Debug.Assert(rowNode != null, "a row lower than the lowest row should never be requested");
                }
                else if (rowY < y)
                {
                    // A row greater than the greatest row was requested.
                    Debug.Assert(rows.First != null);
                    row = rows.First; // Discarded, value doesn't matter.
                    return false;
                }
                else
                {
                    // The row was found.
                    row = rowNode;
                    return true;
                }
            }
        }

        readonly struct PointRow
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

            readonly byte points;

            public PointRow() { }

            private PointRow(byte points)
            {
                this.points = points;
            }

            public readonly bool IsFull => points == 0b0111_1111;
            public readonly bool IsEmpty => points == 0;

            public readonly PointRow Add(int x)
            {
                var flag = flags[x];
                Debug.Assert(!Contains(x));
                return new PointRow((byte)(points ^ flag));
            }

            public readonly bool Contains(int x)
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

