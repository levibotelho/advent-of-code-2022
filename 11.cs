using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Eleven
    {
        public static void Run()
        {
            const int roundCount = 20;

            var lines = GetLines();
            var monkeys = CreateMonkeys(lines);
            for (var i = 0; i < roundCount; i++)
            {
                foreach (var monkey in monkeys)
                {
                    monkey.HandleItems(monkeys);
                }
            }

            var monkeyBusiness = monkeys.OrderByDescending(x => x.HandledCount).Take(2).Aggregate(1, (a, x) => a *= x.HandledCount);
            var top = monkeys.OrderByDescending(x => x.HandledCount).Take(2).ToArray();
            Console.WriteLine($"Monkey business level: {monkeyBusiness}");
        }

        static Monkey[] CreateMonkeys(IEnumerable<string> lines)
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

            return definitions.Where(x => x.Any()).Select(x => new Monkey(x)).OrderBy(x => x.Id).ToArray();
        }

        class Monkey
        {
            const int worryLevelDivisor = 3;

            readonly Queue<Item> items;
            readonly Action<Item> increaseWorryLevel;
            readonly Func<Item, int> test;

            public Monkey(IReadOnlyList<string> definition)
            {
                if (definition.Count != 6)
                {
                    throw new ArgumentException("monkey definition is not six lines long");
                }

                Id = ParseId(definition[0]);
                items = ParseItems(definition[1]);
                increaseWorryLevel = ParseIncreaseWorryLevel(definition[2]);
                test = ParseTest(definition[3], definition[4], definition[5]);
            }

            public int Id { get; }
            public int HandledCount { get; private set; }

            public void HandleItems(Monkey[] monkeys)
            {
                while (items.TryDequeue(out var item))
                {
                    increaseWorryLevel(item);
                    item.WorryLevel /= worryLevelDivisor;
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
                static Func<Item, int> ParseToken(string token)
                {
                    return item => token switch
                    {
                        "old" => item.WorryLevel,
                        _ when int.TryParse(token, out var val) => val,
                        _ => throw new ArgumentOutOfRangeException(nameof(token))
                    };
                }

                static Func<int, int, int> ParseOp(string token)
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
                    x.WorryLevel = level;
                };
            }

            static Func<Item, int> ParseTest(string testLine, string trueLine, string falseLine)
            {
                static int GetLastInt(string line)
                {
                    return int.Parse(line.Split(' ')[^1]);
                }

                var divisibleBy = GetLastInt(testLine);
                var throwToTrue = GetLastInt(trueLine);
                var throwToFalse = GetLastInt(falseLine);
                return x => x.WorryLevel % divisibleBy == 0 ? throwToTrue : throwToFalse;
            }
        }

        class Item
        {
            public Item(int worryLevel)
            {
                WorryLevel = worryLevel;
            }

            public int WorryLevel { get; set; }
        }
    }
}
