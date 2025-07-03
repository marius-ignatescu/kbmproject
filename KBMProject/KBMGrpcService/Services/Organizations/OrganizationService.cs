using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Domain.Organizations;
using KBMGrpcService.Domain.Organizations.Mapping;
using KBMGrpcService.Domain.Organizations.Validation;
using KBMGrpcService.Domain.Users.Mapping;
using KBMGrpcService.Domain.Users;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;
using KBMGrpcService.Domain.Users.Queries;
using KBMGrpcService.Domain.Organizations.Queries;

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

        /// <summary>
        /// Returns a organization by id
        /// </summary>
        /// <param name="request">The gRPC request containing the required organization id</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="OrganizationResponse"/> containing the the organization requested</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<OrganizationResponse> GetOrganizationById(GetOrganizationByIdRequest request, ServerCallContext context)
        {
            if (request.Id == 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid organization ID."));
            }

            var organization = await OrganizationRepositoryHelper.GetActiveOrganizationByIdWithNoTrackingAsync(_db, request.Id);

            if (organization == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));
            }

            return OrganizationMapper.MapToOrganizationResponse(organization);
        }

        /// <summary>
        /// Returns a list of organizations by the specified search criteria
        /// </summary>
        /// <param name="request">The gRPC request containing search parameters</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="QueryOrganizationsResponse"/> containing the the organizations list</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<QueryOrganizationsResponse> QueryOrganizations(QueryOrganizationsRequest request, ServerCallContext context)
        {
            OrganizationValidator.ValidatePagination(request);

            var query = OrganizationRepositoryHelper.GetActiveOrganizations(_db);

            query = OrganizationQueryBuilder.ApplyFiltering(query, request.QueryString);
            query = OrganizationQueryBuilder.ApplyOrdering(query, request.OrderBy, request.Direction, _logger);

            var total = await query.CountAsync();

            var organizations = await OrganizationQueryBuilder.ApplyPaging(query, request.Page, request.PageSize).ToListAsync();

            var response = new QueryOrganizationsResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total
            };
            response.Organizations.AddRange(organizations.Select(OrganizationMapper.MapToOrganizationResponse));

            return response;
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
