using StockAutomationCompare.Model;

namespace StockAutomationCompare.Diff;

public class HoldingsDiff
{
    public readonly IDictionary<string, HoldingsDiffLine> HoldingsDiffLines;

    public HoldingsDiff(IEnumerable<HoldingSnapshotLine> oldHoldings, IEnumerable<HoldingSnapshotLine> newHoldings)
    {
        HoldingsDiffLines = CalculateDiff(oldHoldings, newHoldings);
    }

    public HoldingsDiff(ICollection<HoldingsDiffLine> holdingsDiffs)
    {
        ArgumentNullException.ThrowIfNull(holdingsDiffs);

        HoldingsDiffLines = holdingsDiffs.ToDictionary(h => h.Cusip);
    }

    private static Dictionary<string, HoldingsDiffLine> CalculateDiff(IEnumerable<HoldingSnapshotLine> oldHoldings,
        IEnumerable<HoldingSnapshotLine> newHoldings)
    {
        var oldd = oldHoldings.ToDictionary(
            hl => hl.Cusip,
            hl => new HoldingSnapshotLine?(hl)
        );
        var newd = newHoldings.ToDictionary(
            hl => hl.Cusip,
            hl => new HoldingSnapshotLine?(hl)
        );
        var cusips = oldd.Keys.Union(newd.Keys);
        return cusips.ToDictionary(
            t => t,
            t => new HoldingsDiffLine(oldd.GetValueOrDefault(t), newd.GetValueOrDefault(t))
        );
    }
}
