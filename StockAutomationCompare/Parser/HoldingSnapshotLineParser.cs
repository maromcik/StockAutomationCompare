using System.Globalization;
using System.Numerics;
using Microsoft.VisualBasic.FileIO;
using StockAutomationCompare.Model;

namespace StockAutomationCompare.Parser;

public static class HoldingSnapshotLineParser
{
    public static IEnumerable<HoldingSnapshotLine> ParseLinesFromFile(string filename)
    {
        return ParseLines(new FileStream(filename, FileMode.Open, FileAccess.Read));
    }

    public static IEnumerable<HoldingSnapshotLine> ParseLinesFromBytes(byte[] bytes)
    {
        return ParseLines(new MemoryStream(bytes));
    }

    private static IEnumerable<HoldingSnapshotLine> ParseLines(Stream stream)
    {
        using var lineParser = new TextFieldParser(stream);
        lineParser.HasFieldsEnclosedInQuotes = true;
        lineParser.SetDelimiters(",");

        lineParser.ReadLine(); // skip header
        while (!lineParser.EndOfData)
        {
            var fields = lineParser.ReadFields() ?? throw new InvalidOperationException("parser returned null");

            if (!DateTime.TryParseExact(fields[0], "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var date)) yield break;
            var fund = fields[1];
            var companyName = fields[2].Replace("\"", "");
            var ticker = fields[3].Replace("\"", "");
            var cusip = fields[4];
            var shares = BigInteger.Parse(fields[5].Replace(",", ""), CultureInfo.InvariantCulture);
            var marketValueUsd =
                decimal.Parse(fields[6].Replace("$", "").Replace(",", ""), CultureInfo.InvariantCulture);
            var weight = decimal.Parse(fields[7].Replace("%", ""), CultureInfo.InvariantCulture) / 100;

            var line = HoldingSnapshotLine.Create(date, fund, companyName, ticker, cusip, shares, marketValueUsd,
                weight);

            yield return line;
        }
    }
}
