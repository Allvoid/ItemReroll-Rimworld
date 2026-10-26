using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace ItemReroll
{
    // Re-rolls an item's random properties using the same rules the game used to generate them.
    // Vanilla seeds trait generation with the thing's ID, so calling its own generator again would
    // give the same result; here the rolls run on the free Rand state instead.
    public static class RerollUtility
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic;

        private static readonly FieldInfo UniqueTraitCountField = typeof(CompUniqueWeapon).GetField("NumTraitsRange", Static);
        private static readonly FieldInfo UniqueColorField = typeof(CompUniqueWeapon).GetField("color", Instance);
        private static readonly FieldInfo UniqueNameField = typeof(CompUniqueWeapon).GetField("name", Instance);
        private static readonly FieldInfo UniqueAccuracyCacheField = typeof(CompUniqueWeapon).GetField("ignoreAccuracyMaluses", Instance);
        private static readonly FieldInfo BladelinkTraitCountField = typeof(CompBladelinkWeapon).GetField("TraitsRange", Static);
        private static readonly MethodInfo BladelinkCanAddTraitMethod = typeof(CompBladelinkWeapon).GetMethod("CanAddTrait", Instance);
        private static readonly FieldInfo EquippableAbilityField = typeof(CompEquippableAbility).GetField("ability", Instance);
        private static readonly FieldInfo ArtAuthorField = typeof(CompArt).GetField("authorNameInt", Instance);

        public static bool CouldEverReroll(ThingDef def)
        {
            return def.HasComp<CompUniqueWeapon>() || def.HasComp<CompBladelinkWeapon>() || def.HasAssignableCompFrom(typeof(CompArt)) || def.MadeFromStuff;
        }

        // A relic's weapon type, material and Legendary quality come from the ideoligion, so they are never touched.
        public static bool IsRelic(Thing thing)
        {
            return thing.StyleSourcePrecept is Precept_Relic;
        }

        public static bool CanReroll(ThingWithComps thing)
        {
            ItemRerollSettings s = ItemRerollMod.Settings;
            bool relic = IsRelic(thing);
            bool unique = thing.TryGetComp<CompUniqueWeapon>() != null;
            if (s.traits && (unique || thing.TryGetComp<CompBladelinkWeapon>() != null))
            {
                return true;
            }
            if (unique && !relic && (s.nameAndColor || (s.quality && thing.TryGetComp<CompQuality>() != null)))
            {
                return true;
            }
            if (s.stuff && !relic && OtherStuffs(thing).Any())
            {
                return true;
            }
            CompArt art = thing.TryGetComp<CompArt>();
            return s.art && art != null && art.CanShowArt;
        }

        public static void Reroll(ThingWithComps thing)
        {
            ItemRerollSettings s = ItemRerollMod.Settings;
            bool relic = IsRelic(thing);
            CompUniqueWeapon unique = thing.TryGetComp<CompUniqueWeapon>();
            CompBladelinkWeapon bladelink = thing.TryGetComp<CompBladelinkWeapon>();

            if (unique != null)
            {
                List<WeaponTraitDef> oldTraits = unique.TraitsListForReading.ToList();
                if (s.traits)
                {
                    RerollUniqueTraits(unique);
                }
                if (s.nameAndColor && !relic)
                {
                    RerollUniqueNameAndColor(unique);
                }
                else
                {
                    KeepForcedColorConsistent(unique, oldTraits);
                }
                if (s.quality && !relic)
                {
                    thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQuality(QualityGenerator.Super), ArtGenerationContext.Outsider);
                }
            }
            if (bladelink != null && s.traits)
            {
                RerollBladelinkTraits(bladelink);
            }
            if (s.stuff && !relic)
            {
                RerollStuff(thing);
            }
            if (s.art)
            {
                RerollArt(thing, unique);
            }
            thing.Notify_ColorChanged();
        }

        private static void RerollUniqueTraits(CompUniqueWeapon comp)
        {
            List<WeaponTraitDef> traits = comp.TraitsListForReading;
            traits.Clear();
            int count = ((IntRange)UniqueTraitCountField.GetValue(null)).RandomInRange;
            for (int i = 0; i < count; i++)
            {
                IEnumerable<WeaponTraitDef> source = DefDatabase<WeaponTraitDef>.AllDefs.Where(comp.CanAddTrait);
                if (source.Any())
                {
                    comp.AddTrait(source.RandomElementByWeight(x => x.commonality));
                }
            }
            UniqueAccuracyCacheField.SetValue(comp, null);

            // Traits with an ability overwrite the ability comp's props; start again from the def's props
            // and drop the cached ability so it is rebuilt from the new trait.
            CompEquippableAbilityReloadable ability = comp.parent.TryGetComp<CompEquippableAbilityReloadable>();
            if (ability != null)
            {
                ability.props = comp.parent.def.comps.FirstOrDefault(p => p is CompProperties_EquippableAbilityReloadable) ?? ability.props;
                EquippableAbilityField.SetValue(ability, null);
            }
            comp.Setup(fromSave: false);
        }

        // Mirrors CompUniqueWeapon.PostPostMake: random weapon colour unless a trait forces one,
        // and a name built from the namer labels, the colour and a trait adjective.
        private static void RerollUniqueNameAndColor(CompUniqueWeapon comp)
        {
            List<string> adjectives = new List<string>();
            ColorDef color = DefDatabase<ColorDef>.AllDefs.Where(c => c.colorType == ColorType.Weapon && c.randomlyPickable).RandomElement();
            foreach (WeaponTraitDef trait in comp.TraitsListForReading)
            {
                if (trait.forcedColor != null)
                {
                    color = trait.forcedColor;
                }
                if (trait.traitAdjectives != null)
                {
                    adjectives.AddRange(trait.traitAdjectives);
                }
            }
            UniqueColorField.SetValue(comp, color);

            GrammarRequest request = new GrammarRequest
            {
                Rules =
                {
                    new Rule_String("weapon_type", comp.Props.namerLabels.RandomElement()),
                    new Rule_String("color", color.label),
                    new Rule_String("trait_adjective", adjectives.Any() ? adjectives.RandomElement() : "")
                }
            };
            foreach (Rule rule in TaleData_Pawn.GenerateRandom(humanLike: true).GetRules("ANYPAWN", request.Constants))
            {
                request.Rules.Add(rule);
            }
            request.Includes.Add(RulePackDefOf.NamerUniqueWeapon);
            string name = NameGenerator.GenerateName(request, null, appendNumberIfNameUsed: false, "r_weapon_name").StripTags();
            UniqueNameField.SetValue(comp, name);
            CompArt art = comp.parent.TryGetComp<CompArt>();
            if (art != null)
            {
                art.Title = name;
            }
        }

        // When the name and colour are kept, the colour still has to follow the traits:
        // a new trait may force its colour, and a colour forced by a removed trait no longer applies.
        private static void KeepForcedColorConsistent(CompUniqueWeapon comp, List<WeaponTraitDef> oldTraits)
        {
            ColorDef current = (ColorDef)UniqueColorField.GetValue(comp);
            ColorDef forced = comp.TraitsListForReading.LastOrDefault(t => t.forcedColor != null)?.forcedColor;
            if (forced != null)
            {
                UniqueColorField.SetValue(comp, forced);
            }
            else if (current != null && oldTraits.Any(t => t.forcedColor == current))
            {
                UniqueColorField.SetValue(comp, DefDatabase<ColorDef>.AllDefs.Where(c => c.colorType == ColorType.Weapon && c.randomlyPickable).RandomElement());
            }
        }

        private static void RerollBladelinkTraits(CompBladelinkWeapon comp)
        {
            Pawn bonded = comp.CodedPawn;
            List<WeaponTraitDef> traits = comp.TraitsListForReading;
            if (bonded != null)
            {
                foreach (WeaponTraitDef trait in traits)
                {
                    trait.Worker.Notify_Unbonded(bonded);
                }
            }
            traits.Clear();
            int count = ((IntRange)BladelinkTraitCountField.GetValue(null)).RandomInRange;
            for (int i = 0; i < count; i++)
            {
                IEnumerable<WeaponTraitDef> source = DefDatabase<WeaponTraitDef>.AllDefs.Where(t => (bool)BladelinkCanAddTraitMethod.Invoke(comp, new object[] { t }));
                if (source.Any())
                {
                    traits.Add(source.RandomElementByWeight(x => x.commonality));
                }
            }
            if (bonded != null)
            {
                if (comp.Biocodable)
                {
                    foreach (WeaponTraitDef trait in traits)
                    {
                        trait.Worker.Notify_Bonded(bonded);
                    }
                }
                else
                {
                    comp.UnCode();
                }
            }
        }

        // Same pool and weights as GenStuff.TryRandomStuffByCommonalityFor, minus the current material.
        private static IEnumerable<ThingDef> OtherStuffs(Thing thing)
        {
            if (!thing.def.MadeFromStuff)
            {
                return Enumerable.Empty<ThingDef>();
            }
            return GenStuff.AllowedStuffsFor(thing.def, TechLevel.Undefined, checkAllowedInStuffGeneration: true).Where(s => s != thing.Stuff);
        }

        private static void RerollStuff(ThingWithComps thing)
        {
            if (!OtherStuffs(thing).TryRandomElementByWeight(s => s.stuffProps.commonality, out ThingDef stuff))
            {
                return;
            }
            float health = (float)thing.HitPoints / thing.MaxHitPoints;
            thing.SetStuffDirect(stuff);
            thing.HitPoints = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.CeilToInt(health * thing.MaxHitPoints), 1, thing.MaxHitPoints);
        }

        private static void RerollArt(ThingWithComps thing, CompUniqueWeapon unique)
        {
            CompArt art = thing.TryGetComp<CompArt>();
            if (art == null || !art.CanShowArt)
            {
                return;
            }
            object author = ArtAuthorField.GetValue(art);
            art.Clear();
            ArtAuthorField.SetValue(art, author);
            art.InitializeArt(ArtGenerationContext.Outsider);
            // A unique weapon's art title is its name.
            string name = unique != null ? (string)UniqueNameField.GetValue(unique) : null;
            if (!name.NullOrEmpty())
            {
                art.Title = name;
            }
        }
    }
}
