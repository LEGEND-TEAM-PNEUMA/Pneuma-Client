using Pneuma.UI.Card;
using Pneuma.UI.Combat;
using Pneuma.Unit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 열려 있는 카드 UI 씬(UITestScene)에 전투 시스템을 연결하는 에디터 도구입니다.
///  - Build Battle System : BattleManager + UIBattleCardController를 만들고 씬의 Player·Enemy·CardManager에 연결
///  - Apply Leila Starting Deck : CardManager의 시작 덱을 레일라 기본 덱으로 교체
///
/// UI 오브젝트 생성·배치는 하지 않습니다. 캐릭터와 UI는 씬에 직접 배치한 뒤,
/// 각 뷰 컴포넌트(BattleHUD, UnitStatusView, EnemyIntentView)의 인스펙터에서 참조를 연결하세요.
/// </summary>
public static class BattleSceneBuilder
{
    private const string BuildMenuPath = "Tool/Battle/Build Battle System (Open Scene)";
    private const string DeckMenuPath = "Tool/Battle/Apply Leila Starting Deck (Open Scene)";

    // 레일라 기본 덱. CardMotionTestScene의 BattleCardController 덱과 같은 구성이다.
    private static readonly string[] LeilaStartingDeck =
    {
        "Assets/SO/Card/Attack.asset",
        "Assets/SO/Card/Defense.asset",
        "Assets/SO/Card/DestinedVictory.asset",
        "Assets/SO/Card/StarBlessing.asset",
        "Assets/SO/Card/FateMark.asset",
        "Assets/SO/Card/FutureEcho.asset",
        "Assets/SO/Card/FutureFragment.asset",
        "Assets/SO/Card/LightRevelation.asset",
        "Assets/SO/Card/MomentHourglass.asset",
        "Assets/SO/Card/MultipleFuture.asset",
        "Assets/SO/Card/PrecognitiveDream.asset",
        "Assets/SO/Card/StarShower.asset",
    };

    [MenuItem(BuildMenuPath)]
    public static void Build()
    {
        if (Object.FindFirstObjectByType<BattleManager>() != null)
        {
            EditorUtility.DisplayDialog("Battle Scene Builder",
                "씬에 BattleManager가 이미 있습니다.", "확인");
            return;
        }

        CardManager cardManager = Object.FindFirstObjectByType<CardManager>();
        CardHandView handView = Object.FindFirstObjectByType<CardHandView>();
        Player player = Object.FindFirstObjectByType<Player>();
        Enemy enemy = Object.FindFirstObjectByType<Enemy>();

        if (cardManager == null || handView == null || player == null || enemy == null)
        {
            EditorUtility.DisplayDialog("Battle Scene Builder",
                "CardManager, CardHandView, Player(Leila), Enemy(Rabbit)가 모두 씬에 있어야 합니다.\n" +
                "캐릭터 프리팹을 먼저 배치한 뒤 실행하세요.", "확인");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Build Battle System");
        int undoGroup = Undo.GetCurrentGroup();

        SetBool(cardManager, "autoStartOnPlay", false);
        ApplyStartingDeck(cardManager);

        GameObject system = new GameObject("BattleSystem");
        Undo.RegisterCreatedObjectUndo(system, "Create BattleSystem");

        BattleManager battleManager = system.AddComponent<BattleManager>();
        UIBattleCardController cardController = system.AddComponent<UIBattleCardController>();

        SerializedObject managerSo = new SerializedObject(battleManager);
        SetRef(managerSo, "currentPlayer", player);
        SetRef(managerSo, "battleCardController", cardController);
        SerializedProperty enemies = managerSo.FindProperty("enemies");
        enemies.arraySize = 1;
        enemies.GetArrayElementAtIndex(0).objectReferenceValue = enemy;
        managerSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject controllerSo = new SerializedObject(cardController);
        SetRef(controllerSo, "cardManager", cardManager);
        SetRef(controllerSo, "handView", handView);
        SetRef(controllerSo, "battleManager", battleManager);
        SetRef(controllerSo, "targetEnemy", enemy);
        SetRef(controllerSo, "playerAnimation", player.GetComponent<LeilaAnimationController>());
        controllerSo.ApplyModifiedPropertiesWithoutUndo();

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(system.scene);
        Selection.activeGameObject = system;

        Debug.Log("[BattleSceneBuilder] BattleSystem 연결 완료. UI는 직접 배치하고 뷰 컴포넌트에 참조를 연결하세요.");
    }

    /// <summary>
    /// 씬의 CardManager 덱만 레일라 기본 덱으로 바꿉니다.
    /// </summary>
    [MenuItem(DeckMenuPath)]
    public static void ApplyStartingDeckToOpenScene()
    {
        CardManager cardManager = Object.FindFirstObjectByType<CardManager>();

        if (cardManager == null)
        {
            EditorUtility.DisplayDialog("Battle Scene Builder", "씬에 CardManager가 없습니다.", "확인");
            return;
        }

        ApplyStartingDeck(cardManager);
        EditorSceneManager.MarkSceneDirty(cardManager.gameObject.scene);
    }

    private static void ApplyStartingDeck(CardManager cardManager)
    {
        SerializedObject so = new SerializedObject(cardManager);
        SerializedProperty deck = so.FindProperty("startingDeck");

        if (deck == null)
        {
            Debug.LogError("[BattleSceneBuilder] CardManager에 'startingDeck' 필드가 없습니다.");
            return;
        }

        deck.ClearArray();

        foreach (string path in LeilaStartingDeck)
        {
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (card == null)
            {
                Debug.LogWarning($"[BattleSceneBuilder] 카드 에셋을 찾지 못해 건너뜁니다: {path}");
                continue;
            }

            int index = deck.arraySize;
            deck.InsertArrayElementAtIndex(index);
            deck.GetArrayElementAtIndex(index).objectReferenceValue = card;
        }

        so.ApplyModifiedProperties();
        Debug.Log($"[BattleSceneBuilder] 시작 덱 적용 완료: {deck.arraySize}장");
    }

    private static void SetRef(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty property = so.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"[BattleSceneBuilder] {so.targetObject.GetType().Name}에 '{propertyName}' 필드가 없습니다.");
            return;
        }

        property.objectReferenceValue = value;
    }

    private static void SetBool(Object target, string propertyName, bool value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"[BattleSceneBuilder] {target.GetType().Name}에 '{propertyName}' 필드가 없습니다.");
            return;
        }

        property.boolValue = value;
        so.ApplyModifiedProperties();
    }
}
