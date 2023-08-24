using System;
using System.Diagnostics;

namespace AdventOfCode
{
    public static partial class TwentyTwo
    {
        static void Part2(IEnumerable<string> lines)
        {
            var map = CreateMap(lines);
            var walker = new CubeWalker(map);
            var instructions = CreateInstructions(lines);
            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                walker.Walk(instruction.Count, instruction.TurnRight);
            }

            var password = CalculatePassword(walker.X, walker.Y, walker.Orientation.Value);
            Console.WriteLine($"Password part 2: {password}");
        }

        class CubeWalker
        {
            readonly bool?[][] map;

            public CubeWalker(bool?[][] map)
            {
                this.map = map;
                X = GetStartingX();
                Y = 0;

                var width = map.Max(x => x.Length);
                var height = map.Length;
                EdgeLength = width > height ? width / 4 : width / 3;
            }

            public int X { get; private set; }
            public int Y { get; private set; }
            public Orientation Orientation { get; private set; }
            public int EdgeLength { get; }

            int GetStartingX()
            {
                for (var x = 0; x < map[0].Length; x++)
                {
                    var cell = map[0][x];
                    if (cell != null)
                    {
                        if (cell == true)
                        {
                            throw new InvalidOperationException("first cell of map is a wall");
                        }

                        return x;
                    }
                }

                throw new InvalidOperationException("first row of map contains no open cells");
            }

            public void Walk(int distance, bool isEndTurnClockwise)
            {
                for (var i = 0; i < distance; i++)
                {
                    var (nextX, nextY, nextOrientation, value) = GetNextCoordinate();
                    if (value)
                    {
                        // We've hit a wall.
                        break;
                    }
                    else
                    {
                        // Move to the next coordinate.
                        X = nextX;
                        Y = nextY;
                        Orientation = nextOrientation;
                    }
                }

                Orientation = isEndTurnClockwise ? Orientation.RotateClockwise() : Orientation.RotateCounterclockwise();
            }

