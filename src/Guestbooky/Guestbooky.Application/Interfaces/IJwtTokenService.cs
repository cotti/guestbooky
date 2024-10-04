using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string username);
}