using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Files")]
    public class File
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        public byte[] FileDate { get; set; }
        [ForeignKey("ID_Messages")]
        public int? ID_Messages { get; set; }
        [ForeignKey("ID_FileType")]
        public int ID_FileType { get; set; }
        public string? URL { get; set; }

        public Message Messages { get; set; }
        public FileType FileType { get; set; }
    }
} 