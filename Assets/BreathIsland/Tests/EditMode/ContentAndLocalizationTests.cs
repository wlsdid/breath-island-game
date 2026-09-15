using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Localization.Tables;

namespace BreathIsland.Tests
{
    public sealed class ContentAndLocalizationTests
    {
        [Test]
        public void CatalogLoadsPersistentAssetsWithOriginalIdsAndBalance()
        {
            var content = DefinitionCatalog.CreateVerticalSlice();
            Assert.That(content.Fish.Select(f => f.id), Is.EqualTo(new[] { "glimmer", "cloud", "coral", "moon", "pearl", "breaker" }));
            Assert.That(content.Fish.Select(f => f.coinValue), Is.EqualTo(new[] { 18, 28, 34, 56, 64, 0 }));
            Assert.That(content.Fish.Select(f => f.materialValue), Is.EqualTo(new[] { 1, 2, 2, 4, 5, 9 }));
            Assert.That(content.Fish.Select(f => f.pullStrength), Is.EqualTo(new[] { 4f, 6f, 7f, 9f, 10f, 13f }));
            foreach (var fish in content.Fish) Assert.That(AssetDatabase.Contains(fish), Is.True);
            foreach (var facility in content.Facilities.Values)
            {
                Assert.That(AssetDatabase.Contains(facility), Is.True);
                Assert.That(facility.coinCosts, Is.EqualTo(new[] { 60, 110 }));
                Assert.That(facility.materialCosts, Is.EqualTo(new[] { 3, 7 }));
            }
            foreach (var npc in content.Npcs.Values) Assert.That(AssetDatabase.Contains(npc), Is.True);
        }

        [Test]
        public void BothTablesContainEveryContentAndUiKey()
        {
            var tables = AssetDatabase.FindAssets("t:StringTable", new[] { "Assets/BreathIsland/Localization" })
                .Select(g => AssetDatabase.LoadAssetAtPath<StringTable>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            Assert.That(tables, Has.Length.EqualTo(2));
            Assert.That(tables[0].Values.Select(e => e.Key), Is.EquivalentTo(tables[1].Values.Select(e => e.Key)));
            var content = DefinitionCatalog.CreateVerticalSlice();
            var keys = content.Fish.SelectMany(f => new[] { f.nameKey, f.descriptionKey })
                .Concat(content.Facilities.Values.Select(f => f.nameKey))
                .Concat(content.Npcs.Values.SelectMany(n => new[] { n.nameKey, n.roleKey }.Concat(n.lineKeys)));
            foreach (var table in tables)
            {
                foreach (var key in keys) Assert.That(table.GetEntry(key)?.Value, Is.Not.Null.And.Not.Empty, key);
                foreach (var entry in table.Values) Assert.That(entry.Value, Is.Not.Null.And.Not.Empty, entry.Key);
                foreach (var file in System.IO.Directory.GetFiles("Assets/BreathIsland/Runtime/UI", "*.cs"))
                    foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(System.IO.File.ReadAllText(file), "Text\\(\"([^\"]+)\"\\)"))
                        Assert.That(table.GetEntry(match.Groups[1].Value), Is.Not.Null, file + ": " + match.Groups[1].Value);
            }
        }
    }
}
