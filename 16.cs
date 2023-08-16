using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            var pathLengths = GetPathLengths(valves);
            var greatestFlow = GetGreatestFlow(valves, pathLengths);
            Console.WriteLine($"Greatest flow: {greatestFlow}");
        }

        static int GetGreatestFlow(IReadOnlyDictionary<string, Valve> valves, Dictionary<string, Dictionary<string, int>> pathLengths)
        {
            static int Travel(
                Valve current,
                Valve to,
                int flow,
                int time,
                HashSet<Valve> unvisited,
                Dictionary<string, Dictionary<string, int>> pathLengths
            )
            {
                unvisited.Remove(to);
                if (current.FlowRate > 0)
                {
                    time--;
                    flow += current.FlowRate * time;
                }

                var max = flow;
                foreach (var connection in current.Connections.Where(unvisited.Contains))
                {
                    var travelTime = pathLengths[current.Id][connection.Id];
                    var arrivalTime = time - travelTime;

                    // If we arrive with one minute left then we won't be able to travel or get a valve
                    // open before time == 0.
                    var connectionMax = arrivalTime > 1 ? Travel(to, connection, flow, arrivalTime, unvisited, pathLengths) : flow;
                    max = Math.Max(max, connectionMax);
                }

                unvisited.Add(to);
                return max;
            }

            var flow = 0;
            var remainingMinutes = 30;
            var unvisited = new HashSet<Valve>(pathLengths.Keys.Select(x => valves[x]));
            var start = valves["AA"];
            return Travel(start, start, flow, remainingMinutes, unvisited, pathLengths);
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
            foreach (var valve in valves.Where(x => x.Key == "AA" || x.Value.FlowRate != 0))
            {
                var id = valve.Value.Id;
                var node = nodes[id];
                from[id] = GetShortestPaths(node, nodes.Values);
            }

            return from;
        }

        static Dictionary<string, int> GetShortestPaths(DijkstraNode start, IReadOnlyCollection<DijkstraNode> nodes)
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
            return nodes.ToDictionary(x => x.Id, x => x.Distance);
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
