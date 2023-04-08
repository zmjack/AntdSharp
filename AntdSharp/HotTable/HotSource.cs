using NStandard;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AntdSharp.HotTable
{
    public static class HotSource
    {
        public static HotSource<TRow> Create<TRow>(TRow[] rows, string[] showColumns = null, string[] hideColumns = null) where TRow : class
        {
            return new HotSource<TRow>(rows, showColumns, hideColumns);
        }

        public static HotSource<TRow> Create<TRow>(HotRow<TRow>[] rows, string[] showColumns = null, string[] hideColumns = null) where TRow : class
        {
            return new HotSource<TRow>(rows, showColumns, hideColumns);
        }
    }

    public class HotSource<TRow> where TRow : class
    {
        internal string[] ShowColumns { get; set; }
        internal string[] HideColumns { get; set; }
        private PropertyInfo[] Properties { get; set; }

        public TRow[] Rows { get; set; }
        public string[] ColumnHeaders { get; set; }
        public string[] ColumnEditors { get; set; }
        public HotColumn[] Columns { get; set; }
        public Dictionary<string, string[]> OptionsByCoord { get; set; }
        public Dictionary<string, int> ColumnNumberMap { get; set; }

        private HotSource(string[] showColumns, string[] hideColumns)
        {
            var properties = typeof(TRow).GetProperties()
                .Where(p => !p.HasAttribute<JsonIgnoreAttribute>())
                .Where(p => (!(ShowColumns?.Any() ?? false) || (ShowColumns?.Contains(p.Name) ?? true)))
                .Where(p => !(HideColumns?.Contains(p.Name) ?? false))
                .ToArray();

            Properties = properties;
            ColumnNumberMap = GetColumnNumberMap();

            ShowColumns = showColumns;
            HideColumns = hideColumns;

            ColumnHeaders = properties.Select(p => p.Name).ToArray();
            Columns = ColumnHeaders.Select(x => new HotColumn { Data = StringEx.CamelCase(x) }).ToArray();
        }

        public HotSource(TRow[] rows, string[] showColumns, string[] hideColumns) : this(showColumns, hideColumns)
        {
            Rows = rows;
            OptionsByCoord = new();
            ColumnEditors = new string[Properties.Length].Let(i => "");
        }

        public HotSource(HotRow<TRow>[] hotRows, string[] showColumns, string[] hideColumns) : this(showColumns, hideColumns)
        {
            Rows = hotRows.Select(x => x.Row).ToArray();
            OptionsByCoord = GetOptionsByCoord(hotRows);
            ColumnEditors = Properties.Select(p => hotRows.Any(row => row.OptionsByColumn is not null && row.OptionsByColumn.ContainsKey(p.Name)) ? "dropdown" : string.Empty).ToArray();
        }

        private Dictionary<string, string[]> GetOptionsByCoord(HotRow<TRow>[] hotRows)
        {
            var dict = new Dictionary<string, string[]>();
            foreach (var (index, hotRow) in hotRows.AsIndexValuePairs())
            {
                var optionsByColumn = hotRow.OptionsByColumn;

                if (optionsByColumn is not null)
                {
                    foreach (var option in optionsByColumn)
                    {
                        dict.Add($"({index},{ColumnNumberMap[option.Key]})", option.Value);
                    }
                }
            }
            return dict;
        }

        private Dictionary<string, int> GetColumnNumberMap()
        {
            var dict = new Dictionary<string, int>();
            foreach (var (index, value) in Properties.AsIndexValuePairs())
            {
                dict.Add(value.Name, index);
            }
            return dict;
        }

    }
}
