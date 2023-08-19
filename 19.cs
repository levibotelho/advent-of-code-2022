using System;
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

                // We can only build one robot at a time. Don't create more obsidian robots than the
                // amount of obsidian we can spend per minute, which is that which can be used to build
                // a geode robot.
                var maxObsidianRobots = simulation.Blueprint.GeodeRobotCostObsidian;
                if (simulation.ObsidianRobots < maxObsidianRobots && simulation.TryBuildObsidianRobot(out next))
                {
                    best = Math.Max(best, Simulate(next, best, duration));
                }

                // Same logic. Clay can only be used for obsidian robots.
                var maxClayRobots = simulation.Blueprint.ObsidianRobotCostClay;
                if (simulation.ClayRobots < maxClayRobots && simulation.TryBuildClayRobot(out next))
                {
                    best = Math.Max(best, Simulate(next, best, duration));
                }

                // Same logic. Don't create more ore robots than the amount of ore which can be spent
                // on any other robot.
                var maxOreRobots = Math.Max(
                    Math.Max(
                        Math.Max(simulation.Blueprint.OreRobotCostOre, simulation.Blueprint.ClayRobotCostOre),
                        simulation.Blueprint.ObsidianRobotCostOre
                    ),
                    simulation.Blueprint.GeodeRobotCostOre
                );
                if (simulation.OreRobots < maxOreRobots && simulation.TryBuildOreRobot(out next))
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
            readonly int duration;

            public Simulation(Blueprint blueprint, int duration)
            {
                Blueprint = blueprint;
                this.duration = duration;
                OreRobots = 1;
            }

            Simulation(Simulation toClone)
            {
                Blueprint = toClone.Blueprint;
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

            public Blueprint Blueprint { get; private set; }

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
                if (Ore < Blueprint.OreRobotCostOre)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.Ore -= Blueprint.OreRobotCostOre;
                next.TickMutate();
                next.OreRobots++;
                return true;
            }

            public bool TryBuildClayRobot(out Simulation next)
            {
                if (Ore < Blueprint.ClayRobotCostOre)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.Ore -= Blueprint.ClayRobotCostOre;
                next.TickMutate();
                next.ClayRobots++;
                return true;
            }

            public bool TryBuildObsidianRobot(out Simulation next)
            {
                if (Ore < Blueprint.ObsidianRobotCostOre || Clay < Blueprint.ObsidianRobotCostClay)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.Ore -= Blueprint.ObsidianRobotCostOre;
                next.Clay -= Blueprint.ObsidianRobotCostClay;
                next.TickMutate();
                next.ObsidianRobots++;
                return true;
            }

            public bool TryBuildGeodeRobot(out Simulation next)
            {
                if (Ore < Blueprint.GeodeRobotCostOre || Obsidian < Blueprint.GeodeRobotCostObsidian)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.Ore -= Blueprint.GeodeRobotCostOre;
                next.Obsidian -= Blueprint.GeodeRobotCostObsidian;
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