            (int x, int y, Orientation orientation, bool value) GetNextCoordinate()
            {
                var (nextXLinear, nextYLinear) = GetNextLinear();
                var nextValueLinear = GetValue(nextXLinear, nextYLinear);
                if (nextValueLinear != null)
                {
                    // We can advance linearly. Return the coordinatates/value and retain the orientation.
                    return (nextXLinear, nextYLinear, Orientation, nextValueLinear.Value);
                }

                var deltaX = nextXLinear - X;
                var deltaY = nextYLinear - Y;

                Debug.Assert(deltaX == 0 || deltaY == 0);
                Debug.Assert(deltaX == -1 || deltaX == 1 || deltaY == -1 || deltaY == 1);
                Debug.Assert(deltaX != deltaY);

                // 1 through 4 search progressively further from the point in question. This particular
                // order works for the test and input cubes, ensuring that the correct path is chosen
                // even when multiple candidates exist. It's hacky, but gets the job done on both datasets.
                if (
                    TryGetNext1(deltaX, deltaY, out var nextX, out var nextY, out var nextOrientation, out var nextValue) ||
                    TryGetNext4(deltaX, deltaY, out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryGetNext3(deltaX, deltaY, out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryGetNext2(deltaX, deltaY, out nextX, out nextY, out nextOrientation, out nextValue)
                )
                {
                    return (nextX, nextY, nextOrientation, nextValue);
                }

                throw new InvalidOperationException("could not get next coordinate, the layout is not a supported (valid?) cube");
            }

            bool TryGetNext4(
                int deltaX,
                int deltaY,
                out int nextX,
                out int nextY,
                out Orientation nextOrientation,
                out bool nextValue
            )
            {
                nextOrientation = Orientation;
                var congruentShift = EdgeLength * 4 - 1; // e.g. edge length of 2 = 7 -> 0
                var crossShift = EdgeLength * 2;

                if (deltaX == 0)
                {
                    var yShifted = deltaY == -1 ? Y + congruentShift : Y - congruentShift;
                    return GetSingleValidCase(
                        out nextX,
                        out nextY,
                        out nextOrientation,
                        out nextValue,
                        (X - crossShift, yShifted, Orientation),
                        (X, yShifted, Orientation),
                        (X + crossShift, yShifted, Orientation)
                    );
                }
                else if (deltaY == 0)
                {
                    var xShifted = deltaX == -1 ? X + congruentShift : X - congruentShift;
                    return GetSingleValidCase(
                        out nextX,
                        out nextY,
                        out nextOrientation,
                        out nextValue,
                        (xShifted, Y - crossShift, Orientation),
                        (xShifted, Y, Orientation),
                        (xShifted, Y + crossShift, Orientation)
                    );
                }
                else
                {
                    throw new ArgumentException("one of deltaX, deltaY must be zero");
                }
            }

            bool TryGetNext3(
                int deltaX,
                int deltaY,
                out int nextX,
                out int nextY,
                out Orientation nextOrientation,
                out bool nextValue
            )
            {
                var smallShift = EdgeLength;
                var bigShift = 3 * EdgeLength;

                if (deltaX == 0)
                {
                    if (deltaY == -1)
                    {
                        // Up
                        var (xLeft, yLeft) = MirrorFaceTLBR(X, Y);
                        var (xRight, yRight) = MirrorFaceTRBL(X, Y);
                        return GetSingleValidCase(
                            out nextX,
                            out nextY,
                            out nextOrientation,
                            out nextValue,
                            (xLeft - smallShift, yLeft + bigShift, Orientation.RotateClockwise()),
                            (xRight + smallShift, yRight + bigShift, Orientation.RotateCounterclockwise())
                        );
                    }
                    else if (deltaY == 1)
                    {
                        // Down
                        var (xLeft, yLeft) = MirrorFaceTRBL(X, Y);
                        var (xRight, yRight) = MirrorFaceTLBR(X, Y);
                        return GetSingleValidCase(
                            out nextX,
                            out nextY,
                            out nextOrientation,
                            out nextValue,
                            (xLeft - smallShift, yLeft - bigShift, Orientation.RotateCounterclockwise()),
                            (xRight + smallShift, yRight - bigShift, Orientation.RotateClockwise())
                        );
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(deltaY));
                    }
                }

                if (deltaY == 0)
                {
                    if (deltaX == -1)
                    {
                        // Left
                        var (xUp, yUp) = MirrorFaceTLBR(X, Y);
                        var (xDown, yDown) = MirrorFaceTRBL(X, Y);
                        return GetSingleValidCase(
                            out nextX,
                            out nextY,
                            out nextOrientation,
                            out nextValue,
                            (xUp + smallShift, yUp - bigShift, Orientation.RotateCounterclockwise()),
                            (xDown + smallShift, yDown + bigShift, Orientation.RotateClockwise())
                        );
                    }
                    else if (deltaX == 1)
                    {
                        // Right
                        var (xUp, yUp) = MirrorFaceTRBL(X, Y);
                        var (xDown, yDown) = MirrorFaceTLBR(X, Y);
                        return GetSingleValidCase(
                            out nextX,
                            out nextY,
                            out nextOrientation,
                            out nextValue,
                            (xUp - smallShift, yUp - bigShift, Orientation.RotateClockwise()),
                            (xDown - smallShift, yDown + bigShift, Orientation.RotateCounterclockwise())
                        );
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(deltaX));
                    }
                }

                throw new ArgumentException("one of deltaX, deltaY must be zero");
            }

            bool TryGetNext2(
                int deltaX,
                int deltaY,
                out int nextX,
                out int nextY,
                out Orientation nextOrientation,
                out bool nextValue
            )
            {
                static (int offsetPositive, int offsetNegative) GetOffsets(int variable, int edgeLength)
                {
                    // Shift left or right, mirroring the X coordinate around a center edge. e.g.
                    // | _ _ _ _ | _ _ _ _ | _ _*_ _ | _ _ _ _ |
                    //             x ----------------------> x

                    var offsetPositive = (3 * edgeLength) - (2 * (variable % edgeLength)) - 1;
                    // We shift 2 edges over on average, mirroring around a centre point. A long shift
                    // one way translates to a short shift the other way, with both summing to 4 edges.
                    var offsetNegative = (4 * edgeLength) - offsetPositive;
                    return (offsetPositive, offsetNegative);
                }

                int x0, x1, y0, y1;
                if (deltaX == 0)
                {
                    // We're moving vertically. Offset horizontally and move up or down by 1 edge.
                    var (xPlusOffset, xMinusOffset) = GetOffsets(X, EdgeLength);
                    x0 = X + xPlusOffset;
                    x1 = X - xMinusOffset;
                    y0 = Y + EdgeLength;
                    y1 = Y - EdgeLength;
                }
                else if (deltaY == 0)
                {
                    // We're moving horizontally. Offset vertically and move left or right by 1 edge.
                    var (yPlusOffset, yMinusOffset) = GetOffsets(Y, EdgeLength);
                    x0 = X + EdgeLength;
                    x1 = X - EdgeLength;
                    y0 = Y + yPlusOffset;
                    y1 = Y - yMinusOffset;
                }
                else
                {
                    throw new InvalidOperationException("exactly one of deltaX, deltaY must be zero");
                }

                var flippedOrientation = Orientation.RotateClockwise().RotateClockwise();
                return GetSingleValidCase(
                    out nextX,
                    out nextY,
                    out nextOrientation,
                    out nextValue,
                    (x0, y0, flippedOrientation),
                    (x0, y1, flippedOrientation),
                    (x1, y0, flippedOrientation),
                    (x1, y1, flippedOrientation)
                );
            }

