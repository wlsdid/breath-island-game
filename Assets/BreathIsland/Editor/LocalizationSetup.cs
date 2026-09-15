using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace BreathIsland.Editor
{
    // Registers authored tables with Localization/Addressables; never regenerates their entries.
    public static class LocalizationSetup
    {
        private const string Root = "Assets/BreathIsland/Localization";

        [InitializeOnLoadMethod]
        private static void RegisterOnFirstImport()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                    AssetDatabase.LoadAssetAtPath<LocalizationSettings>(Root + "/LocalizationSettings.asset") == null)
                    EnsureConfigured();
            };
        }

        [MenuItem("숨결섬/Localization 설정 검증")]
        public static void EnsureConfigured()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(Root + "/LocalizationSettings.asset");
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(settings, Root + "/LocalizationSettings.asset");
            }
            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            LocalizationSettings.InitializeSynchronously = true;
            foreach (var code in new[] { "ko", "en" })
            {
                var locale = AssetDatabase.LoadAssetAtPath<Locale>(Root + "/" + code + ".asset");
                if (locale == null)
                {
                    locale = Locale.CreateLocale(code);
                    AssetDatabase.CreateAsset(locale, Root + "/" + code + ".asset");
                }
                LocalizationEditorSettings.AddLocale(locale);
            }
            var tables = new List<LocalizationTable>();
            foreach (var code in new[] { "ko", "en" })
            {
                var table = AssetDatabase.LoadAssetAtPath<StringTable>(Root + "/GameText_" + code + ".asset");
                if (table == null) throw new InvalidOperationException("Missing authored table: " + code);
                tables.Add(table);
            }
            var collection = LocalizationEditorSettings.GetStringTableCollection("GameText");
            if (collection == null)
                collection = (StringTableCollection)LocalizationEditorSettings.CreateCollectionFromLooseTables(tables, Root + "/GameText.asset");
            foreach (var table in tables) collection.AddTable(table);
            collection.SetPreloadTableFlag(true);
            var addressables = AddressableAssetSettingsDefaultObject.Settings;
            addressables.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
            EditorUtility.SetDirty(addressables);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("Breath Island localization configured.");
        }
    }
}
