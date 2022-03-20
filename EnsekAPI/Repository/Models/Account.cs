using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace EnsekAPI.Repository.Models
{
    public partial class Account
    {
        public Account()
        {
            MeterReadings = new HashSet<MeterReading>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [InverseProperty(nameof(MeterReading.Account))]
        public virtual ICollection<MeterReading> MeterReadings { get; set; }
    }
}
