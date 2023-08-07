using System;
namespace AdventOfCode
{
    public static class Helpers
    {
        public static IEnumerable<string> GetLines()
        {
            Console.WriteLine("Paste input followed by two blank lines...");
            var lines = new List<string>();
            while (lines.Count < 2 || lines[^1] != "" || lines[^2] != "")
            {
                var line = Console.ReadLine();
                if (line != null)
                {
                    lines.Add(line);
                }
            }
            return lines.SkipLast(2);
        }
    }
}

