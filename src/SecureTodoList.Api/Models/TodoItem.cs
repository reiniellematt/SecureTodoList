using System;
using System.ComponentModel.DataAnnotations;

namespace SecureTodoList.Api.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Title { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public bool IsDone { get; set; } = false;
    }
}
