using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.Infrastructure.Persistence.Configurations;

public class MongoDbSettings
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
}