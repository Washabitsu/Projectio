using System.ComponentModel.DataAnnotations;

namespace Projectio.Core.Dtos
{
    public class RoleDTO
    {

        public string? Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }

    public class RoleINDTO : RoleDTO
    {
    }

}
