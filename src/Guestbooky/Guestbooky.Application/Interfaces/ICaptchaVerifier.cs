using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.Application.Interfaces
{
    public interface ICaptchaVerifier
    {
        public Task<bool> VerifyAsync(string challengeResponse, CancellationToken cancellationToken);
    }
}
