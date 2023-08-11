using System;
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
        }

        static int GetOrderScore(IEnumerable<string> lines)
        {
            var score = 0;
            var pairNumber = 1;
            Node? left = null;
            foreach (var node in lines.Where(x => !string.IsNullOrWhiteSpace(x)).Select(ParsePacket))
            {
                if (left == null)
                {
                    left = node;
                    continue;
                }

                if (AreInOrder(left, node))
                {
                    score += pairNumber;
                }

                pairNumber++;
                left = null;
            }

            return score;
        }

        static bool AreInOrder(Node left, Node right)
        {
            static bool? AreInOrderInner(Node left, Node right)
            {
                if (left.Children == null && right.Children == null)
                {
                    // Both children are null. The values can be compared directly.
                    if (left.Value < right.Value)
                    {
                        return true;
                    }

                    if (right.Value < left.Value)
                    {
                        return false;
                    }

                    return null;
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
                            return false;
                        }

                        // Neither side is done. Compare the children pairwise.
                        var areInOrder = AreInOrderInner(left.Children[i], right.Children[i]);
                        if (areInOrder == true)
                        {
                            return true;
                        }
                        if (areInOrder == false)
                        {
                            return false;
                        }
                    }

                    // If the lists are the same length then we can't come to a conclusion.
                    if (right.Children.Count == left.Children.Count)
                    {
                        return true;
                    }

                    // The right list is longer. The lists are in order.
                    return true;
                }
                else if (left.Children == null)
                {
                    // If we compare a value to a list, convert the value to a single-element list and compare.
                    var leftList = new Node();
                    leftList.AddChild(left);
                    return AreInOrderInner(leftList, right);
                }
                else if (right.Children == null)
                {
                    // Same as above
                    var rightList = new Node();
                    rightList.AddChild(right);
                    return AreInOrderInner(left, rightList);
                }

                throw new InvalidOperationException("unreachable comparison branch reached");
            }

            // If we can't make a decision then they're equal and in order.
            return AreInOrderInner(left, right) ?? true;
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
    }
}
