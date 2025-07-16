using AutoMapper;
using KBMContracts.Dtos;
using KBMGrpcService.Protos;
using Microsoft.AspNetCore.Mvc;

namespace KBMHttpService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserProtoService.UserProtoServiceClient _client;
        private readonly IMapper _mapper;

        public UserController(UserProtoService.UserProtoServiceClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _client.GetUserByIdAsync(new GetByIdRequest { Id = id });
            var userDTO = _mapper.Map<UserDTO>(response);
            return Ok(userDTO);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO createUserDTO)
        {
            var grpcRequest = _mapper.Map<CreateUserRequest>(createUserDTO);
            var response = await _client.CreateUserAsync(grpcRequest);
            return Ok(response);
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] QueryUsersRequestDTO queryUsersRequestDTO)
        {
            var grpcRequest = _mapper.Map<QueryUsersRequest>(queryUsersRequestDTO);
            var response = await _client.QueryUsersAsync(grpcRequest);
            return Ok(response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateUserDTO updateUserDTO)
        {
            var grpcRequest = _mapper.Map<UpdateUserRequest>(updateUserDTO);
            var response = await _client.UpdateUserAsync(grpcRequest);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var grpcRequest = new DeleteUserRequest { Id = id };
            var response = await _client.DeleteUserAsync(grpcRequest);
            return Ok(response);
        }

        [HttpPut("associate")]
        public async Task<IActionResult> Associate([FromBody] AssociateUserDTO associateUserDTO)
        {
            var grpcRequest = _mapper.Map<AssociationRequest>(associateUserDTO);
            var response = await _client.AssociateUserToOrganizationAsync(grpcRequest);
            return Ok(response);
        }

        [HttpPut("disassociate")]
        public async Task<IActionResult> Disassociate([FromBody] DissociateUserDTO dissociateUserDTO)
        {
            var grpcRequest = _mapper.Map<DisassociationRequest>(dissociateUserDTO);
            var response = await _client.DisassociateUserFromOrganizationAsync(grpcRequest);
            return Ok(response);
        }
    }
}
