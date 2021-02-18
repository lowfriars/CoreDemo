using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreDemo
{
    /// <summary>
    /// Data model for recording Composer information
    /// </summary>
    public partial class Composers
    {
        
        public Composers()
        {
            Works = new HashSet<Works>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(100)")]        // Set to definite number as nvarchar(max) - the default - is invalid for SQLite
        public string Name { get; set; }

        [Required]                                  // This is just documentation as a non-nullable int is always required
        [Display(Name = "Year of Birth")]
        public int YearOfBirth { get; set; }

        [Display(Name = "Year of Death")]
        public int? YearOfDeath { get; set; }       // Nullable type means this won't be required
        public virtual ICollection<Works> Works { get; set; }
    }
}
