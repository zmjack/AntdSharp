using System.ComponentModel.DataAnnotations;

namespace AntdSharp
{
    public class TableColumn
    {
        [Required] public string Key { get; set; }
        [Required] public string DataIndex { get; set; }
        [Required] public string Title { get; set; }
        public int? Width { get; set; }
    }
}
