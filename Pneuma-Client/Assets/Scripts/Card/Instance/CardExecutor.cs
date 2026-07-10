public class CardExecutor
{
    public void Execute(CardInstance card)
    {
        if (card == null)
            return;

        CardData data = card.Data;

        if (data.CardSkills == null)
            return;

        for (int i = 0; i < data.CardSkills.Count; i++)
        {
            CardSkill skill = data.CardSkills[i];

            if (skill == null)
                continue;

            // TODO: 전투 효과 시스템 연결 후 스킬 타입별 효과를 적용합니다.
        }
    }
}
