using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class SpriterImportSettingsWindow : EditorWindow
{
    public class SpriterImportConfig
    {
        public string asset;
        public bool useUi;
    }

    List<string> assets;
    List<SpriterImportConfig> assetsConfirmed = new List<SpriterImportConfig>();

    public static void OpenWindowFor(string[] assets)
    {
        var window = (SpriterImportSettingsWindow)EditorWindow.GetWindow(typeof(SpriterImportSettingsWindow));
        window.assets = assets.ToList();
    }

    void Update()
    {
        if (assets.Count == 0 && assetsConfirmed.Count > 0)
        {
            assetsConfirmed.ForEach(x => SpriterDotNetUnity.SpriterImporter.CreateSpriter(x.asset, x.useUi));
            assetsConfirmed.Clear();
            Close();
        }
        else if (assets.Count == 0 && assetsConfirmed.Count == 0)
        {
            Close();
        }
    }

    void OnGUI()
    {
        if (assets.Count > 0)
        {
            var asset = assets.First();

            GUILayout.Label("Importin Spriter animation from: " + asset);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Use Sprite Renderer"))
            {
                SetAssetConfigurationsForFirst(false);
            }
            if (GUILayout.Button("Use UI Canvas Renderer"))
            {
                SetAssetConfigurationsForFirst(true);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Use Sprite Renderer for the rest"))
            {
                SetAssetConfigurationsForTheRest(false);
            }
            if (GUILayout.Button("Use UI Canvas Renderer for the rest"))
            {
                SetAssetConfigurationsForTheRest(true);
            }
            GUILayout.EndHorizontal();
        }
    }

    void SetAssetConfigurationsForFirst(bool useUi)
    {
        var asset = assets.First();
        assets.RemoveAt(0);

        assetsConfirmed.Add(new SpriterImportConfig{ asset = asset, useUi = useUi });
    }

    void SetAssetConfigurationsForTheRest(bool useUi)
    {
        assetsConfirmed.AddRange(assets.Select(asset => new SpriterImportConfig{ asset = asset, useUi = useUi }));
        assets.Clear();
    }
}

