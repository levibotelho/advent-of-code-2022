using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Nine
    {
        public static void Run()
        {
            var lines = GetLines();
            var movements = lines.Select(x => new Movement(x)).ToArray();
            var tailPositionCount2 = GetTailPositionCount(new Rope(2), movements);
            Console.WriteLine($"2-segment tail position count: {tailPositionCount2}");
            var tailPositionCount10 = GetTailPositionCount(new Rope(10), movements);
            Console.WriteLine($"10-segment tail position count: {tailPositionCount10}");
        }

        static int GetTailPositionCount(Rope rope, IReadOnlyCollection<Movement> movements)
        {
            foreach (var movement in movements)
            {
                rope.ApplyMovement(movement);
            }

            return rope.TailTrace.Distinct().Count();
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

        class Rope
        {
            private readonly List<Position> tailTrace = new();
            private readonly Position[] segments;

            public Rope(int segmentCount)
            {
                segments = Enumerable.Repeat(new Position(0, 0), segmentCount).ToArray();
                tailTrace.Add(segments[^1]);
            }

            public IReadOnlyList<Position> TailTrace => tailTrace;

            public void ApplyMovement(Movement movement)
            {
                if (movement.X != 0 && movement.Y == 0)
                {
                    Move(movement.X, (x) => segments[0].MoveX(x));
                }
                else if (movement.X == 0 && movement.Y != 0)
                {
                    Move(movement.Y, (x) => segments[0].MoveY(x));
                }
                else
                {
                    throw new NotSupportedException("exactly one of x, y must be equal to zero");
                }
            }

            void Move(int steps, Func<int, Position> move)
            {
                var sign = Math.Sign(steps);
                var absSteps = sign * steps;
                for (var i = 0; i < absSteps; i++)
                {
                    // Move head
                    segments[0] = move(sign);
                    for (var j = 1; j < segments.Length; j++)
                    {
                        var current = segments[j];
                        var leader = segments[j - 1];
                        var deltaX = leader.X - current.X;
                        var deltaY = leader.Y - current.Y;
                        if (Math.Abs(deltaX) <= 1 && Math.Abs(deltaY) <= 1)
                        {
                            // The current segment is touching its leader.
                            continue;
                        }

                        var moveX = Math.Sign(deltaX);
                        var moveY = Math.Sign(deltaY);
                        segments[j] = current.MoveX(moveX).MoveY(moveY);
                    }

                    tailTrace.Add(segments[^1]);
                }
            }
        }

        readonly struct Position
        {
            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public readonly Position MoveX(int value)
            {
                return new Position(X + value, Y);
            }

            public Position MoveY(int value)
            {
                return new Position(X, Y + value);
            }
        }
    }
}
