using KBMGrpcService.Protos;
using Microsoft.AspNetCore.Mvc;

namespace KBMHttpService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly OrganizationProtoService.OrganizationProtoServiceClient _client;

        public OrganizationController(OrganizationProtoService.OrganizationProtoServiceClient client)
        {
            _client = client;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrganizationRequest request)
        {
            var response = await _client.CreateOrganizationAsync(request);
            return Ok(response);
        }
    }
}
