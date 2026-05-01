using System.Collections.Generic;

namespace ClinicAppointmentSystem.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
