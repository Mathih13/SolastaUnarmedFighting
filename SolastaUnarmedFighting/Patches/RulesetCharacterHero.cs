using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace SolastaUnarmedFighting.Patches
{
    [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshArmorClass")]
    internal static class RulesetCharacterHero_CalculateUnarmoredDefense
    {
        // Runs after RefreshArmorClass
        static void Postfix(RulesetCharacterHero __instance)
        {
            var acAttribute = __instance.Attributes["ArmorClass"];
            foreach (var feat in __instance.TrainedFeats)
            {
                AttemptArmorClassBonusApplication(__instance, acAttribute, feat);
            }

            // For future reference, this could trigger on fightingstyles, and pretty much any "BaseDefinition"
            // That updates during RefreshArmorClass
            foreach (var style in __instance.ActiveFightingStyles)
            {
                AttemptArmorClassBonusApplication(__instance, acAttribute, style);
            }

            acAttribute.Refresh();
        }

        private static void AttemptArmorClassBonusApplication(RulesetCharacterHero character, RulesetAttribute acAttribute, BaseDefinition definition)
        {
            // Todo: Make the name more unique to avoid any mod conflicts?
            if (definition.Name.Contains("UnarmoredDefense") && !character.IsWearingArmor())
            {
                var sectionSplit = definition.Name.Split('/');
                var baseName = sectionSplit[0];
                var settings = CreateSettingsDictionary(sectionSplit.Skip(1));
                var attribute = GetAttributeFromFeatName(baseName);
                var attributeModifier = AttributeDefinitions.ComputeAbilityScoreModifier(character.Attributes[attribute].CurrentValue);
                var trendInfo = new RuleDefinitions.TrendInfo(attributeModifier, RuleDefinitions.FeatureSourceType.AbilityScore, $"SolastaUnarmoredDefense/&{attribute}Title", null);

                if (LegalState(settings, character))
                {
                    acAttribute.AddModifier(RulesetAttributeModifier.BuildAttributeModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive, attributeModifier, "16Feat"));
                    acAttribute.ValueTrends.Add(trendInfo);
                }
            }
        }

        private static bool LegalState(Dictionary<string, string> settings, RulesetCharacterHero instance)
        {
            bool legal = true;

            if (!SettingIsTrue(settings["shield"]) && instance.IsWearingShield())
            {
                legal = false;
            }

            return legal;
        }

        private static bool SettingIsTrue(string value)
        {
            return value == "True";
        }

        private static Dictionary<string, string> CreateSettingsDictionary(IEnumerable<string> sectionSplit)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var section in sectionSplit)
            {
                var keyValueSplit = section.Split('=');
                dict.Add(keyValueSplit[0], keyValueSplit[1]);
            }
            return dict;
        }

        private static string GetAttributeFromFeatName(string baseName)
        {
            foreach (var attr in AttributeDefinitions.AbilityScoreNames)
            {
                if (baseName.Contains(attr))
                {
                    return attr;
                }
            }
            throw new Exception($"Unable to find attribute from feat name {baseName}");
        }
    }
}
