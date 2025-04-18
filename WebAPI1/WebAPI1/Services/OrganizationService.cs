using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Services;

public class OrganizationTreeNode
{
    public OrganizationDto Data { get; set; }
    public List<OrganizationTreeNode> Children { get; set; } = new();
}

// 組織DTO
public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public int TypeId { get; set; }
    public string? TypeName { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Address { get; set; }
    public string ContactPerson { get; set; }
    public string ContactPhone { get; set; }
    public string TaxId { get; set; }
    public string DomainName { get; set; }
    public string? DomainVerificationToken { get; set; }
    public DateTime? DomainVerifiedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public interface IOrganizationService
{
    Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAsync();
    Task<OrganizationDto> GetOrganizationByIdAsync(int organizationId);
    Task<IEnumerable<OrganizationDto>> GetChildOrganizationsAsync(int parentId);
    Task<List<OrganizationDto>> GetAllDescendantOrganizationsAsync(int rootParentId);
    Task<OrganizationTreeNode> GetOrganizationTreeByDomainAsync(string domain);
    Task<List<OrganizationTreeNode>> GetFilteredOrgTreeAsync();
    
    List<int> GetDescendantOrganizationIds(int parentId);
}
public class OrganizationService:IOrganizationService
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<OrganizationService> _logger;
    public OrganizationService(
        isha_sys_devContext db,
        ILogger<OrganizationService> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAsync()
    {
        try
        {
            var organizations = await _db.Organizations
                .Include(o => o.OrganizationType)
                .Include(o => o.ParentOrganization)
                .Include(o => o.Domains.Where(d => d.IsPrimary))
                .ToListAsync();

            return organizations.Select(o => new OrganizationDto
            {
                Id = o.Id,
                Name = o.Name,
                Code = o.Code,
                TypeId = o.TypeId,
                TypeName = o.OrganizationType?.TypeName,
                ParentId = o.ParentId,
                ParentName = o.ParentOrganization?.Name,
                Address = o.Address,
                ContactPerson = o.ContactPerson,
                ContactPhone = o.ContactPhone,
                TaxId = o.TaxId,
                // 從Domains導航屬性獲取主要域名資訊
                DomainName = o.Domains.FirstOrDefault(d => d.IsPrimary)?.DomainName,
                DomainVerificationToken = o.Domains.FirstOrDefault(d => d.IsPrimary)?.VerificationToken,
                DomainVerifiedAt = o.Domains.FirstOrDefault(d => d.IsPrimary)?.VerifiedAt,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all organizations");
            throw;
        }
    }

    public async Task<OrganizationDto> GetOrganizationByIdAsync(int organizationId)
    {
        try
        {
            var organization = await _db.Organizations
                .Include(o => o.OrganizationType)
                .Include(o => o.ParentOrganization)
                .Include(o => o.Domains.Where(d => d.IsPrimary))
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                return null;
            }

            // 獲取主要域名資訊
            var primaryDomain = organization.Domains.FirstOrDefault(d => d.IsPrimary);

            return new OrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Code = organization.Code,
                TypeId = organization.TypeId,
                TypeName = organization.OrganizationType?.TypeName,
                ParentId = organization.ParentId,
                ParentName = organization.ParentOrganization?.Name,
                Address = organization.Address,
                ContactPerson = organization.ContactPerson,
                ContactPhone = organization.ContactPhone,
                TaxId = organization.TaxId,
                // 從 primaryDomain 獲取域名相關資訊
                DomainName = primaryDomain?.DomainName,
                DomainVerificationToken = primaryDomain?.VerificationToken,
                DomainVerifiedAt = primaryDomain?.VerifiedAt,
                IsActive = organization.IsActive,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while getting organization with ID {organizationId}");
            throw;
        }
    }

    public async Task<IEnumerable<OrganizationDto>> GetChildOrganizationsAsync(int parentId)
    {
        try
        {
            var organizations = await _db.Organizations
                .Include(o => o.OrganizationType)
                .Include(o => o.Domains.Where(d => d.IsPrimary))
                .Where(o => o.ParentId == parentId)
                .ToListAsync();

            return organizations.Select(o =>
            {
                // 獲取主要域名資訊
                var primaryDomain = o.Domains.FirstOrDefault(d => d.IsPrimary);

                return new OrganizationDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Code = o.Code,
                    TypeId = o.TypeId,
                    TypeName = o.OrganizationType?.TypeName,
                    ParentId = o.ParentId,
                    ParentName = null, // 父組織名稱已知，不需要再次包含
                    Address = o.Address,
                    ContactPerson = o.ContactPerson,
                    ContactPhone = o.ContactPhone,
                    TaxId = o.TaxId,
                    // 從主要域名獲取域名相關資訊
                    DomainName = primaryDomain?.DomainName,
                    DomainVerificationToken = primaryDomain?.VerificationToken,
                    DomainVerifiedAt = primaryDomain?.VerifiedAt,
                    IsActive = o.IsActive,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while getting child organizations for parent ID {parentId}");
            throw;
        }
    }
    
    public async Task<List<OrganizationDto>> GetAllDescendantOrganizationsAsync(int rootParentId)
    {
        try
        {
            var allOrgs = await _db.Organizations
                .Include(o => o.OrganizationType)
                .Include(o => o.Domains.Where(d => d.IsPrimary))
                .ToListAsync();

            var orgDict = allOrgs.ToDictionary(o => o.Id);
            var result = new List<OrganizationDto>();

            void TraverseChildren(int parentId)
            {
                // 🔍 找出所有子節點（不止一個）
                var children = allOrgs.Where(o => o.ParentId == parentId).ToList();

                foreach (var child in children)
                {
                    var primaryDomain = child.Domains.FirstOrDefault(d => d.IsPrimary);

                    result.Add(new OrganizationDto
                    {
                        Id = child.Id,
                        Name = child.Name,
                        Code = child.Code,
                        TypeId = child.TypeId,
                        TypeName = child.OrganizationType?.TypeName,
                        ParentId = child.ParentId,
                        ParentName = orgDict.ContainsKey(child.ParentId ?? 0) ? orgDict[child.ParentId.Value]?.Name : null,
                        Address = child.Address,
                        ContactPerson = child.ContactPerson,
                        ContactPhone = child.ContactPhone,
                        TaxId = child.TaxId,
                        DomainName = primaryDomain?.DomainName,
                        DomainVerificationToken = primaryDomain?.VerificationToken,
                        DomainVerifiedAt = primaryDomain?.VerifiedAt,
                        IsActive = child.IsActive,
                        CreatedAt = child.CreatedAt,
                        UpdatedAt = child.UpdatedAt
                    });

                    // 🔁 遞迴所有子節點
                    TraverseChildren(child.Id);
                }
            }

            TraverseChildren(rootParentId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while getting descendant organizations for parent ID {rootParentId}");
            throw;
        }
    }
    
    
    //用domain找出組織id，如有兩個以上找最上層
    public async Task<OrganizationTreeNode> GetOrganizationTreeByDomainAsync(string domain)
    {
        // Step 1: 取得所有符合該 domain 的組織
        var organizations = await _db.OrganizationDomains
            .Include(d => d.Organization)
                .ThenInclude(o => o.OrganizationType)
            .Where(d => d.DomainName.ToLower() == domain.ToLower() && d.IsPrimary)
            .Select(d => d.Organization)
            .ToListAsync();

        if (organizations.Count == 0) return null;

        // Step 2: 載入 type 階層資料（從你那張表）
        var hierarchies = await _db.OrganizationHierarchies
            .Select(h => new { h.ParentTypeId, h.ChildTypeId })
            .ToListAsync();

        // Step 3: 建立 child → parent 映射表，方便反查
        var parentMap = new Dictionary<int, int>(); // ChildTypeId -> ParentTypeId
        foreach (var h in hierarchies)
        {
            parentMap[h.ChildTypeId] = h.ParentTypeId;
        }

        // Step 4: 過濾出「最上層」的組織（不被其他人當作 child）
        var topOrg = organizations.FirstOrDefault(org =>
            organizations.All(other =>
                org == other || !IsChildOf(org.TypeId, other.TypeId, parentMap)
            )
        );

        if (topOrg == null) return null;

        // Step 5: 建立樹狀資料
        return await BuildOrganizationTreeAsync(topOrg.Id);
    }


    // 工具方法：判斷 childTypeId 是否是 parentTypeId 的下層（可遞延）
    private bool IsChildOf(int childTypeId, int potentialParentTypeId, Dictionary<int, int> parentMap)
    {
        int current = childTypeId;
        while (parentMap.TryGetValue(current, out int parent))
        {
            if (parent == potentialParentTypeId)
                return true;
            current = parent;
        }
        return false;
    }


    // Step 6: 使用原本的 BuildNode 方法建立樹狀結構
    public async Task<OrganizationTreeNode> BuildOrganizationTreeAsync(int rootId)
    {
        var allOrgs = await _db.Organizations
            .Include(o => o.OrganizationType)
            .Include(o => o.Domains.Where(d => d.IsPrimary))
            .ToListAsync();

        var root = allOrgs.FirstOrDefault(o => o.Id == rootId);
        if (root == null) return null;

        OrganizationTreeNode BuildNode(Organization org)
        {
            var dto = new OrganizationDto
            {
                Id = org.Id,
                Name = org.Name,
                TypeId = org.TypeId,
                TypeName = org.OrganizationType?.TypeName,
            };

            var node = new OrganizationTreeNode { Data = dto };
            var children = allOrgs.Where(o => o.ParentId == org.Id).ToList();

            foreach (var child in children)
            {
                node.Children.Add(BuildNode(child));
            }

            return node;
        }

        return BuildNode(root);
    }
    
    
    // 針對 TypeId = 2, 3, 4 建立樹狀
    public async Task<List<OrganizationTreeNode>> GetFilteredOrgTreeAsync()
    {
        // Step 1: 撈出符合類型的所有組織（含必要的導航屬性）
        var filteredOrgs = await _db.Organizations
            .Include(o => o.OrganizationType)
            .Where(o => o.TypeId == 2 || o.TypeId == 3 || o.TypeId == 4)
            .ToListAsync();

        // Step 2: 建立樹狀節點（從 root 節點開始建）
        var roots = filteredOrgs
            .Where(o => o.ParentId == null || !filteredOrgs.Any(p => p.Id == o.ParentId))
            .ToList();

        List<OrganizationTreeNode> tree = new();
        foreach (var root in roots)
        {
            tree.Add(BuildNode(root, filteredOrgs));
        }

        return tree;
    }

    private OrganizationTreeNode BuildNode(Organization org, List<Organization> scope)
    {
        var dto = new OrganizationDto
        {
            Id = org.Id,
            Name = org.Name,
            TypeId = org.TypeId,
            TypeName = org.OrganizationType?.TypeName,
        };

        var node = new OrganizationTreeNode { Data = dto };

        var children = scope.Where(o => o.ParentId == org.Id).ToList();
        foreach (var child in children)
        {
            node.Children.Add(BuildNode(child, scope));
        }

        return node;
    }
    
    
    //工具:找到自己與底下階層組織的 ID
    public List<int> GetDescendantOrganizationIds(int parentId)
    {
        var result = new List<int> { parentId };

        var children = _db.Organizations
            .Where(o => o.ParentId == parentId)
            .Select(o => o.Id)
            .ToList();

        foreach (var childId in children)
        {
            result.AddRange(GetDescendantOrganizationIds(childId));
        }

        return result;
    }
    
}