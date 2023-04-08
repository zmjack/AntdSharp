using NStandard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntdSharp
{
    public static class TableSource
    {
        public static TableSource<TRow> Create<TRow>(TRow[] rows, string[] showColumns = default, string[] hideColumns = default, int? page = default, int? pageSize = default, int? pageCount = default) where TRow : class
        {
            return new TableSource<TRow>(rows, showColumns, hideColumns, page, pageSize, pageCount);
        }

        public static TableSourceBase Combine<TSource>(TSource[] @this) where TSource : TableSourceBase
        {
            if (!@this.Any(x => x is not null)) return new TableSourceBase();
            @this = @this.Where(x => x is not null).ToArray();

            var rowList = new List<object>();
            foreach (var source in @this)
            {
                foreach (var row in source.Rows) rowList.Add(row);
            }

            var defaultColumns = @this.SelectMany(x => x.Columns).GroupBy(x => x.Key).Select(g => new TableColumn
            {
                Title = g.Key,
                Key = g.Key,
                DataIndex = StringEx.CamelCase(g.Key),
                Width = g.Max(x => x.Width),
            }).ToArray();

            return new TableSourceBase
            {
                Columns = defaultColumns,
                ShowColumns = (from item in @this let cols = item.ShowColumns where cols is not null select cols).SelectMany(x => x).ToArray(),
                HideColumns = (from item in @this let cols = item.HideColumns where cols is not null select cols).SelectMany(x => x).ToArray(),
                Rows = rowList.ToArray(),
            };
        }
    }

    public class TableSourceBase
    {
        public TableColumn[] Columns { get; set; }
        internal string[] ShowColumns { get; set; }
        internal string[] HideColumns { get; set; }

        public Array Rows { get; set; }
        public int? Width { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public int? PageCount { get; set; }
    }

    public class TableSource<TRow> : TableSourceBase where TRow : class
    {
        public new TRow[] Rows { get => base.Rows as TRow[]; }

        public TableSource() : base() { }

        public TableSource(TRow[] rows, string[] showColumns, string[] hideColumns, int? page, int? pageSize, int? pageCount)
        {
            base.Rows = rows;
            Columns = GetColumns();

            Page = page;
            PageSize = pageSize;
            PageCount = pageCount;
            Width = GetWidth();

            ShowColumns = showColumns;
            HideColumns = hideColumns;
        }

        public TableColumn[] GetColumns()
        {
            var columns = typeof(TRow).GetProperties()
                .Where(p => p.Name != "Guid" && (!(ShowColumns?.Any() ?? false) || (ShowColumns?.Contains(p.Name) ?? true)) && !(HideColumns?.Contains(p.Name) ?? false))
                .Select(p =>
                {
                    var nameLengthA = p.Name?.GetLengthA() ?? 0;

                    int width;
                    if (Rows.Any())
                    {
                        if (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?)) width = (int)(19 * 6.5);
                        else width = (int)(new[]
                        {
                            nameLengthA,
                            Rows.Max(row => p.GetValue(row)?.ToString().GetLengthA() ?? 0)
                        }.Max() * 6.5);
                    }
                    else
                    {
                        width = (int)(nameLengthA * 6.5);
                    }

                    return new TableColumn
                    {
                        Title = p.Name,
                        Key = p.Name,
                        DataIndex = StringEx.CamelCase(p.Name),
                        Width = width + 18,
                    };
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
