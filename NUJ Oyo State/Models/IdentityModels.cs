using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NUJ_Oyo_State.Models.Data;
using static NUJ_Oyo_State.Models.ApplicationDbContext;

namespace NUJ_Oyo_State.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public string FirstName { get; set; }
        public string OtherNames { get; set; }
        public string Gender { get; set; }
        public System.DateTime? DOB { get; set; }
        public string StateOfOrigin { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string MembershipId { get; set; }
        public string Branch { get; set; }
        public string Designation { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<User, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
            //disable initializer
            Database.SetInitializer<ApplicationDbContext>(null);
        }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


        public class CustomUserRole : IdentityUserRole<int> { }
        public class CustomUserClaim : IdentityUserClaim<int> { }
        public class CustomUserLogin : IdentityUserLogin<int> { }
        public class CustomRole : IdentityRole<int, CustomUserRole>
        {
            public CustomRole() { }
            public CustomRole(string name) { Name = name; }
        }
        public class CustomUserStore : UserStore<User, CustomRole, int,
        CustomUserLogin, CustomUserRole, CustomUserClaim>
        {
            public CustomUserStore(ApplicationDbContext context)
            : base(context)
            {
            }
        }

        public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole>
        {
            public CustomRoleStore(ApplicationDbContext context)
            : base(context)
            {
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("tblUsers").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<CustomUserRole>().ToTable("tblUserRoles");
            modelBuilder.Entity<CustomUserLogin>().ToTable("tblUserLogins");
            modelBuilder.Entity<CustomUserClaim>().ToTable("tblUserClaims").Property(p => p.Id).HasColumnName("UserClaimId");
            modelBuilder.Entity<CustomRole>().ToTable("tblRoles").Property(p => p.Id).HasColumnName("RoleId");
        }

        // Other Database Sets
        public DbSet<MembershipRequestDTO> MembershipRequest { get; set; }
        public DbSet<ImagesDTO> Images { get; set; }
        public DbSet<GalleryDTO> Gallery { get; set; }
        public DbSet<CategoriesDTO> Categories { get; set; }
    }

   
}