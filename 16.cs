using System;
using System.Text.RegularExpressions;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static partial class Sixteen
    {
        public static void Run()
        {
            // 1. Parse lines
            // 2. Construct graph of all nodes
            // 3. Construct shortest path calculations between (nonzero + root) nodes
            // 4. DFS from root to every nonzero node, followed by every other nonzero node, etc.
            //    stopping once we hit 30 iterations. At each step calculate the total flow over
            //    all remaining steps from the current node and all parent nodes.
            // 5. Take the highest flow rate.

            var lines = GetLines();
            var valves = ParseValves(lines);
        }

        static Valve[] ParseValves(IEnumerable<string> lines)
        {
            var regex = ValveRegex();
            var valves = new Dictionary<string, Valve>();
            foreach (var line in lines)
            {
                var values = regex.Match(line);
                var id = values.Captures[1].Value;
                var flowRate = int.Parse(values.Captures[2].Value);
                var connections = values.Captures[3].Value.Split(',').Select(x => x.Trim()).ToArray();
            }
        }

        class Valve
        {
            public required string Id { get; init; }
            public required int FlowRate { get; init; }
            public required List<Valve> Connections { get; init; }
        }

        [GeneratedRegex("Valve (\\w+) has flow rate=(\\d+); tunnels lead to valves (.*)")]
        private static partial Regex ValveRegex();
    }
}
