using System;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class TwentyTwo
    {
        public static void Run()
        {
            var lines = GetLines().ToArray();
            var map = CreateMap(lines);
            var instructions = CreateInstructions(lines);

            // Cells are bool? where null = no value, false = open, true = wall
            // Direction is 0-3 where 0 = right, 1 = down, 2 = left, 3 = up
            var y = 0;
            var (x, isStartWall) = GetNextInRow(map[y], map[y].Length - 1, 1);
            if (isStartWall)
            {
                throw new InvalidOperationException("cannot start on wall");
            }

            var direction = 0;
            foreach (var instruction in instructions)
            {
                switch (direction)
                {
                    case 0:
                    case 2:
                        var xDirection = 1 - direction; // 0 = right, 2 = left
                        for (var i = 0; i < instruction.Count; i++)
                        {
                            var (nextX, isWall) = GetNextInRow(map[y], x, xDirection);
                            if (isWall)
                            {
                                break;
                            }

                            x = nextX;
                        }
                        break;
                    case 1:
                    case 3:
                        var yDirection = 2 - direction; // 1 = down, 3 = up
                        for (var i = 0; i < instruction.Count; i++)
                        {
                            var (nextY, isWall) = GetNextInColumn(map, x, y, yDirection);
                            if (isWall)
                            {
                                break;
                            }

                            y = nextY;
                        }
                        break;
                }

                direction = instruction.TurnRight
                    ? direction == 3 ? 0 : direction + 1
                    : direction == 0 ? 3 : direction - 1;

                Debug.Assert(direction >= 0 && direction <= 3);
                Debug.Assert(x >= 0 && x < map[0].Length);
                Debug.Assert(y >= 0 && y < map.Length);
            }

            var row = y + 1;
            var column = x + 1;
            var score = (1000 * row) + (4 * column) + direction;
            Console.WriteLine(score);
        }

        static (int, bool) GetNextInRow(bool?[] line, int xCurrent, int direction)
        {
            var next = xCurrent + direction;
            while (true)
            {
                if (next < 0)
                {
                    next += line.Length;
                }

                next %= line.Length;
                var value = line[next];
                if (value != null)
                {
                    return (next, value.Value);
                }

                next += direction;
            }
        }

        static (int, bool) GetNextInColumn(bool?[][] map, int x, int yCurrent, int direction)
        {
            var next = yCurrent + direction;
            while (true)
            {
                if (next < 0)
                {
                    next += map.Length;
                }

                next %= map.Length;
                var value = map[next][x];
                if (value != null)
                {
                    return (next, value.Value);
                }

                next += direction;
            }
        }

        static IReadOnlyList<Instruction> CreateInstructions(IEnumerable<string> lines)
        {
            // The last line has no rotation. Add a rotation right and a rotation left to simplify
            // parsing without changing the final direction.
            var instructionsLine = lines.Last() + "R0L";
            var count = "";
            var instructions = new List<Instruction>();
            foreach (var character in instructionsLine)
            {
                if (char.IsNumber(character))
                {
                    count += character;
                }
                else if (character == 'R')
                {
                    var countInt = int.Parse(count);
                    instructions.Add(new Instruction(countInt, true));
                    count = "";
                }
                else if (character == 'L')
                {
                    var countInt = int.Parse(count);
                    instructions.Add(new Instruction(countInt, false));
                    count = "";
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(lines));
                }
            }

            return instructions;
        }

        static bool?[][] CreateMap(IEnumerable<string> lines)
        {
            var mapLines = lines.SkipLast(2).ToArray();
            var maxLength = mapLines.Max(x => x.Length);
            return mapLines
                .Select(x => x.PadRight(maxLength).Select(x => x switch
                {
                    ' ' => (bool?)null,
                    '.' => false,
                    '#' => true,
                    _ => throw new ArgumentOutOfRangeException(nameof(lines))
                }).ToArray())
                .ToArray();
        }

        readonly record struct Instruction(int Count, bool TurnRight);
    }
}
