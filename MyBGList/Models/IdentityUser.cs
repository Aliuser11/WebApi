using Microsoft.AspNetCore.Identity;

namespace MyBGList.Models
{
    public class ApiUser : IdentityUser
    {
    }
}

/*Creating the User entity;:
IdentityUser class already contains all the properties we need:
UserName, Password, and so on.*/