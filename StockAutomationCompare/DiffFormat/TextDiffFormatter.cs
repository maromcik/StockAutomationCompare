using System.Numerics;
using System.Text;
using StockAutomationCompare.Diff;

namespace StockAutomationCompare.DiffFormat;

public static class TextDiffFormatter
{

    public static string ChevronUp => "<img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAYAAABytg0kAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAGElEQVR4nGNwOhSqBcIMjgfD3Rz2hbkDADh8BkHeKoTKAAAAAElFTkSuQmCC'>";

    public static string ChevronDown => "<img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAACXBIWXMAAAsTAAALEwEAmpwYAAACGUlEQVR4nO3YXU8aQRQG4DFeNlZsbetXWxP/RYM5o2LjVe3NnkPBX+itiUkD5IwCplq/KK1V/PwnmNm0sCogzu4AG+dN5nazT3bmnc0RwsXFxcXFxcXleSVZzIyBwrxUVF3Me9MijgFeTQDTrlRU9xdjLXYY4NWEZNprIAKYVCk9JeKynaTCnw8Q/xYwng08JumfCdpvh4gFJrntvQHGymOIBkbR6fxWdlIMGkIy/eoWMZCY5fLaWxNEc+FfYG+i/whFVXPEAGAW8+l3oPB3eESjACrA38ZjjQicmeOeYfQWAKY/USOCmKXc19fWEZLxxBZCNteRNQxwdkYqOu8Bov4f83nHexUpYn6L3oPCC8MXqhqfJ6Y9/d/WfwTjid6OYWoamA70r08oRKrofTBF3G8gjTH9MqBo3xjjIxgvTfd3qxr1MaaNx7Sz8iP78kmIhULmo2S6stE4/h0UAvOp9GXEPoLxsJva1BjTGgfG8qMYYJqVCq9tIqK4k0BRqT2mLobMv4RZTeq7KUQjbrZ9sN6DT//UtBum6/16NyuVo7YPTeW80ZZDAxu1GAbDWNNf02wCEkUdRlD3wHjW9VjJx3SehJSjRAQxssM5NRpa+AO2FhORzo1hDwOKTo0nLy3GO0WbiDt3WeAaCIW4MyFRVJQKN5a/r70QPcpCgeZA4Y3+Z9MXqIhzvHVvWN9v/X4PFxcXFxcXEZfcAg05iUIAeTgAAAAAAElFTkSuQmCC'>";

    public static string Format(HoldingsDiff diff, string newLine = "\r\n")
    {
        var newPositions = diff.HoldingsDiffLines.Values
            .Where(hdl => hdl.Old.Shares == 0)
            .OrderBy(hdl => hdl.CompanyName)
            .Select(GetFormattedLine)
            .ToList();
        var increasedPositions = diff.HoldingsDiffLines.Values
            .Where(hdl => hdl.Old.Shares > 0 && hdl.QuantityDiff > 0)
            .OrderBy(hdl => hdl.CompanyName)
            .Select(GetFormattedLine)
            .ToList();
        var reducedPositions = diff.HoldingsDiffLines.Values
            .Where(hdl => hdl.QuantityDiff < 0)
            .OrderBy(hdl => hdl.CompanyName)
            .Select(GetFormattedLine)
            .ToList();

        if (newPositions.Count == 0 && increasedPositions.Count == 0 && reducedPositions.Count == 0)
        {
            return "No changes in the index";
        }

        List<StringBuilder?> sections =
        [
            GetTextSection(newLine, "New positions:", newPositions),
            GetTextSection(newLine, "Increased positions:", increasedPositions),
            GetTextSection(newLine, "Reduced positions:", reducedPositions)
        ];
        var result = new StringBuilder();
        result.AppendJoin(newLine + newLine, sections.Where(s => s != null));
        return result.ToString();
    }

    private static StringBuilder? GetTextSection(string newLine, string title, List<string> lines)
    {
        if (lines.Count == 0) return null;
        var result = new StringBuilder();
        result.Append(title);
        result.Append(newLine);
        result.AppendJoin(newLine, lines);
        return result;
    }

    private static string GetFormattedLine(HoldingsDiffLine diffLine)
    {
        var changeEmoji = diffLine.QuantityDiff > 0 ? "\ud83d\udcc8" : "\ud83d\udcc9";
        string quantityChange;

        if (diffLine.Old.Shares == 0)
        {
            quantityChange = "";
        }
        else
        {
            var changeValue = (decimal) BigInteger.Abs(diffLine.QuantityDiff) / (decimal) diffLine.Old.Shares;
            quantityChange = $" ({changeEmoji}{changeValue:0.00%})";
        }

        return $"{diffLine.CompanyName}, {diffLine.Ticker}, {diffLine.New.Shares}{quantityChange}, {diffLine.New.Weight:0.00%}";
    }
}
