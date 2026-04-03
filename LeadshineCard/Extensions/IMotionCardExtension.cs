using LeadshineCard.Core.Interfaces;

namespace LeadshineCard.Extensions;

public static class IMotionCardExtension
{
    public static IEnumerable<IAxisController> GetAxises(this IMotionCard card)
    {
        var axiscount = card.GetCardInfo().TotalAxes;
        List<IAxisController> axises = [];
        for (var i = 0; i < axiscount; i++)
        {
            axises.Add(card.GetAxisController((ushort)i));
        }
        return axises;
    }
}
