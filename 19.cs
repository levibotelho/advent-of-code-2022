﻿using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Nineteen
    {
        public static void Run()
        {
            // RunPart1();
            RunPart2();
        }

        static void RunPart1()
        {
            var lines = GetLines();
            var blueprints = lines.Select(x => new Blueprint(x));
            var score = blueprints.AsParallel().Select(x => SimulateBest(x, 24)).Select((x, i) => x * (i + 1)).Sum();
            Console.WriteLine($"Score part 1 {score}");
        }

        static void RunPart2()
        {
            var lines = GetLines();
            var blueprints = lines.Select(x => new Blueprint(x)).Take(3);
            var score = blueprints.AsParallel().Select(x => SimulateBest(x, 32)).Aggregate(1, (a, x) => a * x);
            Console.WriteLine($"Score part 2 {score}");
        }

        static int SimulateBest(Blueprint blueprint, int duration)
        {
            static int Simulate(Simulation simulation, int best, int duration)
            {
                if (simulation.IsFinished)
                {
                    return simulation.Geodes;
                }

                var maxGeodes = simulation.Geodes;
                var geodeRobots = simulation.GeodeRobots;
                for (var i = simulation.Minute; i < duration; i++)
                {
                    maxGeodes += geodeRobots;
                    geodeRobots++;
                }

                // We can't do any better than the best even if we add a new geode robot every turn.
                if (maxGeodes < best)
                {
                    return best;
                }

                if (simulation.TryBuildGeodeRobot(out var next))
                {
                    // Always spend obsidian to build geode robots if possible.
                    return Math.Max(best, Simulate(next, best, duration));
                }

                if (simulation.TryBuildObsidianRobot(out next))
                {
                    best = Math.Max(best, Simulate(next, best, duration));
                }

                if (simulation.TryBuildOreRobot(out next))
                {
                    best = Math.Max(best, Simulate(next, best, duration));
                }

                if (simulation.TryBuildClayRobot(out next))
                {
                    best = Math.Max(best, Simulate(next, best, duration));
                }

                return Math.Max(best, Simulate(simulation.Tick(), best, duration));
            }

            var simulation = new Simulation(blueprint, duration);
            return Simulate(simulation, int.MinValue, duration);
        }

        class Simulation
        {
            readonly Blueprint blueprint;
            readonly int duration;

            public Simulation(Blueprint blueprint, int duration)
            {
                this.blueprint = blueprint;
                this.duration = duration;
                OreRobots = 1;
            }

            Simulation(Simulation toClone)
            {
                blueprint = toClone.blueprint;
                duration = toClone.duration;

                OreRobots = toClone.OreRobots;
                ClayRobots = toClone.ClayRobots;
                ObsidianRobots = toClone.ObsidianRobots;
                GeodeRobots = toClone.GeodeRobots;

                Ore = toClone.Ore;
                Clay = toClone.Clay;
                Obsidian = toClone.Obsidian;
                Geodes = toClone.Geodes;

                Minute = toClone.Minute;
            }

            public int OreRobots { get; private set; }
            public int ClayRobots { get; private set; }
            public int ObsidianRobots { get; private set; }
            public int GeodeRobots { get; private set; }

            public int Ore { get; private set; }
            public int Clay { get; private set; }
            public int Obsidian { get; private set; }
            public int Geodes { get; private set; }

            public int Minute { get; private set; }
            public bool IsFinished => Minute == duration;

            public bool TryBuildOreRobot(out Simulation next)
            {
                if (Ore < blueprint.OreRobotCostOre)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.Ore -= blueprint.OreRobotCostOre;
                next.TickMutate();
                next.OreRobots++;
                return true;
            }

            public bool TryBuildClayRobot(out Simulation next)
            {
                if (Ore < blueprint.ClayRobotCostOre)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.Ore -= blueprint.ClayRobotCostOre;
                next.TickMutate();
                next.ClayRobots++;
                return true;
            }

            public bool TryBuildObsidianRobot(out Simulation next)
            {
                if (Ore < blueprint.ObsidianRobotCostOre || Clay < blueprint.ObsidianRobotCostClay)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.Ore -= blueprint.ObsidianRobotCostOre;
                next.Clay -= blueprint.ObsidianRobotCostClay;
                next.TickMutate();
                next.ObsidianRobots++;
                return true;
            }

            public bool TryBuildGeodeRobot(out Simulation next)
            {
                if (Ore < blueprint.GeodeRobotCostOre || Obsidian < blueprint.GeodeRobotCostObsidian)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.Ore -= blueprint.GeodeRobotCostOre;
                next.Obsidian -= blueprint.GeodeRobotCostObsidian;
                next.TickMutate();
                next.GeodeRobots++;
                return true;
            }

            public Simulation Tick()
            {
                if (IsFinished)
                {
                    throw new InvalidOperationException();
                }

                var next = new Simulation(this);
                next.TickMutate();
                return next;
            }

            void TickMutate()
            {
                Ore += OreRobots;
                Clay += ClayRobots;
                Obsidian += ObsidianRobots;
                Geodes += GeodeRobots;
                Minute++;

                Debug.Assert(Ore >= 0);
                Debug.Assert(Clay >= 0);
                Debug.Assert(Obsidian >= 0);
                Debug.Assert(Geodes >= 0);
                Debug.Assert(Minute <= duration);
                Debug.Assert(OreRobots <= duration);
                Debug.Assert(ClayRobots <= duration);
                Debug.Assert(ObsidianRobots <= duration);
                Debug.Assert(GeodeRobots <= duration);
            }
        }

        class Blueprint
        {
            public Blueprint(string spec)
            {
                var titleContentSplit = spec.Trim().Split(':');
                Id = int.Parse(titleContentSplit[0].Trim().Split(' ')[1]);
                var robots = titleContentSplit[1].Trim().Split(".");
                OreRobotCostOre = int.Parse(robots[0].Trim().Split(' ')[4]);
                ClayRobotCostOre = int.Parse(robots[1].Trim().Split(' ')[4]);
                var obsidianSplit = robots[2].Trim().Split(' ');
                ObsidianRobotCostOre = int.Parse(obsidianSplit[4]);
                ObsidianRobotCostClay = int.Parse(obsidianSplit[7]);
                var geodeSplit = robots[3].Trim().Split(' ');
                GeodeRobotCostOre = int.Parse(geodeSplit[4]);
                GeodeRobotCostObsidian = int.Parse(geodeSplit[7]);
            }

            public int Id { get; }
            public int OreRobotCostOre { get; }
            public int ClayRobotCostOre { get; }
            public int ObsidianRobotCostOre { get; }
            public int ObsidianRobotCostClay { get; }
            public int GeodeRobotCostOre { get; }
            public int GeodeRobotCostObsidian { get; }
        }
    }
}
