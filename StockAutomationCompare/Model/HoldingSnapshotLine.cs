using System.Numerics;

namespace StockAutomationCompare.Model;

public readonly struct HoldingSnapshotLine
{
    public DateTime Date { get; } // apparently no `Date` type
    public string Fund { get; }
    public string CompanyName { get; }
    public string Ticker { get; }
    public string Cusip { get; }
    public BigInteger Shares { get; }
    public decimal MarketValueUsd { get; }
    public decimal Weight { get; }

    private HoldingSnapshotLine(DateTime date, string fund, string companyName, string ticker, string cusip,
        BigInteger shares, decimal marketValueUsd, decimal weight)
    {
        Date = date;
        Fund = fund;
        CompanyName = companyName;
        Ticker = ticker;
        Cusip = cusip;
        Shares = shares;
        MarketValueUsd = marketValueUsd;
        Weight = weight;
    }

    private HoldingSnapshotLine(DateTime date, string fund, string companyName, string ticker, string cusip)
    {
        Date = date;
        Fund = fund;
        CompanyName = companyName;
        Ticker = ticker;
        Cusip = cusip;
        Shares = 0;
        MarketValueUsd = 0;
        Weight = 0;
    }

    public static HoldingSnapshotLine Create(DateTime date, string fund, string company, string ticker, string cusip,
        BigInteger shares, decimal marketValueUsd, decimal weight)
    {
        // todo assertions
        return new HoldingSnapshotLine(date, fund, company, ticker, cusip, shares, marketValueUsd, weight);
    }

    public static HoldingSnapshotLine CreateDefaultFrom(HoldingSnapshotLine other)
    {
        return new HoldingSnapshotLine(other.Date, other.Fund, other.CompanyName, other.Ticker, other.Cusip);
    }

    public override string ToString()
    {
        return
            $"HoldingSnapshotLine({Date}, {Fund}, {CompanyName}, {Ticker}, {Cusip}, {Shares}, {MarketValueUsd}, {Weight})";
    }
}
