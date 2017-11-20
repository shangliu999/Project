using ETexsys.APIRequestModel.Request;
using ETextsys.Terminal.Utilities;
using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.DAL
{
    public class LaundryContext : DbContext
    {
        public LaundryContext() : base("name=DefaultConnection")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.AddFromAssembly(typeof(LaundryContext).Assembly);

            if (!File.Exists(Directory.GetCurrentDirectory() + "/DBData/db.sqlite"))
            {
                Database.SetInitializer(new MyDbInitializer(modelBuilder));
            }
        }
        
        public DbSet<Bag> Bags { get; set; }

        public DbSet<BrandType> BrandTypes { get; set; }

        public DbSet<Category> Categorys { get; set; }

        public DbSet<Color> Colors { get; set; }

        public DbSet<Fabric> Fabrics { get; set; }

        public DbSet<Region> Regions { get; set; }

        public DbSet<Scrap> Scraps { get; set; }

        public DbSet<Size> Sizes { get; set; }
        
        public DbSet<Sys_User_Dataview> Sys_User_Dataviews { get; set; }

        public DbSet<Textile> Textiles { get; set; }

        public DbSet<TextileClass> TextileClasses { get; set; }

        public DbSet<ClassSize> ClassSizes { get; set; }

        public DbSet<RFIDTag> RFIDTags { get; set; }

        public DbSet<SendLog> SendLogs { get; set; }
    }

    public class MyDbInitializer : SqliteDropCreateDatabaseAlways<LaundryContext>
    {
        public MyDbInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder) { }

        protected override void Seed(LaundryContext context)
        {
            base.Seed(context);
        }
    }
}
