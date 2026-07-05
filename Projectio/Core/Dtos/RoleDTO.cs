using System.ComponentModel.DataAnnotations;

namespace Projectio.Core.Dtos
{
    public class RoleDto
    {

        public string? Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }

    public class RoleInDto : RoleDto
    {
    }

}
