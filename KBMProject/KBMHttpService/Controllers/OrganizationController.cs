using AutoMapper;
using KBMContracts.Dtos;
using KBMGrpcService.Protos;
using Microsoft.AspNetCore.Mvc;

namespace KBMHttpService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly OrganizationProtoService.OrganizationProtoServiceClient _client;
        private readonly IMapper _mapper;

        public OrganizationController(OrganizationProtoService.OrganizationProtoServiceClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _client.GetOrganizationByIdAsync(new GetOrganizationByIdRequest { Id = id });
            var organizationDTO = _mapper.Map<OrganizationDTO>(response);
            return Ok(organizationDTO);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationDTO createOrganizationDTO)
        {
            var createOrganizationRequest = _mapper.Map<CreateOrganizationRequest>(createOrganizationDTO);
            var response = await _client.CreateOrganizationAsync(createOrganizationRequest);
            return Ok(response);
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] QueryOrganizationsRequestDTO queryOrganizationsDTO)
        {
            var grpcRequest = _mapper.Map<QueryOrganizationsRequest>(queryOrganizationsDTO);
            var response = await _client.QueryOrganizationsAsync(grpcRequest);
            return Ok(response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateOrganizationDTO updateOrganizationDTO)
        {
            var grpcRequest = _mapper.Map<UpdateOrganizationRequest>(updateOrganizationDTO);
            var response = await _client.UpdateOrganizationAsync(grpcRequest);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var grpcRequest = new DeleteOrganizationRequest { Id = id };
            var response = await _client.DeleteOrganizationAsync(grpcRequest);
            return Ok(response);
        }
    }
}