            /// <summary>
            /// Returns the next coordinates by rotating a quarter turn onto a face that is diagonally-adjacent,
            /// if possible.
            /// </summary>
            bool TryGetNext1(
                int deltaX,
                int deltaY,
                out int nextX,
                out int nextY,
                out Orientation nextOrientation,
                out bool nextValue
            )
            {
                if (deltaX == 0)
                {
                    if (deltaY == -1)
                    {
                        // Up
                        var (xLeft, yLeft) = MirrorFaceTRBL(X, Y);
                        var (xRight, yRight) = MirrorFaceTLBR(X, Y);
                        return GetSingleValidCase(
                            out nextX,
                            out nextY,
                            out nextOrientation,
                            out nextValue,
                            (xLeft - EdgeLength, yLeft - EdgeLength, Orientation.RotateCounterclockwise()),
                            (xRight + EdgeLength, yRight - EdgeLength, Orientation.RotateClockwise())
                        );
                    }
                    else if (deltaY == 1)
                    {
                        // Down
                        var (xLeft, yLeft) = MirrorFaceTLBR(X, Y);
                        var (xRight, yRight) = MirrorFaceTRBL(X, Y);
                        return GetSingleValidCase(
                            out nextX,
                            out nextY,
                            out nextOrientation,
                            out nextValue,
                            (xLeft - EdgeLength, yLeft + EdgeLength, Orientation.RotateClockwise()),
                            (xRight + EdgeLength, yRight + EdgeLength, Orientation.RotateCounterclockwise())
                        );
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(deltaY));
                    }
                }

                if (deltaY == 0)
                {
                    if (deltaX == -1)
                    {
                        // Left
                        var (xUp, yUp) = MirrorFaceTRBL(X, Y);
                        var (xDown, yDown) = MirrorFaceTLBR(X, Y);
                        return GetSingleValidCase(
                            out nextX,
                            out nextY,
                            out nextOrientation,
                            out nextValue,
                            (xUp - EdgeLength, yUp - EdgeLength, Orientation.RotateClockwise()),
                            (xDown - EdgeLength, yDown + EdgeLength, Orientation.RotateCounterclockwise())
                        );
                    }
                    else if (deltaX == 1)
                    {
                        // Right
                        var (xUp, yUp) = MirrorFaceTLBR(X, Y);
                        var (xDown, yDown) = MirrorFaceTRBL(X, Y);
                        return GetSingleValidCase(
                            out nextX,
                            out nextY,
                            out nextOrientation,
                            out nextValue,
                            (xUp + EdgeLength, yUp - EdgeLength, Orientation.RotateCounterclockwise()),
                            (xDown + EdgeLength, yDown + EdgeLength, Orientation.RotateClockwise())
                        );
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(deltaX));
                    }
                }

