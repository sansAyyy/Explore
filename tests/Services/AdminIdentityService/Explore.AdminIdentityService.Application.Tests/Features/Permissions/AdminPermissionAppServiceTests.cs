using BuildingBlocks.Common.Pagination;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Responses;
using Explore.AdminIdentityService.Application.Features.Permissions.Services;
using Explore.AdminIdentityService.Domain.AdminPermissions;
using Explore.AdminIdentityService.Domain.AdminRolePermissions;

namespace Explore.AdminIdentityService.Application.Tests.Features.Permissions;

public sealed class AdminPermissionAppServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldFail_WhenResourceTypeIsUnsupported()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.CreateAsync(new CreateAdminPermissionRequest
        {
            Code = "reports.page",
            Name = "Reports Page",
            ResourceType = (PermissionResourceType)4,
            IsActive = true
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("validation_failed", result.Error.Code);
        Assert.Equal("ResourceType is invalid.", result.Error.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenResourceTypeIsUnsupported()
    {
        var fixture = CreateFixture();
        var permission = fixture.CommandRepository.AddPermission(
            code: "reports.page",
            name: "Reports Page",
            resourceType: PermissionResourceType.Page);

        var result = await fixture.Service.UpdateAsync(permission.Id, new UpdateAdminPermissionRequest
        {
            ParentId = null,
            Code = permission.Code,
            Name = permission.Name,
            Description = permission.Description,
            ResourceType = (PermissionResourceType)4
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("validation_failed", result.Error.Code);
        Assert.Equal("ResourceType is invalid.", result.Error.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateButtonPermission()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.CreateAsync(new CreateAdminPermissionRequest
        {
            ParentId = null,
            Code = "reports.export",
            Name = "Export Reports",
            Description = "Allows exporting reports.",
            ResourceType = PermissionResourceType.Button,
            IsActive = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("reports.export", result.Value!.Code);
        Assert.Equal(PermissionResourceType.Button, result.Value.ResourceType);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public async Task CreateAsync_ShouldAllowNestedPermissionHierarchy()
    {
        var fixture = CreateFixture();
        var groupPermission = fixture.CommandRepository.AddPermission(
            code: "system_management.page",
            name: "System Management",
            resourceType: PermissionResourceType.Page);
        var pagePermission = fixture.CommandRepository.AddPermission(
            code: "admin_users.page",
            name: "Admin Users",
            resourceType: PermissionResourceType.Page,
            parentId: groupPermission.Id);

        var result = await fixture.Service.CreateAsync(new CreateAdminPermissionRequest
        {
            ParentId = pagePermission.Id,
            Code = "admin_users.create",
            Name = "Create Admin User",
            Description = "Allows creating admin users.",
            ResourceType = PermissionResourceType.Button,
            IsActive = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(pagePermission.Id, result.Value!.ParentId);
        Assert.Equal("admin_users.create", result.Value.Code);
    }

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenParentContainsCycleThroughDescendant()
    {
        var fixture = CreateFixture();
        var groupPermission = fixture.CommandRepository.AddPermission(
            code: "system_management.page",
            name: "System Management",
            resourceType: PermissionResourceType.Page);
        var pagePermission = fixture.CommandRepository.AddPermission(
            code: "admin_users.page",
            name: "Admin Users",
            resourceType: PermissionResourceType.Page,
            parentId: groupPermission.Id);

        var result = await fixture.Service.UpdateAsync(groupPermission.Id, new UpdateAdminPermissionRequest
        {
            ParentId = pagePermission.Id,
            Code = groupPermission.Code,
            Name = groupPermission.Name,
            Description = groupPermission.Description,
            ResourceType = groupPermission.ResourceType
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("validation_failed", result.Error.Code);
        Assert.Equal("ParentId contains a cycle.", result.Error.Message);
    }

    private static Fixture CreateFixture()
    {
        var commandRepository = new FakeAdminPermissionCommandRepository();
        var queryRepository = new FakeAdminPermissionQueryRepository(commandRepository);
        var rolePermissionCommandRepository = new FakeAdminRolePermissionCommandRepository();
        var unitOfWork = new FakeAdminIdentityUnitOfWork();
        var service = new AdminPermissionAppService(
            commandRepository,
            queryRepository,
            rolePermissionCommandRepository,
            unitOfWork);

        return new Fixture(service, commandRepository, unitOfWork);
    }

    private sealed record Fixture(
        AdminPermissionAppService Service,
        FakeAdminPermissionCommandRepository CommandRepository,
        FakeAdminIdentityUnitOfWork UnitOfWork);

    private sealed class FakeAdminPermissionCommandRepository : IAdminPermissionCommandRepository
    {
        private readonly List<AdminPermission> _permissions = [];

        public AdminPermission AddPermission(
            string code,
            string name,
            PermissionResourceType resourceType,
            Guid? parentId = null,
            string? description = null,
            bool isActive = true)
        {
            var permission = AdminPermission.Create(
                Guid.NewGuid(),
                parentId,
                code,
                name,
                description,
                resourceType,
                isActive);

            _permissions.Add(permission);
            return permission;
        }

        public Task<AdminPermission?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_permissions.SingleOrDefault(x => x.Id == id));
        }

        public Task<IReadOnlyCollection<AdminPermission>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<AdminPermission> permissions = _permissions
                .Where(x => ids.Contains(x.Id))
                .ToArray();

            return Task.FromResult(permissions);
        }

        public Task<bool> ExistsByParentIdAsync(Guid parentId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_permissions.Any(x => x.ParentId == parentId));
        }

        public Task AddAsync(AdminPermission adminPermission, CancellationToken cancellationToken)
        {
            _permissions.Add(adminPermission);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_permissions.Any(x =>
                x.Code == code &&
                (!excludedId.HasValue || x.Id != excludedId.Value)));
        }

        public void Remove(AdminPermission adminPermission)
        {
            _permissions.Remove(adminPermission);
        }

    }

    private sealed class FakeAdminPermissionQueryRepository : IAdminPermissionQueryRepository
    {
        private readonly FakeAdminPermissionCommandRepository _commandRepository;

        public FakeAdminPermissionQueryRepository(FakeAdminPermissionCommandRepository commandRepository)
        {
            _commandRepository = commandRepository;
        }

        public Task<IReadOnlyCollection<AdminPermissionBasicResponse>> GetRootsAsync(bool? isActive, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<AdminPermissionTreeNodeResponse?> GetDescendantsAsync(Guid rootId, bool? isActive, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<AdminPermissionDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var permission = _commandRepository
                .GetByIdAsync(id, cancellationToken)
                .GetAwaiter()
                .GetResult();

            return Task.FromResult(permission is null
                ? null
                : new AdminPermissionDetailResponse(
                    permission.Id,
                    permission.ParentId,
                    permission.Code,
                    permission.Name,
                    permission.Description,
                    permission.ResourceType,
                    permission.IsActive,
                    permission.CreatedAt,
                    permission.CreatedBy,
                    permission.UpdatedAt,
                    permission.UpdatedBy,
                    permission.Version));
        }

        public Task<PagedResult<AdminPermissionBasicResponse>> GetPagedAsync(GetPagedAdminPermissionsRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeAdminRolePermissionCommandRepository : IAdminRolePermissionCommandRepository
    {
        public Task<IReadOnlyCollection<AdminRolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<AdminRolePermission> permissions = [];
            return Task.FromResult(permissions);
        }

        public Task AddRangeAsync(IReadOnlyCollection<AdminRolePermission> adminRolePermissions, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void RemoveRange(IReadOnlyCollection<AdminRolePermission> adminRolePermissions)
        {
        }

        public Task<bool> ExistsByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

    }

    private sealed class FakeAdminIdentityUnitOfWork : IAdminIdentityUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            CommitCount++;
            return Task.FromResult(1);
        }
    }
}

