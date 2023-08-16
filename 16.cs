using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static partial class Sixteen
    {
        public static void Run()
        {
            var lines = GetLines();
            var valves = ParseValves(lines);
            var pathLengths = GetPathLengths(valves);
            var greatestFlow = GetGreatestFlow(valves, pathLengths);
            Console.WriteLine($"Greatest flow: {greatestFlow}");
        }

        static int GetGreatestFlow(IReadOnlyDictionary<string, Valve> valves, Dictionary<string, Dictionary<string, int>> pathLengths)
        {
            static int Visit(
                Valve valve,
                int flow,
                int time,
                IEnumerable<Valve> valves,
                Dictionary<string, Dictionary<string, int>> pathLengths
            )
            {
                // Mark the valve as visited so that we don't return to it in future path explorations.
                valve.Visited = true;

                // Turn it on if it yields any flow.
                if (valve.FlowRate > 0)
                {
                    time--;
                    flow += valve.FlowRate * time;
                }

                // Explore all possibilities involving all remaining unvisited valves.
                var max = flow;
                foreach (var next in valves.Where(x => !x.Visited))
                {
                    // We only calculate path lengths to valves with nonzero flow. We don't want
                    // to go to zero-flow valves.
                    if (!pathLengths[valve.Id].TryGetValue(next.Id, out var travelTime))
                    {
                        continue;
                    }

                    var arrivalTime = time - travelTime;

                    // If we arrive with more than one minute left we can potentially extract more flow. If not
                    // then we've calculated the maximum flow obtainable from this branch of the tree.
                    var branchMax = arrivalTime > 1 ? Visit(next, flow, arrivalTime, valves, pathLengths) : flow;
                    max = Math.Max(max, branchMax);
                }

                // We're done exploring all possibilities where this valve is visited. Mark it as unvisited
                // so that it can be revisited at a different time in future path explorations.
                valve.Visited = false;
                return max;
            }

            var flow = 0;
            var remainingMinutes = 30;
            var start = valves["AA"];
            return Visit(start, flow, remainingMinutes, valves.Values, pathLengths);
        }

        static Dictionary<string, Dictionary<string, int>> GetPathLengths(IReadOnlyDictionary<string, Valve> valves)
        {
            var nodes = valves.ToDictionary(x => x.Key, x => new DijkstraNode(x.Value.Id));
            foreach (var valve in valves)
            {
                var node = nodes[valve.Value.Id];
                foreach (var connection in valve.Value.Connections)
                {
                    var connectedNode = nodes[connection.Id];
                    node.Connections.Add(connectedNode);
                }
            }

            var from = new Dictionary<string, Dictionary<string, int>>();
            var sourceValveIds = valves.Values.Where(x => x.Id == "AA" || x.FlowRate > 0).Select(x => x.Id);
            foreach (var id in sourceValveIds)
            {
                CalculateShortestPathTree(nodes[id], nodes.Values);

                // Only get shortest paths for other valves that have a nonzero flow rate. We never
                // want to travel to a zero-flow valve.
                var shortestPaths = nodes.Values
                    .Where(x => valves[x.Id].FlowRate > 0 && x.Id != id)
                    .ToDictionary(x => x.Id, x => x.Distance);
                from[id] = shortestPaths;
            }

            return from;
        }

        static void CalculateShortestPathTree(DijkstraNode start, IReadOnlyCollection<DijkstraNode> nodes)
        {
            static void Visit(DijkstraNode node, HashSet<DijkstraNode> unvisited)
            {
                Debug.Assert(node.Distance != int.MaxValue);
                foreach (var connection in node.Connections)
                {
                    connection.Distance = Math.Min(connection.Distance, node.Distance + 1);
                }

                unvisited.Remove(node);
                var next = unvisited.MinBy(x => x.Distance);
                if (next == null)
                {
                    return;
                }

                Visit(next, unvisited);
            }

            var unvisited = new HashSet<DijkstraNode>(nodes.Count);
            foreach (var node in nodes)
            {
                unvisited.Add(node);
                node.Distance = int.MaxValue;
            }

            start.Distance = 0;
            Visit(start, unvisited);
        }

        static IReadOnlyDictionary<string, Valve> ParseValves(IEnumerable<string> lines)
        {
            var regex = ValveRegex();
            var valves = new Dictionary<string, Valve>();
            foreach (var line in lines)
            {
                var groups = regex.Matches(line)[0].Groups;
                var id = groups[1].Value;
                var flowRate = int.Parse(groups[2].Value);
                var connections = groups[3].Value.Split(',').Select(x => x.Trim()).ToArray();
                if (valves.TryGetValue(id, out var valve))
                {
                    valve.FlowRate = flowRate;
                }
                else
                {
                    valve = new Valve(id, flowRate);
                    valves[id] = valve;
                }

                foreach (var connectionId in connections)
                {
                    if (valves.TryGetValue(connectionId, out var connectionValve))
                    {
                        valve.Connections.Add(connectionValve);
                    }
                    else
                    {
                        connectionValve = new Valve(connectionId);
                        valve.Connections.Add(connectionValve);
                        valves[connectionId] = connectionValve;
                    }
                }
            }

            return valves;
        }

        class Valve
        {
            public Valve(string id)
            {
                Id = id;
            }

            public Valve(string id, int flowRate)
            {
                Id = id;
                FlowRate = flowRate;
            }

            public string Id { get; }
            public int FlowRate { get; set; }
            public List<Valve> Connections { get; } = new();
            public bool Visited { get; set; }
        }

        class DijkstraNode
        {
            public DijkstraNode(string id)
            {
                Id = id;
            }

            public string Id { get; }
            public List<DijkstraNode> Connections { get; } = new();
            public int Distance { get; set; }
        }

        [GeneratedRegex("Valve (\\w+) has flow rate=(\\d+); tunnels? leads? to valves? (.*)")]
        private static partial Regex ValveRegex();
    }
}
