using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Role
    {
        public int RoleID { get; set; }

        [Required, MaxLength(20)]
        public string RoleName { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
