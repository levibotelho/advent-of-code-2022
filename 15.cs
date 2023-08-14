using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static partial class Fifteen
    {
        [GeneratedRegex("(-?\\d+)")]
        private static partial Regex MyRegex();

        public static void Run()
        {
            // Run(10, 20);
            Run(2000000, 4000000);
        }

        static void Run(int pointCountLine, int maxBeaconCoordinate)
        {
            var lines = GetLines();
            var readings = GetReadings(lines).ToArray();
            CalculateReachablePoints(readings, pointCountLine);
            CalculateTuningFrequency(readings, maxBeaconCoordinate);
        }

        static void CalculateTuningFrequency(IEnumerable<Reading> readings, int maxBeaconCoordinate)
        {
            var sw = new Stopwatch();
            sw.Start();
            var fixedStore = new RangedPointStore();
            for (var i = 0; i <= maxBeaconCoordinate; i++)
            {
                GetReachablePointsOnLine(readings, fixedStore, i);
                var missing = fixedStore.GetMissingPoint(maxBeaconCoordinate);
                if (missing != -1)
                {
                    Console.WriteLine($"Frequency: {(long)missing * 4000000 + i}");
                    Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");
                    return;
                }

                fixedStore.Clear();
            }
        }

        static void CalculateReachablePoints(IEnumerable<Reading> readings, int pointCountLine)
        {
            var dynamicStore = new DynamicPointStore();
            GetReachablePointsOnLine(readings, dynamicStore, pointCountLine);
            var sensorBeaconXCoordinates = GetSensorBeaconXCoordinates(readings, pointCountLine);
            foreach (var point in sensorBeaconXCoordinates)
            {
                dynamicStore.Remove(point);
            }

            var reachablePoints = dynamicStore.GetPoints().Count();
            Console.WriteLine($"Reachable points: {reachablePoints}");
        }

        static void GetReachablePointsOnLine(IEnumerable<Reading> readings, IPointStore pointStore, int line)
        {
            foreach (var reading in readings)
            {
                var distance = reading.Sensor.ManhattanDistance(reading.Beacon);
                var pointOnLine = new Point(reading.Sensor.X, line);
                var pointBeforeLineDistance = reading.Sensor.ManhattanDistance(pointOnLine);
                var additionalDistance = distance - pointBeforeLineDistance;
                var from = reading.Sensor.X - additionalDistance;
                var to = reading.Sensor.X + additionalDistance;
                pointStore.AddRange(from, to);
            }
        }

        static IEnumerable<int> GetSensorBeaconXCoordinates(IEnumerable<Reading> readings, int line)
        {
            var sensorBeaconXCoordinates = new HashSet<int>();
            foreach (var reading in readings)
            {
                if (reading.Beacon.Y == line)
                {
                    sensorBeaconXCoordinates.Add(reading.Beacon.X);
                }

                if (reading.Sensor.Y == line)
                {
                    sensorBeaconXCoordinates.Add(reading.Sensor.X);
                }
            }
            return sensorBeaconXCoordinates;
        }

        static IEnumerable<Reading> GetReadings(IEnumerable<string> lines)
        {
            var regex = MyRegex();
            return lines.Select(x =>
            {
                var matches = regex.Matches(x);
                var coordinates = matches.Select(x => int.Parse(x.Value)).ToArray();
                return new Reading(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
            });
        }

        readonly struct Reading
        {
            public Reading(int sensorX, int sensorY, int beaconX, int beaconY)
            {
                Sensor = new Point(sensorX, sensorY);
                Beacon = new Point(beaconX, beaconY);
            }

            public Point Sensor { get; }
            public Point Beacon { get; }
        }

        readonly struct Point
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public int ManhattanDistance(Point other)
            {
                return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
            }
        }


        interface IPointStore
        {
            void AddRange(int fromInclusive, int toInclusive);
        }

        class DynamicPointStore : IPointStore
        {
            readonly HashSet<int> points = new();

            public void AddRange(int fromInclusive, int toInclusive)
            {
                for (var i = fromInclusive; i <= toInclusive; i++)
                {
                    points.Add(i);
                }
            }

            public void Remove(int point)
            {
                points.Remove(point);
            }

            public IEnumerable<int> GetPoints()
            {
                return points;
            }
        }

        class RangedPointStore : IPointStore
        {
            readonly record struct Range(int From, int To);
            readonly List<Range> ranges = new();

            public void AddRange(int fromInclusive, int toInclusive)
            {
                ranges.Add(new Range(fromInclusive, toInclusive));
            }

            public int GetMissingPoint(int max)
            {
                ranges.Sort((a, b) => a.From - b.From);
                var nextMissing = 0;
                var i = 0;
                while (i < ranges.Count)
                {
                    var range = ranges[i];
                    if (range.From <= nextMissing && range.To >= nextMissing)
                    {
                        nextMissing = range.To + 1;
                        i = 0;
                        continue;
                    }

                    i++;
                }

                return nextMissing <= max ? nextMissing : -1;
            }

            public void Clear()
            {
                ranges.Clear();
            }
        }
    }
}
