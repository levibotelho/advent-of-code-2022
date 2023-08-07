using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Five
    {
        public static void Run()
        {
            var lines = GetLines();
            var topCrates9000 = GetTopCrates(lines, ApplyInstructions9000);
            Console.WriteLine($"Top crates CM9000: {topCrates9000}");
            var topCrates9001 = GetTopCrates(lines, ApplyInstructions9001);
            Console.WriteLine($"Top crates CM9001: {topCrates9001}");
        }

        static string GetTopCrates(IEnumerable<string> lines, Action<IReadOnlyList<Stack<char>>, IEnumerable<Instruction>> applyInstructions)
        {
            var diagram = GetDiagram(lines);
            var instructions = GetInstructions(lines);
            applyInstructions(diagram, instructions);
            return new string(diagram.Select(x => x.Peek()).ToArray());
        }

        static void ApplyInstructions9001(IReadOnlyList<Stack<char>> diagram, IEnumerable<Instruction> instructions)
        {
            var tmpStack = new Stack<char>();
            foreach (var instruction in instructions)
            {
                var fromStack = diagram[instruction.From];
                var toStack = diagram[instruction.To];
                for (var i = 0; i < instruction.Count; i++)
                {
                    var tmp = fromStack.Pop();
                    tmpStack.Push(tmp);
                }

                while (tmpStack.Count > 0)
                {
                    var crate = tmpStack.Pop();
                    toStack.Push(crate);
                }
            }
        }

        static void ApplyInstructions9000(IReadOnlyList<Stack<char>> diagram, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                var fromStack = diagram[instruction.From];
                var toStack = diagram[instruction.To];
                for (var i = 0; i < instruction.Count; i++)
                {
                    var tmp = fromStack.Pop();
                    toStack.Push(tmp);
                }
            }
        }

        static Stack<char>[] GetDiagram(IEnumerable<string> lines)
        {
            var rows = lines
                .TakeWhile(x => x.Trim().StartsWith('['))
                //  *   *   *...
                // 0123456789...
                // [D] [D] [T] [F] [G] [B] [B] [H] [Z]
                .Select(x => new string(x.Where((x, i) => (i - 1) % 4 == 0).ToArray()))
                .Reverse()
                .ToArray();

            var diagram = rows[0].Select(x => new Stack<char>()).ToArray();
            foreach (var row in rows)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    var stack = diagram[i];
                    var crate = row[i];
                    if (char.IsLetter(crate))
                    {
                        stack.Push(crate);
                    }
                }
            }

            return diagram;
        }

        static IEnumerable<Instruction> GetInstructions(IEnumerable<string> lines)
        {
            return lines
                .SkipWhile(x => !x.StartsWith('m'))
                .Select(x => new Instruction(x));
        }

        class Instruction
        {
            public int Count { get; }
            public int From { get; }
            public int To { get; }

            public Instruction(string line)
            {
                var values = line
                    .Split(' ')
                    .Where(x => x.All(char.IsNumber))
                    .Select(int.Parse)
                    .ToArray();
                Count = values[0];
                From = values[1] - 1;
                To = values[2] - 1;
            }
        }
    }
}
