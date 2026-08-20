namespace Pneuma.UI.Card
{
    // 런타임에 존재하는 카드 1장입니다.
    // CardData는 카드 "종류"이므로 같은 카드를 2장 보유하면 서로 구분되지 않습니다.
    // 더미 이동·손패 오브젝트 매칭에서 장 단위로 구분하기 위해 고유 ID를 부여합니다.
    public class RuntimeCard
    {
        private static int nextId = 0;

        public int Id { get; }
        public CardData Data { get; }

        public RuntimeCard(CardData data)
        {
            Id = nextId++;
            Data = data;
        }

        public string CardName => Data != null ? Data.CardName : "Unknown";
    }
}
