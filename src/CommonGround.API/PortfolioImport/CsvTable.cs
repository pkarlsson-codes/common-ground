using System.Text;

namespace CommonGround.API.PortfolioImport;

/// <summary>A CSV file read into a header and rows, as a spreadsheet program saves it (RFC 4180).</summary>
/// <remarks>Fields may be quoted; a quoted field may hold commas, line breaks and doubled quotes. Blank lines are skipped.</remarks>
internal sealed class CsvTable
{
    private readonly Dictionary<string, int> _columnIndexes;

    private CsvTable(IReadOnlyList<string> headers, IReadOnlyList<CsvRow> rows)
    {
        Headers = headers;
        Rows = rows;
        _columnIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Count; i++)
        {
            _columnIndexes.TryAdd(headers[i], i);
        }
    }

    /// <summary>Gets the column headers from the first line, trimmed.</summary>
    public IReadOnlyList<string> Headers { get; }

    /// <summary>Gets the rows after the header.</summary>
    public IReadOnlyList<CsvRow> Rows { get; }

    /// <summary>Reads CSV text. The first non-blank line is the header.</summary>
    /// <param name="text">The file's text.</param>
    /// <returns>The table.</returns>
    public static CsvTable Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var records = ReadRecords(text.TrimStart('﻿'));
        if (records.Count == 0)
        {
            return new CsvTable([], []);
        }

        var headers = records[0].Cells.Select(h => h.Trim()).ToList();
        return new CsvTable(headers, [.. records.Skip(1)]);
    }

    /// <summary>Whether a column with this header exists, ignoring case.</summary>
    /// <param name="header">The column header.</param>
    /// <returns>True when the file has the column.</returns>
    public bool HasColumn(string header) => _columnIndexes.ContainsKey(header);

    /// <summary>The trimmed value of a column in a row, or an empty string when the column or cell is missing.</summary>
    /// <param name="row">The row.</param>
    /// <param name="header">The column header.</param>
    /// <returns>The cell's text.</returns>
    public string Value(CsvRow row, string header)
    {
        ArgumentNullException.ThrowIfNull(row);

        return _columnIndexes.TryGetValue(header, out var index) && index < row.Cells.Count
            ? row.Cells[index].Trim()
            : string.Empty;
    }

    private static List<CsvRow> ReadRecords(string text)
    {
        var records = new List<CsvRow>();
        var cells = new List<string>();
        var cell = new StringBuilder();
        var inQuotes = false;
        var line = 1;
        var recordLine = 1;

        void EndRecord()
        {
            cells.Add(cell.ToString());
            cell.Clear();
            if (cells.Exists(c => c.Trim().Length > 0))
            {
                records.Add(new CsvRow(recordLine, [.. cells]));
            }

            cells.Clear();
        }

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (inQuotes)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
                }
                else
                {
                    if (c == '\n')
                    {
                        line++;
                    }

                    cell.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    break;
                case ',':
                    cells.Add(cell.ToString());
                    cell.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    EndRecord();
                    line++;
                    recordLine = line;
                    break;
                default:
                    cell.Append(c);
                    break;
            }
        }

        EndRecord();
        return records;
    }
}

/// <summary>One CSV record.</summary>
/// <param name="line">The line the record starts on, counting the header as line 1. Spreadsheets show this as the row number.</param>
/// <param name="cells">The record's fields, untrimmed.</param>
internal sealed class CsvRow(int line, IReadOnlyList<string> cells)
{
    /// <summary>Gets the line the record starts on.</summary>
    public int Line { get; } = line;

    /// <summary>Gets the record's fields.</summary>
    public IReadOnlyList<string> Cells { get; } = cells;
}
