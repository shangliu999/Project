using ETexsys.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.DAL
{

    public class EFContextFactory
    {
        public static DbContext GetCurrentDbContext()
        {
            DbContext dbContext = CallContext.GetData("DbContext") as DbContext;

            if (dbContext == null)
            {
                dbContext = new laundry_hotelEntities ();

                CallContext.SetData("DbContext", dbContext);
            }

            dbContext.Configuration.ProxyCreationEnabled = false;

            return dbContext;
        }
    }
}
