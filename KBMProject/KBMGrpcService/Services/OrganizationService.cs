using AutoMapper;
using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Data.QueryBuilders;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Services
{
    public class OrganizationService : OrganizationProtoService.OrganizationProtoServiceBase
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrganizationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        /// <param name="organizationRepository">The repository</param>
        /// <param name="mapper">Automapper</param>
        /// <param name="logger">The logger used to record service level events</param>
        public OrganizationService(IOrganizationRepository organizationRepository, IMapper mapper, ILogger<OrganizationService> logger)
        {
            _organizationRepository = organizationRepository;
            _mapper = mapper;
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
            // Validation
            if (request == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Name is required"));

            if (await _organizationRepository.NameExistsAsync(request.Name))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Organization name must be unique"));

            // Create and save the organization
            var org = _mapper.Map<Organization>(request);
            try
            {
                await _organizationRepository.AddAsync(org);
                _organizationRepository.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to create organization {Name}", request.Name);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create organization"));
            }

            return _mapper.Map<CreateOrganizationResponse>(org);
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
            _logger.LogInformation("Fetching organization with ID: {OrgId}", request.Id);

            if (request.Id == 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid organization ID."));
            }

            var organization = await _organizationRepository.GetByIdAsync(request.Id);

            if (organization == null)
            {
                _logger.LogWarning("Organization with ID {OrgId} not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));
            }

            return _mapper.Map<OrganizationResponse>(organization);
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
            // Validations
            if (request.Page < 1 || request.PageSize < 1)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0."));
            }

            var query = await _organizationRepository.GetAllAsync();

            query = OrganizationQueryBuilder.ApplyFiltering(query.AsQueryable(), request.QueryString);
            query = OrganizationQueryBuilder.ApplyOrdering(query.AsQueryable(), request.OrderBy, request.Direction);

            var total = query.Count();

            var organizations = OrganizationQueryBuilder.ApplyPaging(query.AsQueryable(), request.Page, request.PageSize).ToList();

            // Build response
            var response = new QueryOrganizationsResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total
            };

            response.Organizations.AddRange(_mapper.Map<IEnumerable<OrganizationResponse>>(organizations));

            return response;
        }

        /// <summary>
        /// Update organization
        /// </summary>
        /// <param name="request">The gRPC request containing the organization that will be updated</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="OrganizationResponse"/> containing the result with the organization updated operation</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<OrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request, ServerCallContext context)
        {
            // Validations
            if (request == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            var organization = await _organizationRepository.GetByIdAsync(request.Id);
            if (organization == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Organization name is required"));

            var organizationExists = await _organizationRepository.NameExistsAsync(request.Name, organization.OrganizationId);

            if (organizationExists)
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Update failed. Another organization with this name already exists"));

            // Update
            try
            {
                // Map on EF entity
                _mapper.Map(request, organization);

                _organizationRepository.Update(organization);
                _organizationRepository.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update organization {OrganizationId}", organization.OrganizationId);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to update the organization due to a database error."));
            }

            // Return the response
            return _mapper.Map<OrganizationResponse>(organization);
        }

        /// <summary>
        /// Delete organization
        /// </summary>
        /// <param name="request">The gRPC request with the Organization ID that will be deleted</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="DeleteResponse"/> containing the result indicating if the operation was with success or not</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<DeleteOrganizationResponse> DeleteOrganization(DeleteOrganizationRequest request, ServerCallContext context)
        {
            // Validations
            if (request.Id == 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid organization ID."));
            }

            var organization = await _organizationRepository.GetByIdAsync(request.Id);
            if (organization == null)
            {
                return new DeleteOrganizationResponse { Success = false };
            }

            // Delete
            bool success = false;
            try
            {
                _organizationRepository.Delete(organization);
                success = _organizationRepository.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete organization {OrganizationId}", organization.OrganizationId);
            }

            return new DeleteOrganizationResponse { Success = success };
        }
    }
}
