using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("FAQ")]
    public class FAQ
    {
        public int ID { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
} 