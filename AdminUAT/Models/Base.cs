using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models
{
    public class Base
    {
        public Guid Id { get; set; }
        public Status Status { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public enum Status
    {
        Active,
        Lock,
        Deleted,
    }

    public class BaseCatalog : Base
    {
        [StringLength(250)]
        public string Value { get; set; }
    }
}
