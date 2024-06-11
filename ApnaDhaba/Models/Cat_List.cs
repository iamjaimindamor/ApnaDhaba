using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ApnaDhaba.Models
{
    public class Cat_List
    {
        [Key]
        public int Id { get; set; }

        public List<SelectListItem>? catList { get; set; }
    }
}