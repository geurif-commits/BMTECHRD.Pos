using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BMTECHRD.Pos.Auth.Core.Interfaces;

public class TestApiClient : IApiClient
{
    private readonly HttpClient _http;

    public TestApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<object>> GetTablesAsync(Guid businessId)
    {
        var url = $"api/tables?businessId={businessId}";
        try
        {
            var list = await _http.GetFromJsonAsync<List<object>>(url);
            return list ?? new List<object>();
        }
        catch
        {
            return new List<object>();
        }
    }
}
