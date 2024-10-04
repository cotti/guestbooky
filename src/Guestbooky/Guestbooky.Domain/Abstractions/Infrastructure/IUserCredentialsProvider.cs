using Guestbooky.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.Domain.Abstractions.Infrastructure;

public interface IUserCredentialsProvider
{
    ApplicationUser GetCredentials();

    ValueTask UpdateApplicationUser(ApplicationUser updated);
}
