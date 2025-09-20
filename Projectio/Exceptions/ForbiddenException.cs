using Microsoft.EntityFrameworkCore;
using System;

namespace Projectio.Exceptions
{
    public class ForbiddenException : Exception
    {
        private  HttpContext? _context { get; set; }
        public ForbiddenException() : base("You do not have permission to access this resource.")
        {
           
        }
        public ForbiddenException(string message, HttpContext context) : base(message)
        {
            _context = context;
        }




    }
}
