using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Thirteen
    {
        public static void Run()
        {
            var lines = GetLines();
            var pairOrderScore = GetOrderScore(lines);
            Console.WriteLine($"Pair order score: {pairOrderScore}");
            var distressSignal = GetDistressSignal(lines);
            Console.WriteLine($"Distress signal: {distressSignal}");
        }

        static int GetDistressSignal(IEnumerable<string> lines)
        {
            var nodes = GetNodes(lines);
            var dividers = GetNodes(new[] { "[[2]]", "[[6]]" }).ToArray();
            var sorted = nodes.Concat(dividers).Order(new NodeComparer()).ToList();
            var divider0Index = sorted.IndexOf(dividers[0]);
            var divider1Index = sorted.IndexOf(dividers[1]);
            return (divider0Index + 1) * (divider1Index + 1);
        }

        static IEnumerable<Node> GetNodes(IEnumerable<string> lines)
        {
            return lines.Where(x => !string.IsNullOrWhiteSpace(x)).Select(ParsePacket);
        }

        static int GetOrderScore(IEnumerable<string> lines)
        {
            var score = 0;
            var pairNumber = 1;
            Node? left = null;
            foreach (var node in GetNodes(lines))
            {
                if (left == null)
                {
                    left = node;
                    continue;
                }

                if (ComparePackets(left, node) != 1)
                {
                    score += pairNumber;
                }

                pairNumber++;
                left = null;
            }

            return score;
        }

        /// <summary>
        /// Compares two packets for equality
        /// </summary>
        /// <returns>-1 if left < right (in order), 1 if right < left (out of order), or 0 if equal</returns>
        /// <exception cref="InvalidOperationException"></exception>
        static int ComparePackets(Node left, Node right)
        {
            if (left.Children == null && right.Children == null)
            {
                Debug.Assert(right.Value != null);
                Debug.Assert(left.Value != null);
                return Math.Sign(left.Value.Value - right.Value.Value);
            }
            else if (left.Children != null && right.Children != null)
            {
                // Both children are lists. Iterate through and compare.
                for (var i = 0; i < left.Children.Count; i++)
                {
                    var isRightDone = i > right.Children.Count - 1;
                    if (isRightDone)
                    {
                        // If right finishes before left then we're out of order.
                        return 1;
                    }

                    // Neither side is done. Compare the children pairwise.
                    var comparison = ComparePackets(left.Children[i], right.Children[i]);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }

                // The lists are equal if they are the same length, otherwise the right list is longer
                // and they are in order.
                return right.Children.Count == left.Children.Count ? 0 : -1;
            }
            else if (left.Children == null)
            {
                // If we compare a value to a list, convert the value to a single-element list and compare.
                var leftList = new Node();
                leftList.AddChild(left);
                return ComparePackets(leftList, right);
            }
            else if (right.Children == null)
            {
                // Same as above
                var rightList = new Node();
                rightList.AddChild(right);
                return ComparePackets(left, rightList);
            }

            throw new InvalidOperationException("unreachable comparison branch reached");
        }


        static Node ParsePacket(string line)
        {
            var nodes = new Stack<Node>();
            var incompleteToken = "";
            foreach (var character in line)
            {
                void TryCompleteToken()
                {
                    if (incompleteToken != "")
                    {
                        nodes.Peek().AddChild(new Node(int.Parse(incompleteToken)));
                        incompleteToken = "";
                    }
                }

                switch (character)
                {
                    case '[':
                        TryCompleteToken();
                        var current = new Node();
                        if (nodes.TryPeek(out var parent))
                        {
                            parent.AddChild(current);
                        }
                        nodes.Push(current);
                        break;
                    case ',':
                        TryCompleteToken();
                        break;
                    case ']':
                        TryCompleteToken();
                        var last = nodes.Pop();
                        if (!nodes.Any())
                        {
                            return last;
                        }
                        break;
                    case var digit:
                        incompleteToken += digit;
                        break;
                }
            }

            throw new InvalidOperationException("invalid packet format");
        }

        class Node
        {
            public Node()
            {
                Children = new List<Node>();
            }

            public Node(int value)
            {
                Value = value;
            }

            public int? Value { get; }
            public List<Node>? Children { get; }

            public void AddChild(Node node)
            {
                if (Children == null)
                {
                    throw new InvalidOperationException("cannot add child to leaf node");
                }

                Children.Add(node);
            }
        }

        class NodeComparer : IComparer<Node>
        {
            public int Compare(Node? x, Node? y)
            {
                if (x == null || y == null)
                {
                    throw new NotSupportedException("neither x nor y can be null");
                }

                return ComparePackets(x, y);
            }
        }
    }
}
