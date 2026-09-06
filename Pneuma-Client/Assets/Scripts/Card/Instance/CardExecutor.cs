using System.Collections.Generic;
using Pneuma.Unit;
using UnityEngine;

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
                    foreach (Enemy enemy in GetTargetEnemies(skill, targetEnemy))
                    {
                        for (int hit = 0; hit < skill.HitCount; hit++)
                        {
                            enemy.TakeDamage(skill.Value);
                        }
                    }
                    break;

                case SkillType.Block:
                case SkillType.GrantBlock:
                    BattleManager.Instance?.CurrentPlayer?.AddShield(skill.Value);
                    break;

                case SkillType.Heal:
                    BattleManager.Instance?.CurrentPlayer?.Heal(skill.Value);
                    break;

                case SkillType.EnergyHeal:
                    BattleManager.Instance?.CurrentPlayer?.RecoverEnergy(skill.Value);
                    break;

                default:
                    // 드로우, 카드 생성, 상태이상, 패시브는 해당 전투 시스템 연결 시 처리합니다.
                    break;
            }
        }
    }

    private static IEnumerable<Enemy> GetTargetEnemies(CardSkill skill, Enemy selectedEnemy)
    {
        IReadOnlyList<Enemy> enemies = BattleManager.Instance?.Enemies;

        if (skill.Target == SkillTargetType.AllEnemy && enemies != null)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];

                if (enemy == null || enemy.IsDead)
                    continue;

                yield return enemy;
            }

            yield break;
        }

        if (skill.Target == SkillTargetType.RandomEnemy && enemies != null)
        {
            List<Enemy> aliveEnemies = new List<Enemy>();

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];

                if (enemy == null || enemy.IsDead)
                    continue;

                aliveEnemies.Add(enemy);
            }

            if (aliveEnemies.Count > 0)
            {
                yield return aliveEnemies[Random.Range(0, aliveEnemies.Count)];
                yield break;
            }
        }

        if (selectedEnemy != null && !selectedEnemy.IsDead)
        {
            yield return selectedEnemy;
        }
    }
}
