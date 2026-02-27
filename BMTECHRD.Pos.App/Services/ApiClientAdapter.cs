using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BMTECHRD.Pos.Auth.Core.Interfaces;

namespace BMTECHRD.Pos.App.Services;

public class ApiClientAdapter : IApiClient
{
    private readonly ApiClient _inner;

    public ApiClientAdapter(ApiClient inner)
    {
        _inner = inner;
    }

    public async Task<List<object>> GetTablesAsync(Guid businessId)
    {
        var list = await _inner.GetTablesAsync(businessId);
        var result = new List<object>();
        foreach (var t in list)
            result.Add(t);
        return result;
    }
}
