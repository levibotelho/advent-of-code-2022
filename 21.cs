using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class TwentyOne
    {
        public static void Run()
        {
            var lines = GetLines();
            var monkeys = lines.Select(x => new Monkey(x)).ToDictionary(x => x.Name, x => x);
            var result = GetResult(monkeys);
            Console.WriteLine(result);
        }

        static long GetResult(Dictionary<string, Monkey> monkeys)
        {
            static long Resolve(string name, Dictionary<string, Monkey> monkeys)
            {
                var monkey = monkeys[name];
                return monkey.ConstValue
                    ?? monkey.Operation!(
                        Resolve(monkey.Arg1!, monkeys),
                        Resolve(monkey.Arg2!, monkeys)
                    );
            }

            return Resolve("root", monkeys);
        }

        class Monkey
        {
            public Monkey(string line)
            {
                var split = line.Split(':');
                Name = split[0];
                var operation = split[1].Trim().Split(' ');
                if (operation.Length == 1)
                {
                    ConstValue = long.Parse(operation[0]);
                }
                else if (operation.Length == 3)
                {
                    Arg1 = operation[0];
                    Arg2 = operation[2];
                    Operation = operation[1] switch
                    {
                        "+" => (a, b) => a + b,
                        "-" => (a, b) => a - b,
                        "*" => (a, b) => a * b,
                        "/" => (a, b) => a / b,
                        _ => throw new ArgumentException("invalid operation")
                    };
                }
                else
                {
                    Debug.Fail("invalid number of arguments");
                }
            }

            public string Name { get; }
            public long? ConstValue { get; }
            public string? Arg1 { get; }
            public string? Arg2 { get; }
            public Func<long, long, long>? Operation { get; }
        }
    }
}
