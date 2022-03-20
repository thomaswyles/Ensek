using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace EnsekAPI.Repository.Models
{
    public partial class MeterReading
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? AccountId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? ReadingDateTime { get; set; } 
        public int? ReadValue { get; set; }

        [ForeignKey(nameof(AccountId))]
        [InverseProperty("MeterReadings")]
        public virtual Account Account { get; set; }
    }
}
