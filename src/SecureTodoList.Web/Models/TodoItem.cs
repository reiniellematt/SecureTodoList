using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTodoList.Web.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public bool IsDone { get; set; } = false;
    }
}
