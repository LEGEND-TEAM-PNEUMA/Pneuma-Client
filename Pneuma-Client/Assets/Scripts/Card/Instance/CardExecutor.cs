using Pneuma.Unit;

public class CardExecutor
{
    public void Execute(CardInstance card, Enemy targetEnemy)
    {
        if (card == null)
            return;

        CardData data = card.Data;

        if (data == null || data.CardSkills == null)
            return;

        for (int i = 0; i < data.CardSkills.Count; i++)
        {
            CardSkill skill = data.CardSkills[i];

            if (skill == null)
                continue;

            switch (skill.SkillType)
            {
                case SkillType.Damage:
                    // 테스트용으로 공격 카드만 더미 적 피해 API에 연결합니다.
                    targetEnemy?.TakeDamage(skill.Value);
                    break;

                default:
                    // TODO: 대상 지정, 회복, 방어도 등 나머지 스킬 타입 처리를 확장합니다.
                    break;
            }
        }
    }
}
