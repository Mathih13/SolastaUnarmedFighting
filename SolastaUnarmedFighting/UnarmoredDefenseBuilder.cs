using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaUnarmedFighting
{
    public static class UnarmoredDefenseBuilder
    {
        public static void BuildAndAdd()
        {
            UnarmoredDefenseFeatBuilder.UnarmoredDefenseVersion(new UnarmoredDefenseData
            {
                Guid = "cd42525f-2efc-4872-9125-73780537998b",
                Attribute = "Constitution",
                CanUseShield = true
            });
            UnarmoredDefenseFeatBuilder.UnarmoredDefenseVersion(new UnarmoredDefenseData
            {
                Guid = "bb61346e-3b47-44aa-8802-84dd58b2fba9",
                Attribute = "Wisdom",
                CanUseShield = false
            });
        }
    }

    internal class UnarmoredDefenseData
    {
        public string Guid { get; set; }
        public string Attribute { get; set; }
        public bool CanUseShield { get; set; }
    }


    internal class UnarmoredDefenseFeatBuilder :
   BaseDefinitionBuilder<FeatDefinition>
    {
        protected UnarmoredDefenseFeatBuilder(string name, UnarmoredDefenseData data)
          : base(DatabaseHelper.FeatDefinitions.ArmorMaster, name, data.Guid)
        {
            var attributeString = GetAttributeStringFromInput(data.Attribute);

            Definition.GuiPresentation.Title = GenerateTitle(attributeString);
            Definition.GuiPresentation.Description = GenerateDescription(data, attributeString);

            Definition.Features.Clear();
        }

        private string GenerateTitle(string attributeString)
        {
            return $"SolastaUnarmoredDefense/&{attributeString}Title";
        }

        private string GenerateDescription(UnarmoredDefenseData data, string attributeString)
        {
            if (data.CanUseShield)
            {
                return $"SolastaUnarmoredDefense/&{attributeString}ShieldTrueDescription";
            }
            else
            {
                return $"SolastaUnarmoredDefense/&{attributeString}ShieldFalseDescription";
            }

        }

        private static string GetAttributeStringFromInput(string attribute)
        {

            foreach (var score in AttributeDefinitions.AbilityScoreNames)
            {
                if (attribute.ToLower() == score.ToLower())
                {
                    return score;
                }
            }

            throw new Exception("Attempted to build Unarmored Defense with illegal attribute: " + attribute);
        }

        private static string GenerateInternalName(string attribute, bool canUseShield)
        {
            return $"UnarmoredDefense({attribute})/shield={canUseShield}";
        }

        public static FeatDefinition CreateAndAddToDB(UnarmoredDefenseData data)
            => new UnarmoredDefenseFeatBuilder(GenerateInternalName(data.Attribute, data.CanUseShield), data).AddToDB();

        public static FeatDefinition UnarmoredDefenseVersion(UnarmoredDefenseData data)
            => CreateAndAddToDB(data);
    }

}
