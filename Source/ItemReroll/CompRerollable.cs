using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ItemReroll
{
    // Carries the reroll button. Stores nothing, so removing the mod leaves saves intact.
    public class CompRerollable : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!Prefs.DevMode || !RerollUtility.CanReroll(parent))
            {
                yield break;
            }
            yield return new Command_Action
            {
                defaultLabel = "ItemReroll_Command".Translate(),
                defaultDesc = (RerollUtility.IsRelic(parent) ? "ItemReroll_CommandDescRelic" : "ItemReroll_CommandDesc").Translate(),
                icon = Textures.Reroll,
                action = delegate
                {
                    RerollUtility.Reroll(parent);
                    Messages.Message("ItemReroll_Done".Translate(parent.LabelCap), parent, MessageTypeDefOf.SilentInput, historical: false);
                }
            };
        }
    }

    [StaticConstructorOnStartup]
    public static class Textures
    {
        // Dice icon from Kenney's Board Game Icons (CC0).
        public static readonly Texture2D Reroll = ContentFinder<Texture2D>.Get("UI/Commands/ItemReroll_Dice");
    }
}
