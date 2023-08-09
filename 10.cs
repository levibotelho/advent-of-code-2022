using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Ten
    {
        public static void Run()
        {
            var lines = GetLines();
            var registerHistory = GetRegisterHistory(lines);
            var signalCycles = new int[] { 20, 60, 100, 140, 180, 220 };
            // -1 because the value is the signal after the cycle and we want the value during the cycle.
            var signalSum = signalCycles.Select(x => registerHistory[x - 1] * x).Sum();
            Console.WriteLine($"Signal sum: {signalSum}");

            var screenLines = GetScreenLines(registerHistory);
            Console.WriteLine("");
            foreach (var line in screenLines)
            {
                Console.WriteLine(line);
            }
        }

        static IEnumerable<string> GetScreenLines(List<int> registerHistory)
        {
            const int screenWidth = 40;
            return registerHistory
                .Select((val, i) =>
                {
                    var pixel = i % screenWidth;
                    return Math.Abs(val - pixel) <= 1 ? '#' : '.';
                })
                .Chunk(screenWidth)
                .Select(x => new string(x));
        }

        static List<int> GetRegisterHistory(IEnumerable<string> lines)
        {
            var registerVal = 1;
            var registerHistory = new List<int> { registerVal };
            foreach (var line in lines)
            {
                var split = line.Split(" ");
                switch (split)
                {
                    case ["noop"]:
                        registerHistory.Add(registerVal);
                        break;
                    case ["addx", var valueStr] when int.TryParse(valueStr, out var value):
                        registerHistory.Add(registerVal);
                        registerVal += value;
                        registerHistory.Add(registerVal);
                        break;
                    default:
                        throw new ArgumentException($"unhandled instruction: {line}");
                }
            }
            return registerHistory;
        }
    }
}