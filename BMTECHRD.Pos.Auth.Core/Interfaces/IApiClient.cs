using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMTECHRD.Pos.Auth.Core.Interfaces;

public interface IApiClient
{
    Task<List<object>> GetTablesAsync(Guid businessId);
}
