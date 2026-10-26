using System.Collections.Generic;
using Verse;

namespace ItemReroll
{
    // Adds CompRerollable to every item def that has something random to re-roll.
    // Runs before any save is loaded, so existing items pick the comp up on load.
    [StaticConstructorOnStartup]
    public static class CompInjector
    {
        static CompInjector()
        {
            int count = 0;
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.category != ThingCategory.Item || !typeof(ThingWithComps).IsAssignableFrom(def.thingClass) || !RerollUtility.CouldEverReroll(def))
                {
                    continue;
                }
                if (def.comps == null)
                {
                    def.comps = new List<CompProperties>();
                }
                def.comps.Add(new CompProperties(typeof(CompRerollable)));
                count++;
            }
            Log.Message("[Item Reroll] Reroll button added to " + count + " item types.");
        }
    }
}