                throw new ArgumentException("one of deltaX, deltaY must be zero");
            }

            bool GetSingleValidCase(
                out int nextX,
                out int nextY,
                out Orientation nextOrientation,
                out bool nextValue,
                params (int x, int y, Orientation orientation)[] cases
            )
            {
                nextX = nextY = 0;
                nextOrientation = Orientation.Right;
                nextValue = false;
                var hasFound = false;
                foreach (var (x, y, orientation) in cases)
                {
                    if (IsValid(x, y, orientation, out var nX, out var nY, out var nO, out var nV))
                    {
                        if (hasFound)
                        {
                            throw new InvalidOperationException("found multiple valid cases");
                        }

                        hasFound = true;
                        nextX = nX;
                        nextY = nY;
                        nextOrientation = nO;
                        nextValue = nV;
                    }
                }

                return hasFound;
            }

            bool IsValid(
                int x,
                int y,
                Orientation nextOrientationIn,
                out int nextX,
                out int nextY,
                out Orientation nextOrientation,
                out bool nextValue
            )
            {
                nextX = x;
                nextY = y;
                nextOrientation = nextOrientationIn;
                var value = GetValue(x, y, map);
                if (value != null)
                {
                    nextValue = value.Value;
                    return true;
                }

                nextValue = false;
                return false;
            }

            (int x, int y) MirrorFaceTLBR(int x, int y) => MirrorFaceTLBR(x, y, EdgeLength);
            (int x, int y) MirrorFaceTRBL(int x, int y) => MirrorFaceTRBL(x, y, EdgeLength);

            /// <summary>
            /// Mirrors a coordinate within a face across the top left/bottom right diagonal
            /// </summary>
            /// <returns></returns>
            static (int x, int y) MirrorFaceTLBR(int x, int y, int edgeLength)
            {
                var offsetX = (x / edgeLength) * edgeLength;
                var normalizedX = x - offsetX;
                var offsetY = (y / edgeLength) * edgeLength;
                var normalizedY = y - offsetY;
                return (offsetX + normalizedY, offsetY + normalizedX);
            }

            /// <summary>
            /// Mirrors a coordinate within a face across the top right/bottom left diagonal
            /// </summary>
            /// <returns></returns>
            static (int x, int y) MirrorFaceTRBL(int x, int y, int edgeLength)
            {
                var offsetX = (x / edgeLength) * edgeLength;
                var normalizedX = x - offsetX;
                var offsetY = (y / edgeLength) * edgeLength;
                var normalizedY = y - offsetY;
                return (offsetX + edgeLength - normalizedY - 1, offsetY + edgeLength - normalizedX - 1);
            }

            /// <summary>
            /// Gets a value from the point map, returning null if it corresponds to an empty space
            /// or is out of bounds.
            /// </summary>
            bool? GetValue(int x, int y)
            {
                return GetValue(x, y, map);
            }

            /// <summary>
            /// Gets a value from the point map, returning null if it corresponds to an empty space
            /// or is out of bounds.
            /// </summary>
            static bool? GetValue(int x, int y, bool?[][] map)
            {
                // Map is an array of rows, so the first coordinate selects the row and is therefore y, not x.
                return x < 0 || y < 0 || y >= map.Length || x >= map[y].Length ? null : map[y][x];
            }

            /// <summary>
            /// Returns the next coordinates by continuing in a straight line across the point map. The
            /// coordinates may point to a value that is null or out-of-bounds.
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            (int, int) GetNextLinear()
            {
                if (Orientation == Orientation.Left || Orientation == Orientation.Right)
                {
                    var xDirection = 1 - Orientation.Value; // 0 = right, 2 = left
                    return (X + xDirection, Y);
                }
                else if (Orientation == Orientation.Up || Orientation == Orientation.Down)
                {
                    var yDirection = 2 - Orientation.Value; // 1 = down, 3 = up
                    return (X, Y + yDirection);
                }
                else
                {
                    throw new InvalidOperationException("invalid orientation");
                }
            }
        }

        readonly struct Orientation : IEquatable<Orientation>
        {
            public static readonly Orientation Right = new(0);
            public static readonly Orientation Down = new(1);
            public static readonly Orientation Left = new(2);
            public static readonly Orientation Up = new(3);

            Orientation(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public override bool Equals(object? obj) => obj is Orientation other && Equals(other);

            public bool Equals(Orientation other) => Value == other.Value;

            public override int GetHashCode() => Value.GetHashCode();

            public static bool operator ==(Orientation lhs, Orientation rhs) => lhs.Equals(rhs);

            public static bool operator !=(Orientation lhs, Orientation rhs) => !(lhs == rhs);

            public Orientation RotateClockwise()
            {
                return Value switch
                {
                    0 => Down,
                    1 => Left,
                    2 => Up,
                    3 => Right,
                    _ => throw new InvalidOperationException("invalid orientation")
                };
            }

            public Orientation RotateCounterclockwise()
            {
                return Value switch
                {
                    0 => Up,
                    1 => Right,
                    2 => Down,
                    3 => Left,
                    _ => throw new InvalidOperationException("invalid orientation")
                };
            }

            public override string ToString()
            {
                return Value switch
                {
                    0 => "Right",
                    1 => "Down",
                    2 => "Left",
                    3 => "Up",
                    _ => "<Invalid orientation>"
                };
            }
        }
    }
}
