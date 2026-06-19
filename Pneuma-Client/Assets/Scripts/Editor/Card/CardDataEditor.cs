using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    private SerializedProperty cardName;
    private SerializedProperty cardType;
    private SerializedProperty cardRarity;
    private SerializedProperty cardCharacter;
    private SerializedProperty cardCost;
    private SerializedProperty skillGroupID;
    private SerializedProperty cardMaxCount;
    private SerializedProperty cardOncePerTurn;
    private SerializedProperty cardExhausts;
    private SerializedProperty cardCanProphecy;
    private SerializedProperty cardIsProphecy;
    private SerializedProperty cardIsUpgraded;
    private SerializedProperty cardUpgradeTarget;
    private SerializedProperty cardUpgradedFrom;
    private SerializedProperty cardHasCutscene;
    private SerializedProperty cardCutsceneSound;
    private SerializedProperty cardIllust;
    private SerializedProperty cardDescription;

    private void OnEnable()
    {
        cardName = serializedObject.FindProperty("cardName");
        cardType = serializedObject.FindProperty("cardType");
        cardRarity = serializedObject.FindProperty("cardRarity");
        cardCharacter = serializedObject.FindProperty("cardCharacter");
        cardCost = serializedObject.FindProperty("cardCost");
        skillGroupID = serializedObject.FindProperty("skillGroupID");
        cardMaxCount = serializedObject.FindProperty("cardMaxCount");
        cardOncePerTurn = serializedObject.FindProperty("cardOncePerTurn");
        cardExhausts = serializedObject.FindProperty("cardExhausts");
        cardCanProphecy = serializedObject.FindProperty("cardCanProphecy");
        cardIsProphecy = serializedObject.FindProperty("cardIsProphecy");
        cardIsUpgraded = serializedObject.FindProperty("cardIsUpgraded");
        cardUpgradeTarget = serializedObject.FindProperty("cardUpgradeTarget");
        cardUpgradedFrom = serializedObject.FindProperty("cardUpgradedFrom");
        cardHasCutscene = serializedObject.FindProperty("cardHasCutscene");
        cardCutsceneSound = serializedObject.FindProperty("cardCutsceneSound");
        cardIllust = serializedObject.FindProperty("cardIllust");
        cardDescription = serializedObject.FindProperty("cardDescription");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        BeginSection("기본 정보");
        EditorGUILayout.PropertyField(cardName, new GUIContent("카드 이름"));
        EditorGUILayout.PropertyField(cardType, new GUIContent("카드 타입"));
        EditorGUILayout.PropertyField(cardRarity, new GUIContent("카드 희귀도"));
        EditorGUILayout.PropertyField(cardCharacter, new GUIContent("소속 캐릭터"));
        EditorGUILayout.PropertyField(cardCost, new GUIContent("사용 코스트"));
        EndSection();

        BeginSection("스킬 데이터");
        EditorGUILayout.PropertyField(skillGroupID, new GUIContent("스킬 그룹 ID"));
        EndSection();

        BeginSection("카드 규칙");
        EditorGUILayout.PropertyField(cardMaxCount, new GUIContent("덱 내 최대 수량"));
        EditorGUILayout.PropertyField(cardOncePerTurn, new GUIContent("턴당 1회 제한"));
        EditorGUILayout.PropertyField(cardExhausts, new GUIContent("사용 후 소멸"));
        EditorGUILayout.PropertyField(cardCanProphecy, new GUIContent("예언 등록 가능"));
        EndSection();

        BeginSection("카드 변형");
        EditorGUILayout.PropertyField(cardIsProphecy, new GUIContent("예언 카드 여부"));
        EditorGUILayout.PropertyField(cardIsUpgraded, new GUIContent("강화 카드 여부"));
        EditorGUILayout.PropertyField(cardUpgradeTarget, new GUIContent("강화 대상 카드"));
        EditorGUILayout.PropertyField(cardUpgradedFrom, new GUIContent("강화 전 카드"));
        EndSection();

        BeginSection("컷신");
        EditorGUILayout.PropertyField(cardHasCutscene, new GUIContent("컷신 사용 여부"));
        EditorGUILayout.PropertyField(cardCutsceneSound, new GUIContent("컷신 사운드"));
        EndSection();

        BeginSection("UI / 아트");
        EditorGUILayout.PropertyField(cardIllust, new GUIContent("일러스트"));
        EditorGUILayout.LabelField("카드 설명");
        cardDescription.stringValue = EditorGUILayout.TextArea(cardDescription.stringValue, GUILayout.MinHeight(70));
        EndSection();

        serializedObject.ApplyModifiedProperties();
    }

    private static void BeginSection(string label)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    private static void EndSection()
    {
        EditorGUILayout.EndVertical();
    }
}
