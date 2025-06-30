using KBMGrpcService.Protos;
using KBMGrpcService.Services;
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
    }
}
