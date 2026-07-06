using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardDataFinderWindow : EditorWindow
{
    private const string SearchRoot = "Assets/SO/Card";

    private readonly List<CardSearchResult> results = new List<CardSearchResult>();
    private string searchText = string.Empty;
    private Vector2 scrollPosition;

    [MenuItem("Tool/Card/Card Data Finder")]
    public static void Open()
    {
        GetWindow<CardDataFinderWindow>("Card Data Finder");
    }

    private void OnEnable()
    {
        RefreshResults();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Card Data Finder", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("카드 표시 이름으로 CardData SO 위치를 찾습니다.", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginChangeCheck();
            searchText = EditorGUILayout.TextField("카드 이름", searchText);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshResults();
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                RefreshResults();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Results: {results.Count}", EditorStyles.miniBoldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (CardSearchResult result in results)
        {
            DrawResult(result);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawResult(CardSearchResult result)
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField(result.CardData, typeof(CardData), false);

                if (GUILayout.Button("Select", GUILayout.Width(64)))
                {
                    Selection.activeObject = result.CardData;
                    EditorGUIUtility.PingObject(result.CardData);
                }
            }

            EditorGUILayout.SelectableLabel(result.AssetPath, EditorStyles.miniLabel, GUILayout.Height(18));
        }
    }

    private void RefreshResults()
    {
        results.Clear();

        string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { SearchRoot });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);
            if (cardData == null)
            {
                continue;
            }

            if (!IsMatch(cardData))
            {
                continue;
            }

            results.Add(new CardSearchResult(cardData, assetPath));
        }

        results.Sort((a, b) => string.Compare(a.CardData.CardName, b.CardData.CardName, StringComparison.Ordinal));
    }

    private bool IsMatch(CardData cardData)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return cardData.CardName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private readonly struct CardSearchResult
    {
        public CardSearchResult(CardData cardData, string assetPath)
        {
            CardData = cardData;
            AssetPath = assetPath;
        }

        public CardData CardData { get; }
        public string AssetPath { get; }
    }
}
