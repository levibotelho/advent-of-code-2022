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
        }

        class CubeWalker
        {
            readonly bool?[][] map;

            public CubeWalker(bool?[][] map)
            {
                this.map = map;
                X = 0;
                Y = GetStartingY();

                var width = map.Max(x => x.Length);
                var height = map.Length;
                EdgeLength = width > height ? width / 4 : width / 3;
            }

            public int X { get; private set; }
            public int Y { get; private set; }
            public Orientation Orientation { get; private set; }
            public int EdgeLength { get; }

            int GetStartingY()
            {
                for (var y = 0; y < map[0].Length; y++)
                {
                    var cell = map[0][y];
                    if (cell != null)
                    {
                        if (cell == true)
                        {
                            throw new InvalidOperationException("first cell of map is a wall");
                        }

                        return y;
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

                // Look for the next face by moving progressively further away from the current point.
                // These methods in this order should (?) always find the next point, value, and rotation
                // for any valid cube.
                if (
                    TryGetNext1(deltaX, deltaY, out var nextX, out var nextY, out var nextOrientation, out var nextValue) ||
                    TryGetNext2(deltaX, deltaY, out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryGetNext3(out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryGetNext4(deltaX, deltaY, out nextX, out nextY, out nextOrientation, out nextValue)
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
                bool TryResolve(
                    int x,
                    int y,
                    out int nextX,
                    out int nextY,
                    out bool nextValue
                )
                {
                    nextX = x;
                    nextY = y;
                    var value = GetValue(x, y, map);
                    if (value != null)
                    {
                        nextValue = value.Value;
                        return true;
                    }

                    nextValue = false;
                    return false;
                }

                nextOrientation = Orientation;
                var congruentShift = EdgeLength * 4 - 1; // e.g. edge length of 2 = 7 -> 0
                var crossShift = EdgeLength * 2;

                if (deltaX == 0)
                {
                    var yShifted = deltaY == -1 ? Y + congruentShift : Y - congruentShift;
                    return TryResolve(X - crossShift, yShifted, out nextX, out nextY, out nextValue) ||
                        TryResolve(X, yShifted, out nextX, out nextY, out nextValue) ||
                        TryResolve(X + crossShift, yShifted, out nextX, out nextY, out nextValue);
                }
                else if (deltaY == 0)
                {
                    var xShifted = deltaX == -1 ? X + congruentShift : X - congruentShift;
                    return TryResolve(xShifted, Y - crossShift, out nextX, out nextY, out nextValue) ||
                        TryResolve(xShifted, Y, out nextX, out nextY, out nextValue) ||
                        TryResolve(xShifted, Y + crossShift, out nextX, out nextY, out nextValue);
                }
                else
                {
                    throw new ArgumentException("one of deltaX, deltaY must be zero");
                }
            }

            bool TryGetNext3(
                out int nextX,
                out int nextY,
                out Orientation nextOrientation,
                out bool nextValue
            )
            {
                bool TryResolve(
                    int x,
                    int y,
                    Func<int, int, int, (int, int)> mirror,
                    Func<Orientation, Orientation> rotateOrientation,
                    out int nextX,
                    out int nextY,
                    out Orientation nextOrientation,
                    out bool nextValue
                )
                {
                    var value = GetValue(x, y, map);
                    if (value != null)
                    {
                        (nextX, nextY) = mirror(x, y, EdgeLength);
                        nextOrientation = rotateOrientation(Orientation);
                        nextValue = value.Value;
                        return true;
                    }

                    nextX = nextY = 0;
                    nextOrientation = Orientation.Right;
                    nextValue = false;
                    return false;
                }

                // Don't care about deltaX/Y because only one of these should resolve to a point,
                // if any, due to the large distance between faces.
                var smallShift = EdgeLength;
                var bigShift = 3 * EdgeLength;
                return
                    TryResolve(X + bigShift, Y - smallShift, MirrorFaceTRBL, x => x.RotateCounterclockwise(), out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryResolve(X - bigShift, Y + smallShift, MirrorFaceTRBL, x => x.RotateClockwise(), out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryResolve(X + bigShift, Y + smallShift, MirrorFaceTLBR, x => x.RotateCounterclockwise(), out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryResolve(X - bigShift, Y - smallShift, MirrorFaceTLBR, x => x.RotateClockwise(), out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryResolve(X + smallShift, Y - bigShift, MirrorFaceTRBL, x => x.RotateCounterclockwise(), out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryResolve(X - smallShift, Y + bigShift, MirrorFaceTRBL, x => x.RotateClockwise(), out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryResolve(X + smallShift, Y + bigShift, MirrorFaceTLBR, x => x.RotateCounterclockwise(), out nextX, out nextY, out nextOrientation, out nextValue) ||
                    TryResolve(X - smallShift, Y - bigShift, MirrorFaceTLBR, x => x.RotateClockwise(), out nextX, out nextY, out nextOrientation, out nextValue);
            }

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
                return (offsetX + edgeLength - normalizedX, offsetY + edgeLength - normalizedY);
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

                static bool IsValid(int x, int y, bool?[][] map, out int nextX, out int nextY, out bool value)
                {
                    var valueTmp = GetValue(x, y, map);
                    nextX = x;
                    nextY = y;
                    if (valueTmp == null)
                    {
                        value = false;
                        return false;
                    }

                    value = valueTmp.Value;
                    return true;
                }

                nextOrientation = Orientation.RotateClockwise().RotateClockwise();

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

                return
                    IsValid(x0, y0, map, out nextX, out nextY, out nextValue) ||
                    IsValid(x0, y1, map, out nextX, out nextY, out nextValue) ||
                    IsValid(x1, y0, map, out nextX, out nextY, out nextValue) ||
                    IsValid(x1, y1, map, out nextX, out nextY, out nextValue);
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
                // We're moving vertically
                if (deltaX == 0)
                {
                    // Try rotating back around the corner closer to the origin.
                    var offset0 = (X % EdgeLength) - 1;
                    nextX = X - offset0;
                    nextY = Y - (offset0 * deltaY);
                    var value = GetValue(nextX, nextY);
                    if (value != null)
                    {
                        nextOrientation = deltaY == 1 ? Orientation.RotateClockwise() : Orientation.RotateCounterclockwise();
                        nextValue = value.Value;
                        return true;
                    }

                    // Try rotating around the corner further from the origin.
                    var offset1 = EdgeLength - offset0 + 1;
                    nextX = X + offset1;
                    nextY = Y + (offset1 * deltaY);
                    value = GetValue(nextX, nextY);
                    if (value != null)
                    {
                        nextOrientation = deltaY == 1 ? Orientation.RotateCounterclockwise() : Orientation.RotateClockwise();
                        nextValue = value.Value;
                        return true;
                    }
                }

                // We're moving horizontally
                if (deltaY == 0)
                {
                    // Try rotating back around the corner closer to the origin.
                    var offset0 = (Y % EdgeLength) - 1;
                    nextX = X + (offset0 * deltaX);
                    nextY = Y - offset0;
                    var value = GetValue(nextX, nextY);
                    if (value != null)
                    {
                        nextOrientation = deltaX == 1 ? Orientation.RotateCounterclockwise() : Orientation.RotateClockwise();
                        nextValue = value.Value;
                        return true;
                    }

                    // Try rotating around the corner further from the origin.
                    var offset1 = EdgeLength - offset0 + 1;
                    nextX = X + (offset1 * deltaX);
                    nextY = Y + offset1;
                    value = GetValue(nextX, nextY);
                    if (value != null)
                    {
                        nextOrientation = deltaY == 1 ? Orientation.RotateClockwise() : Orientation.RotateCounterclockwise();
                        nextValue = value.Value;
                        return true;
                    }
                }

                nextX = nextY = 0;
                nextOrientation = Orientation.Right;
                nextValue = false;
                return false;
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
                return x < 0 || y < 0 || x >= map.Length || y >= map[0].Length ? null : map[x][y];
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
        }
    }
}
