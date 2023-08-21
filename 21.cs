using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class TwentyOne
    {
        public static void Run()
        {
            var lines = GetLines().ToArray();
            PrintPart1(lines);
            PrintPart2(lines);
        }

        static void PrintPart2(IEnumerable<string> lines)
        {
            var monkeys = lines.Select(x => new Monkey(x)).ToDictionary(x => x.Name, x => x);
            var humn = monkeys["humn"];
            humn.Value = null;
            humn.IsConst = true;

            Resolve(monkeys["root"], monkeys);

            static void ResolveInverse(
                Monkey monkey,
                Func<long?, long?, long?> resolveArg1,
                Func<long?, long?, long?> resolveArg2,
                Dictionary<string, Monkey> monkeys
            )
            {
                Debug.Assert(monkey != null);
                Debug.Assert(monkey.Arg1 != null);
                Debug.Assert(monkey.Arg2 != null);
                var arg1 = monkeys[monkey.Arg1];
                var arg2 = monkeys[monkey.Arg2];
                Monkey toResolve;
                if (arg1.Value == null && arg2.Value != null)
                {
                    toResolve = arg1;
                    arg1.Value = resolveArg1(monkey.Value, arg2.Value);
                }
                else if (arg1.Value != null && arg2.Value == null)
                {
                    toResolve = arg2;
                    arg2.Value = resolveArg2(arg1.Value, monkey.Value);
                }
                else
                {
                    throw new ArgumentException("both values are either resolved or unresolved");
                }

                if (toResolve.Name == "humn")
                {
                    return;
                }

                Debug.Assert(toResolve.ResolveArg1 != null);
                Debug.Assert(toResolve.ResolveArg2 != null);
                ResolveInverse(toResolve, toResolve.ResolveArg1, toResolve.ResolveArg2, monkeys);
            }

            var root = monkeys["root"];
            Debug.Assert(root.Arg1 != null);
            Debug.Assert(root.Arg2 != null);
            var arg1 = monkeys[root.Arg1];
            var arg2 = monkeys[root.Arg2];
            ResolveInverse(root, (_, _) => arg2.Value, (_, _) => arg1.Value, monkeys);

            Console.WriteLine("HUMN: " + humn.Value);
        }

        static void PrintPart1(IEnumerable<string> lines)
        {
            var monkeys = lines.Select(x => new Monkey(x)).ToDictionary(x => x.Name, x => x);
            var result = Resolve(monkeys["root"], monkeys);
            Console.WriteLine("Root: " + result);
        }

        static long? Resolve(Monkey monkey, Dictionary<string, Monkey> monkeys)
        {
            if (monkey.IsConst)
            {
                Debug.Assert(monkey.Value != null || monkey.Name == "humn");
                return monkey.Value;
            }

            Debug.Assert(monkey.ResolveValue != null);
            monkey.Value ??= monkey.ResolveValue(
                Resolve(monkeys[monkey.Arg1!], monkeys),
                Resolve(monkeys[monkey.Arg2!], monkeys)
            );
            return monkey.Value;
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
                    IsConst = true;
                    Value = long.Parse(operation[0]);
                }
                else if (operation.Length == 3)
                {
                    Arg1 = operation[0];
                    Arg2 = operation[2];
                    switch (operation[1])
                    {
                        case "+":
                            // value = arg1 + arg2
                            ResolveValue = (arg1, arg2) => arg1 == null || arg2 == null ? null : arg1.Value + arg2.Value;
                            // arg1 = value - arg2
                            ResolveArg1 = (value, arg2) => value == null || arg2 == null ? null : value.Value - arg2.Value;
                            // arg2 = value - arg1
                            ResolveArg2 = (arg1, value) => value == null || arg1 == null ? null : value.Value - arg1.Value;
                            break;
                        case "-":
                            // value = arg1 - arg2
                            ResolveValue = (arg1, arg2) => arg1 == null || arg2 == null ? null : arg1.Value - arg2.Value;
                            // arg1 = value + arg2
                            ResolveArg1 = (value, arg2) => value == null || arg2 == null ? null : value.Value + arg2.Value;
                            // arg2 = arg1 - value
                            ResolveArg2 = (arg1, value) => value == null || arg1 == null ? null : arg1.Value - value.Value;
                            break;
                        case "*":
                            // value = arg1 * arg2
                            ResolveValue = (arg1, arg2) => arg1 == null || arg2 == null ? null : arg1.Value * arg2.Value;
                            // arg1 = value / arg2
                            ResolveArg1 = (value, arg2) => value == null || arg2 == null ? null : value.Value / arg2.Value;
                            // arg2 = value / arg1
                            ResolveArg2 = (arg1, value) => value == null || arg1 == null ? null : value.Value / arg1.Value;
                            break;
                        case "/":
                            // value = arg1 / arg2
                            ResolveValue = (arg1, arg2) => arg1 == null || arg2 == null ? null : arg1.Value / arg2.Value;
                            // arg1 = value * arg2
                            ResolveArg1 = (value, arg2) => value == null || arg2 == null ? null : value.Value * arg2.Value;
                            // arg2 = arg1 / value
                            ResolveArg2 = (arg1, value) => value == null || arg1 == null ? null : arg1.Value / value.Value;
                            break;
                        default:
                            throw new ArgumentException("invalid operator");
                    }
                }
                else
                {
                    Debug.Fail("invalid number of arguments");
                }
            }

            public string Name { get; }
            public bool IsConst { get; set; }
            public long? Value { get; set; }
            public string? Arg1 { get; set; }
            public string? Arg2 { get; set; }
            public Func<long?, long?, long?>? ResolveValue { get; }
            public Func<long?, long?, long?>? ResolveArg1 { get; }
            public Func<long?, long?, long?>? ResolveArg2 { get; }
        }
    }
}
