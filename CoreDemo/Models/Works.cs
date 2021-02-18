
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreDemo
{
    /// <summary>
    /// Data model for recording composer's works
    /// </summary>
    public partial class Works
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(100)")]       // Default of nvarchar(max) unacceptable to SQLite
        public string Title { get; set; }
        [Required]
        [Display(Name = "Year of Composition")]
        public int YearOfComposition { get; set; }
        [Column(TypeName = "nvarchar(1000)")]       // Default of nvarchar(max) unacceptable to SQLite
        public string Description { get; set; }
        public int ComposerId { get; set; }
        [ForeignKey("ComposerId")]
        public Composers Composer { get; set; }
    }
}
