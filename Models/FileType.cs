using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("FileType")]
    public class FileType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<File> Files { get; set; }
    }
} 