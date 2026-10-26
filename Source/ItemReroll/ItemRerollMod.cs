using UnityEngine;
using Verse;

namespace ItemReroll
{
    public class ItemRerollSettings : ModSettings
    {
        public bool traits = true;
        public bool nameAndColor = true;
        public bool quality = false;
        public bool stuff = false;
        public bool art = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref traits, "traits", true);
            Scribe_Values.Look(ref nameAndColor, "nameAndColor", true);
            Scribe_Values.Look(ref quality, "quality", false);
            Scribe_Values.Look(ref stuff, "stuff", false);
            Scribe_Values.Look(ref art, "art", false);
        }
    }

    public class ItemRerollMod : Mod
    {
        public static ItemRerollSettings Settings;

        public ItemRerollMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ItemRerollSettings>();
        }

        public override string SettingsCategory()
        {
            return "Item Reroll";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.Label("ItemReroll_SettingsIntro".Translate());
            list.Gap();
            list.CheckboxLabeled("ItemReroll_Traits".Translate(), ref Settings.traits, "ItemReroll_TraitsDesc".Translate());
            list.CheckboxLabeled("ItemReroll_NameAndColor".Translate(), ref Settings.nameAndColor, "ItemReroll_NameAndColorDesc".Translate());
            list.CheckboxLabeled("ItemReroll_Quality".Translate(), ref Settings.quality, "ItemReroll_QualityDesc".Translate());
            list.CheckboxLabeled("ItemReroll_Stuff".Translate(), ref Settings.stuff, "ItemReroll_StuffDesc".Translate());
            list.CheckboxLabeled("ItemReroll_Art".Translate(), ref Settings.art, "ItemReroll_ArtDesc".Translate());
            list.Gap();
            list.Label("ItemReroll_RelicNote".Translate());
            list.End();
        }
    }
}
