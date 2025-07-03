using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Domain.Organizations;
using KBMGrpcService.Domain.Organizations.Mapping;
using KBMGrpcService.Domain.Organizations.Validation;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Services.Organizations
{
    public class OrganizationService : OrganizationProtoService.OrganizationProtoServiceBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OrganizationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        /// <param name="db">The database context</param>
        /// <param name="logger">The logger used to record service level events</param>
        public OrganizationService(AppDbContext db, ILogger<OrganizationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Handles the creation of a organization
        /// </summary>
        /// <param name="request">The gRPC request containing the organization that will be created</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="CreateOrganizationResponse"/> containing the result of the organization creation operation</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<CreateOrganizationResponse> CreateOrganization(CreateOrganizationRequest request, ServerCallContext context)
        {
            if (request == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            OrganizationValidator.ValidateCreateOrganizationRequest(request);

            if (await OrganizationRepositoryHelper.NameExistsAsync(_db, request.Name))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Organization name must be unique"));

            var org = OrganizationMapper.CreateOrganizationEntity(request);

            _db.Organizations.Add(org);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to create organization {Name}", request.Name);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create organization"));
            }

            return new CreateOrganizationResponse { Id = org.OrganizationId };
        }

        public override async Task<OrganizationResponse> GetOrganizationById(GetOrganizationByIdRequest request, ServerCallContext context)
        {
            return new OrganizationResponse();
        }

        public override async Task<QueryOrganizationsResponse> QueryOrganizations(QueryOrganizationsRequest request, ServerCallContext context)
        {
            return new QueryOrganizationsResponse();
        }

        public override async Task<OrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request, ServerCallContext context)
        {
            return new OrganizationResponse();
        }

        public override async Task<DeleteOrganizationResponse> DeleteOrganization(DeleteOrganizationRequest request, ServerCallContext context)
        {
            return new DeleteOrganizationResponse();
        }
    }
}
