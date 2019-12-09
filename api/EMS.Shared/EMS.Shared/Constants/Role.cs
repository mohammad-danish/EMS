
namespace EMS.Shared.Constants
{
    public static class Role
    {
        public const string Normal = "Normal User"; 
        public const string Admin = "Admin"; 
        public const string SuperAdmin = "SuperAdmin"; 
        public const string Dev = "Dev";

        public static string AnyOneOf(params string[] roles) => string.Join(',', roles);
    }
}
