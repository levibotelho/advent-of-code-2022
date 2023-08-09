using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Nine
    {
        public static void Run()
        {
            var lines = GetLines();
            var movements = lines.Select(x => new Movement(x));
            var tailPositionCount = GetTailPositionCount(movements);
            Console.WriteLine($"Tail position count: {tailPositionCount}");
        }

        static int GetTailPositionCount(IEnumerable<Movement> movements)
        {
            var rope = new Rope();
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
            private readonly List<Position> headTrace = new();
            private readonly List<Position> tailTrace = new();

            public Rope()
            {
                headTrace.Add(Head);
                tailTrace.Add(Tail);
            }

            public Position Head { get; private set; } = new Position(0, 0);
            public Position Tail { get; private set; } = new Position(0, 0);

            public IReadOnlyList<Position> HeadTrace => headTrace;
            public IReadOnlyList<Position> TailTrace => headTrace;

            public void ApplyMovement(Movement movement)
            {
                if (movement.X != 0 && movement.Y == 0)
                {
                    Move(movement.X, Head.MoveX);
                }
                else if (movement.X == 0 && movement.Y != 0)
                {
                    Move(movement.Y, Head.MoveY);
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
                    var lastHead = Head;
                    Head = move(sign);
                    RecalculateTail(lastHead);
                    headTrace.Add(Head);
                    tailTrace.Add(Tail);
                }
            }

            void RecalculateTail(Position lastHead)
            {
                var deltaX = Math.Abs(Head.X - Tail.X);
                var deltaY = Math.Abs(Head.Y - Tail.Y);
                if (deltaX > 1 || deltaY > 1)
                {
                    Tail = lastHead;
                }
                tailTrace.Add(Tail);
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
                return new Position(X + value, Y);
            }
        }
    }
}
