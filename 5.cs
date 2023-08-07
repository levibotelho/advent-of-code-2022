using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
	public static class Five
	{
		public static void Run()
		{
            var lines = GetLines();
			var diagram = GetDiagram(lines);
			var instructions = GetInstructions(lines);
			ApplyInstructions(diagram, instructions);
			var topCrates = new string(diagram.Select(x => x.Peek()).ToArray());
			Console.WriteLine($"Top crates: {topCrates}");
        }

		static void ApplyInstructions(IReadOnlyList<Stack<char>> diagram, IEnumerable<Instruction> instructions)
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
			public int Count { get; init; }
			public int From { get; init; }
			public int To { get; init; }

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
