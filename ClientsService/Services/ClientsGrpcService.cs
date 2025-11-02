using AutoMapper;
using Clients;
using ClientsService.Data;
using ClientsService.Models;
using FluentValidation;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using static Clients.Clients;

namespace ClientsService.Services;

public class ClientsGrpcService : ClientsBase
{
    private readonly ClientsDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateClientRequest> _createValidator;
    private readonly IValidator<UpdateClientRequest> _updateValidator;

    public ClientsGrpcService(
        ClientsDbContext context, 
        IMapper mapper, 
        IValidator<CreateClientRequest> createValidator,
        IValidator<UpdateClientRequest> updateValidator)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // 1. Create Client
    public override async Task<ClientResponse> CreateClient(CreateClientRequest request, ServerCallContext context)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var client = _mapper.Map<Client>(request);
        client.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        client.Id = Guid.NewGuid();
        client.Status = "active";
        client.RegistrationDate = DateTime.UtcNow;

        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        return _mapper.Map<ClientResponse>(client);
    }

    // 2. Get by ID
    public override async Task<ClientResponse> GetClientById(GetClientByIdRequest request, ServerCallContext context)
    {
        var client = await _context.Clients.FindAsync(Guid.Parse(request.Id));
        if (client == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Client not found"));
        }
        return _mapper.Map<ClientResponse>(client);
    }

    // 3. Get All (with Filters)
    public override async Task<GetAllClientsResponse> GetAllClients(GetAllClientsRequest request, ServerCallContext context)
    {
        IQueryable<Client> query = _context.Clients;

        if (!string.IsNullOrEmpty(request.FilterStatus))
        {
            if (request.FilterStatus.Equals("inactive", StringComparison.OrdinalIgnoreCase))
            {
                query = _context.Clients.IgnoreQueryFilters()
                            .Where(c => c.Status == "inactive");
            }
        }

        if (!string.IsNullOrEmpty(request.FilterName))
        {
            query = query.Where(c => (c.FirstName + " " + c.LastName).Contains(request.FilterName));
        }

        if (!string.IsNullOrEmpty(request.FilterEmail))
        {
            query = query.Where(c => c.Email == request.FilterEmail);
        }

        if (!string.IsNullOrEmpty(request.FilterUsername))
        {
            query = query.Where(c => c.Username.Contains(request.FilterUsername));
        }

        var clients = await query.ToListAsync();
        var response = new GetAllClientsResponse();
        response.Clients.AddRange(_mapper.Map<IEnumerable<ClientResponse>>(clients));
        return response;
    }

    // 4. Delete (Soft Delete)
    public override async Task<DeleteClientResponse> DeleteClient(DeleteClientRequest request, ServerCallContext context)
    {
        var client = await _context.Clients.FindAsync(Guid.Parse(request.Id));
        if (client == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Active client not found"));
        }

        client.Status = "inactive";
        await _context.SaveChangesAsync();

        return new DeleteClientResponse { StatusMessage = "Client deactivated successfully" };
    }

    // 5. Update Client
    public override async Task<ClientResponse> UpdateClient(UpdateClientRequest request, ServerCallContext context)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var client = await _context.Clients.FindAsync(Guid.Parse(request.Id));
        if (client == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Client not found"));
        }

        _mapper.Map(request, client);

        if (!string.IsNullOrEmpty(request.Password))
        {
            client.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        _context.Clients.Update(client);
        await _context.SaveChangesAsync();

        return _mapper.Map<ClientResponse>(client);
    }

    // 6. Method for Auth Service
    public override async Task<ClientAuthResponse> GetClientForAuth(GetClientForAuthRequest request, ServerCallContext context)
    {
        var client = await _context.Clients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Email == request.EmailOrUsername || c.Username == request.EmailOrUsername);

        if (client == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Invalid credentials"));
        }

        return new ClientAuthResponse
        {
            Id = client.Id.ToString(),
            PasswordHash = client.PasswordHash,
            Status = client.Status,
            Role = client.Role
        };
    }
}