using Microsoft.AspNetCore.Http;
using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.Business;

public interface IBusinessService
{
    Task<CreateBusinessResponse> CreateAsync(CreateBusinessRequest request, IFormFile? logo, CancellationToken ct);
    Task<List<BusinessPublicDto>> GetPublicAsync(CancellationToken ct);
}
