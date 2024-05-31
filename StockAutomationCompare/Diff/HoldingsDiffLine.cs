using System.Numerics;
using StockAutomationCompare.Model;

namespace StockAutomationCompare.Diff;

public readonly struct HoldingsDiffLine
{
    public HoldingSnapshotLine Old { get; }
    public HoldingSnapshotLine New { get; }

    public string CompanyName => New.CompanyName;
    public string Ticker => New.Ticker;
    public string Cusip => New.Cusip;

    public BigInteger QuantityDiff => New.Shares - Old.Shares;

    public HoldingsDiffLine(HoldingSnapshotLine? oldSnapshot, HoldingSnapshotLine? newSnapshot)
    {
        if (oldSnapshot.HasValue && newSnapshot.HasValue)
        {
            Old = oldSnapshot.Value;
            New = newSnapshot.Value;
        }
        else if (oldSnapshot.HasValue)
        {
            Old = oldSnapshot.Value;
            New = HoldingSnapshotLine.CreateDefaultFrom(oldSnapshot.Value);
        }
        else if (newSnapshot.HasValue)
        {
            New = newSnapshot.Value;
            Old = HoldingSnapshotLine.CreateDefaultFrom(newSnapshot.Value);
        }
        else
        {
            throw new InvalidOperationException("Both oldSnapshot and newSnapshot cannot be null");
        }
    }
}
