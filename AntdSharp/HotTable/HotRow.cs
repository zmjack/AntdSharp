using System.Collections.Generic;

namespace AntdSharp.HotTable
{
    public static class HotRow
    {
        public static HotRow<TRow> Create<TRow>(TRow row, Dictionary<string, string[]> optionsByColumn = null)
        {
            return new HotRow<TRow>
            {
                Row = row,
                OptionsByColumn = optionsByColumn,
            };
        }
    }

    public class HotRow<TRow>
    {
        public TRow Row { get; set; }
        public Dictionary<string, string[]> OptionsByColumn { get; set; }
    }
}
