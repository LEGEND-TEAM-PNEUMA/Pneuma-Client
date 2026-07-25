public class CardInstance
{
    public CardData Data { get; private set; }
    public int CurrentCost { get; private set; }
    public bool HasFateMark { get; private set; }
    public bool IsRegisteredInProphecySlot { get; private set; }

    public string CardName => Data.CardName;
    public string Description => Data.CardDescription;

    public CardInstance(CardData data)
    {
        Data = data;
        CurrentCost = data != null ? data.CardCost : 0;
    }

    public void SetCurrentCost(int cost)
    {
        CurrentCost = cost < 0 ? 0 : cost;
    }

    public void ResetCurrentCost()
    {
        CurrentCost = Data.CardCost;
    }

    public void SetFateMark(bool hasFateMark)
    {
        HasFateMark = hasFateMark;
    }

    public void SetProphecySlotRegistered(bool isRegistered)
    {
        IsRegisteredInProphecySlot = isRegistered;
    }
}
