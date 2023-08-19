using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Nineteen
    {
        public static void Run()
        {
            var lines = GetLines();
        }

        record class Blueprint(
            int OreRobotCostOre,
            int ClayRobotCostOre,
            int ObsidianRobotCostOre,
            int ObsidianRobotCostClay,
            int GeodeRobotCostOre,
            int GeodeRobotCostObsidian
        );

        class Simulation
        {
            readonly Blueprint blueprint;

            public Simulation(Blueprint blueprint)
            {
                this.blueprint = blueprint;
            }

            Simulation(Simulation toClone)
            {
                blueprint = toClone.blueprint;

                OreRobots = toClone.OreRobots;
                ClayRobots = toClone.ClayRobots;
                ObsidianRobots = toClone.ObsidianRobots;
                GeodeRobots = toClone.GeodeRobots;

                Ore = toClone.Ore;
                Clay = toClone.Clay;
                Obsidian = toClone.Obsidian;
                Geode = toClone.Geode;

                Minute = toClone.Minute;
            }

            public int OreRobots { get; private set; }
            public int ClayRobots { get; private set; }
            public int ObsidianRobots { get; private set; }
            public int GeodeRobots { get; private set; }

            public int Ore { get; private set; }
            public int Clay { get; private set; }
            public int Obsidian { get; private set; }
            public int Geode { get; private set; }

            public int Minute { get; private set; }

            public bool TryBuildOreRobot(out Simulation next)
            {
                if (Ore < blueprint.OreRobotCostOre)
                {
                    next = this;
                    return false;
                }

                next = new Simulation(this);
                next.OreRobots++;
                next.Ore -= blueprint.OreRobotCostOre;
                next.Tick();
                return true;
            }

            public void Tick()
            {
                Minute++;
            }
        }
    }
}
