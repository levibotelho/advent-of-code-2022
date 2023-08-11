using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Eleven
    {
        public static void Run()
        {
            var lines = GetLines();
            var level3And20 = GetMonkeyBusinessLevel(lines, 3, 20);
            Console.WriteLine($"Monkey business level 3/20: {level3And20}");
            var level1And10000 = GetMonkeyBusinessLevel(lines, 1, 10_000);
            Console.WriteLine($"Monkey business level 1/10000: {level1And10000}");
        }

        static long GetMonkeyBusinessLevel(IEnumerable<string> lines, int worryLevelDivisor, int roundCount)
        {
            var monkeys = CreateMonkeys(lines, worryLevelDivisor);
            var testDivisorCommonFactor = monkeys.Select(x => x.TestDivisor).Aggregate(1L, (a, x) => a * x);
            for (var i = 0; i < roundCount; i++)
            {
                foreach (var monkey in monkeys)
                {
                    monkey.HandleItems(monkeys, testDivisorCommonFactor);
                }
            }

            return monkeys.OrderByDescending(x => x.HandledCount).Take(2).Aggregate(1L, (a, x) => a *= x.HandledCount);
        }

        static Monkey[] CreateMonkeys(IEnumerable<string> lines, int worryLevelDivisor)
        {
            const int definitionSize = 6;
            var definitions = new List<List<string>> { new List<string>(definitionSize) };
            foreach (var line in lines)
            {
                if (line == "")
                {
                    definitions.Add(new List<string>(definitionSize));
                }
                else
                {
                    definitions[^1].Add(line);
                }
            }

            return definitions.Where(x => x.Any()).Select(x => new Monkey(x, worryLevelDivisor)).OrderBy(x => x.Id).ToArray();
        }

        class Monkey
        {
            readonly Queue<Item> items;
            readonly Action<Item> increaseWorryLevel;
            readonly Func<Item, int> test;

            readonly int worryLevelDivisor;

            public Monkey(IReadOnlyList<string> definition, int worryLevelDivisor)
            {
                if (definition.Count != 6)
                {
                    throw new ArgumentException("monkey definition is not six lines long");
                }

                Id = ParseId(definition[0]);
                items = ParseItems(definition[1]);
                increaseWorryLevel = ParseIncreaseWorryLevel(definition[2]);
                TestDivisor = GetLastInt(definition[3]);
                test = ParseTest(TestDivisor, definition[4], definition[5]);
                this.worryLevelDivisor = worryLevelDivisor;
            }

            public int Id { get; }
            public long TestDivisor { get; }
            public long HandledCount { get; private set; }

            public void HandleItems(Monkey[] monkeys, long testDivisorCommonFactor)
            {
                while (items.TryDequeue(out var item))
                {
                    increaseWorryLevel(item);
                    item.WorryLevel /= worryLevelDivisor;
                    // if (worryLevelDivisor == 1)
                    // {
                    item.WorryLevel %= testDivisorCommonFactor;
                    // }
                    var targetMonkey = test(item);
                    monkeys[targetMonkey].PassItem(item);
                    HandledCount++;
                }
            }

            public void PassItem(Item item)
            {
                items.Enqueue(item);
            }

            static int ParseId(string line)
            {
                var idSegment = line.Trim().Split(' ').Last();
                var idString = idSegment[..idSegment.IndexOf(':')];
                return int.Parse(idString);
            }

            static Queue<Item> ParseItems(string line)
            {
                var items = line.Split(':')[1].Split(',').Select(x => new Item(int.Parse(x.Trim())));
                return new Queue<Item>(items);
            }

            static Action<Item> ParseIncreaseWorryLevel(string line)
            {
                static Func<Item, long> ParseToken(string token)
                {
                    return item => token switch
                    {
                        "old" => item.WorryLevel,
                        _ when long.TryParse(token, out var val) => val,
                        _ => throw new ArgumentOutOfRangeException(nameof(token))
                    };
                }

                static Func<long, long, long> ParseOp(string token)
                {
                    return token switch
                    {
                        "+" => (a, b) => a + b,
                        "*" => (a, b) => a * b,
                        _ => throw new ArgumentOutOfRangeException(nameof(token))
                    };
                }

                var tokens = line.Split(' ');
                var arg0Expression = ParseToken(tokens[^3]);
                var arg1Expression = ParseToken(tokens[^1]);
                var opExpression = ParseOp(tokens[^2]);
                return x =>
                {
                    var level = opExpression(arg0Expression(x), arg1Expression(x));
                    if (level < 0)
                    {
                        Debugger.Break();
                    }
                    x.WorryLevel = level;
                };
            }

            static Func<Item, int> ParseTest(long testDivisor, string trueLine, string falseLine)
            {
                var tossToTrue = GetLastInt(trueLine);
                var tossToFalse = GetLastInt(falseLine);
                return x => x.WorryLevel % testDivisor == 0 ? tossToTrue : tossToFalse;
            }

            static int GetLastInt(string line)
            {
                return int.Parse(line.Split(' ')[^1]);
            }
        }

        class Item
        {
            public Item(long worryLevel)
            {
                WorryLevel = worryLevel;
            }

            public long WorryLevel { get; set; }
        }
    }
}
