using Guestbooky.Domain.Entities.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Guestbooky.Domain.Abstractions.Repositories;

public interface IGuestbookMessageRepository
{
    Task<long> CountAsync(CancellationToken cancellationToken);
    Task<IEnumerable<GuestbookMessage>> GetAsync(long offset, CancellationToken cancellationToken);
    Task AddAsync(GuestbookMessage message, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
