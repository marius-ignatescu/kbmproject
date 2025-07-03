using KBMGrpcService.Protos;
using Microsoft.AspNetCore.Mvc;

namespace KBMHttpService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserProtoService.UserProtoServiceClient _client;

        public UserController(UserProtoService.UserProtoServiceClient client)
        {
            _client = client;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _client.GetUserByIdAsync(new GetByIdRequest { Id = id });
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateUserRequest request)
        {
            var response = await _client.CreateUserAsync(request);
            return Ok(response);
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query(QueryUsersRequest request)
        {
            var response = await _client.QueryUsersAsync(request);
            return Ok(response);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update(UpdateUserRequest request)
        {
            var response = await _client.UpdateUserAsync(request);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _client.DeleteUserAsync(new DeleteUserRequest { Id = id });
            return Ok(response);
        }

        [HttpPost("associate")]
        public async Task<IActionResult> Associate(AssociationRequest request)
        {
            var response = await _client.AssociateUserToOrganizationAsync(request);
            return Ok(response);
        }

        [HttpPost("disassociate")]
        public async Task<IActionResult> Disassociate(DisassociationRequest request)
        {
            var response = await _client.DisassociateUserFromOrganizationAsync(request);
            return Ok(response);
        }
    }
}
