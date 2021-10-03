using NStandard;
using System;
using System.Linq;

namespace AntdSharp
{
    public static class TableSource
    {
        public static TableSource<TRow> Create<TRow>(TRow[] rows)
        {
            return new TableSource<TRow>(rows);
        }

        public static TableSource<TRow> Create<TRow, TKey>(TRow[] rows, int page, int pageSize, int pageCount)
        {
            return new TableSource<TRow>(rows, page, pageSize, pageCount);
        }
    }

    public class TableSource<TRow>
    {
        public TableSource(TRow[] rows)
        {
            Rows = rows;
            Columns = GetDefaultColumns();
            Width = GetWidth();
        }

        public TableSource(TRow[] rows, int page, int pageSize, int pageCount)
        {
            Rows = rows;
            Page = page;
            PageSize = pageSize;
            PageCount = pageCount;
            Columns = GetDefaultColumns();
            Width = GetWidth();
        }

        public TableColumn[] Columns { get; set; }
        public TRow[] Rows { get; set; }
        public int? Width { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public int? PageCount { get; set; }

        private TableColumn[] GetDefaultColumns()
        {
            var columns = typeof(TRow).GetProperties().ToArray().Select(p => new TableColumn
            {
                Title = p.Name,
                Key = p.Name,
                DataIndex = StringEx.CamelCase(p.Name),
                Width = Rows.Any() ? Any.Create(() =>
                {
                    if (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?)) return (int)(19 * 6.5);
                    else return (int)(Rows.Max(row => p.GetValue(row)?.ToString().GetLengthA() ?? 0) * 6.5);
                }) + 18 : 18,
            }).Concat(new[]
            {
                new TableColumn
                {
                    Title = "",
                    Key = "",
                    DataIndex = "",
                    Width = 18,
                }
            }).ToArray();
            return columns;
        }

        private int? GetWidth() => Columns.Sum(x => x.Width) + Columns.Length * 18;

    }
}
