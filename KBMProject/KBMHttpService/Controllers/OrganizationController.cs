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

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _client.GetOrganizationByIdAsync(new GetOrganizationByIdRequest { Id = id });
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrganizationRequest request)
        {
            var response = await _client.CreateOrganizationAsync(request);
            return Ok(response);
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query(QueryOrganizationsRequest request)
        {
            var response = await _client.QueryOrganizationsAsync(request);
            return Ok(response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateOrganizationRequest request)
        {
            var response = await _client.UpdateOrganizationAsync(request);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _client.DeleteOrganizationAsync(new DeleteOrganizationRequest { Id = id });
            return Ok(response);
        }
    }
}
