using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI1.Entities;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Authorize]
[Route("Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    // ======= 角色管理 =======
    
    
    [HttpGet("roles")]
    public async Task<ActionResult<string[]>> GetRoles(CancellationToken ct)
        => Ok(await _adminService.GetRolesAsync(ct));

    public record CreateRoleReq(string Name);
    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleReq req, CancellationToken ct)
    {
        await _adminService.CreateRoleAsync(req?.Name ?? "", ct);
        return NoContent();
    }

    public record RenameRoleReq(string NewName);
    [HttpPatch("roles/{name}")]
    public async Task<IActionResult> RenameRole([FromRoute] string name, [FromBody] RenameRoleReq req, CancellationToken ct)
    {
        await _adminService.RenameRoleAsync(name, req?.NewName ?? "", ct);
        return NoContent();
    }

    [HttpDelete("roles/{name}")]
    public async Task<IActionResult> DeleteRole([FromRoute] string name, CancellationToken ct)
    {
        await _adminService.DeleteRoleAsync(name, ct);
        return NoContent();
    }
    
    // ======= 既有矩陣/權限 =======
    /// <summary>
    /// 取得權限矩陣（角色清單 + 每個 permission 的 grants）
    /// </summary>
    [HttpGet("matrix")]
    public async Task<ActionResult<PermissionMatrixDto>> GetMatrix(CancellationToken ct)
    {
        var result = await _adminService.GetMatrixAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// 新增或更新單一 Permission（僅 key/label）
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertPermissionDto dto, CancellationToken ct)
    {
        await _adminService.UpsertPermissionAsync(dto, ct);
        return NoContent();
    }

    /// <summary>
    /// 刪除單一 Permission（會連動刪除對應 RolePermission）
    /// </summary>
    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key, CancellationToken ct)
    {
        await _adminService.DeletePermissionAsync(key, ct);
        return NoContent();
    }

    /// <summary>
    /// 儲存整個矩陣（差異化更新 RolePermission，並 upsert Permission 描述）
    /// </summary>
    [HttpPost("matrix")]
    public async Task<IActionResult> SaveMatrix([FromBody] SavePermissionMatrixRequest req, CancellationToken ct)
    {
        await _adminService.SaveMatrixAsync(req, ct);
        return NoContent();
    }
    
    
    // ======= 使用者管理 =======
    // 取得使用者詳情
    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> GetUserDetail(Guid id, CancellationToken ct)
    {
        var detail = await _adminService.GetUserDetailAsync(id, ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    // 查詢使用者清單
    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<UserListItemDto>>> SearchUsers([FromQuery] UserListQueryDto q, CancellationToken ct)
        => Ok(await _adminService.SearchUsersAsync(q, ct));

    // 修改使用者
    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto2 dto, CancellationToken ct)
    {
        await _adminService.UpdateUserAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken ct)
    {
        await _adminService.DeleteUserAsync(id, ct);
        return NoContent();
    }
    
    // 啟用/停用
    [HttpPatch("users/{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetActiveDto dto, CancellationToken ct)
    {
        await _adminService.SetActiveAsync(id, dto.IsActive, ct);
        return NoContent();
    }

    // Email 驗證狀態
    public record SetEmailVerifiedDto(bool EmailVerified);
    [HttpPatch("users/{id:guid}/email-verified")]
    public async Task<IActionResult> SetEmailVerified(Guid id, [FromBody] SetEmailVerifiedDto dto, CancellationToken ct)
    {
        await _adminService.SetEmailVerifiedAsync(id, dto.EmailVerified, ct);
        return NoContent();
    }

    // 修改角色
    [HttpPut("users/{id:guid}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] SetRolesDto dto, CancellationToken ct)
    {
        await _adminService.SetRolesAsync(id, dto.Roles, ct);
        return NoContent();
    }
    
    // ======= 日誌管理 =======
    [HttpGet("data-change-logs")]
    public async Task<ActionResult<IEnumerable<DataChangeLog>>> GetLogs([FromQuery] string? q, CancellationToken ct)
    {
        var logs = await _adminService.GetLogsAsync(q, ct);
        return Ok(logs);
    }
    
    // ======= 組織管理 =======
    [HttpGet("org/tree")]
    public async Task<ActionResult<List<OrgTreeNodeDto>>> GetOrgTree(CancellationToken ct)
        => Ok(await _adminService.GetOrgTreeAsync(ct));

    [HttpGet("org/{id:int}")]
    public async Task<ActionResult<Organization>> GetOrgById([FromRoute] int id, CancellationToken ct)
    {
        var org = await _adminService.GetOrgAsync(id, ct);
        return org is null ? NotFound() : Ok(org);
    }

    [HttpPost("org")]
    public async Task<IActionResult> CreateOrg([FromBody] OrgUpsertDto dto, CancellationToken ct)
    {
        var id = await _adminService.CreateOrgAsync(dto, ct);
        return CreatedAtAction(nameof(GetOrgById), new { id }, new { id });
    }

    [HttpPut("org/{id:int}")]
    public async Task<IActionResult> UpdateOrg([FromRoute] int id, [FromBody] OrgUpsertDto dto, CancellationToken ct)
    {
        await _adminService.UpdateOrgAsync(id, dto, ct);
        return NoContent();
    }

    [HttpPatch("org/{id:int}/move")]
    public async Task<IActionResult> MoveOrg([FromRoute] int id, [FromBody] MoveParentDto dto, CancellationToken ct)
    {
        await _adminService.MoveOrgAsync(id, dto.NewParentId, ct);
        return NoContent();
    }

    [HttpDelete("org/{id:int}")]
    public async Task<IActionResult> DeleteOrg([FromRoute] int id, CancellationToken ct)
    {
        await _adminService.DeleteOrgAsync(id, ct);
        return NoContent();
    }
    
    // ======= 組織「類型」 APIs =======
    public record OrgTypeDto(int Id, string TypeCode, string TypeName, string? Description, bool CanHaveChildren);
    public record OrgTypeUpsertDto(string TypeCode, string TypeName, string? Description, bool CanHaveChildren);
    [HttpGet("org/types")]
    public async Task<ActionResult<IEnumerable<OrgTypeDto>>> GetOrgTypes(CancellationToken ct)
        => Ok(await _adminService.GetOrgTypesAsync(ct));

    [HttpPost("org/types")]
    public async Task<IActionResult> CreateOrgType([FromBody] OrgTypeUpsertDto dto, CancellationToken ct)
    {
        var id = await _adminService.CreateOrgTypeAsync(dto, ct);
        return CreatedAtAction(nameof(GetOrgTypes), new { id }, new { id });
    }

    [HttpPut("org/types/{id:int}")]
    public async Task<IActionResult> UpdateOrgType([FromRoute] int id, [FromBody] OrgTypeUpsertDto dto, CancellationToken ct)
    {
        await _adminService.UpdateOrgTypeAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("org/types/{id:int}")]
    public async Task<IActionResult> DeleteOrgType([FromRoute] int id, CancellationToken ct)
    {
        await _adminService.DeleteOrgTypeAsync(id, ct);
        return NoContent();
    }


    // ======= 組織「階層規則」 APIs =======
    public record HierRuleDto(int Id, int ParentTypeId, int ChildTypeId, bool IsRequired, int? MaxChildren);
    public record HierRuleUpsertDto(int ParentTypeId, int ChildTypeId, bool IsRequired, int? MaxChildren);

    [HttpGet("org/hierarchy")]
    public async Task<ActionResult<IEnumerable<HierRuleDto>>> GetHierarchy(CancellationToken ct)
        => Ok(await _adminService.GetHierarchyRulesAsync(ct));

    [HttpPost("org/hierarchy")]
    public async Task<IActionResult> CreateHierarchy([FromBody] HierRuleUpsertDto dto, CancellationToken ct)
    {
        var id = await _adminService.CreateHierarchyRuleAsync(dto, ct);
        return CreatedAtAction(nameof(GetHierarchy), new { id }, new { id });
    }

    [HttpPut("org/hierarchy/{id:int}")]
    public async Task<IActionResult> UpdateHierarchy([FromRoute] int id, [FromBody] HierRuleUpsertDto dto, CancellationToken ct)
    {
        await _adminService.UpdateHierarchyRuleAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("org/hierarchy/{id:int}")]
    public async Task<IActionResult> DeleteHierarchy([FromRoute] int id, CancellationToken ct)
    {
        await _adminService.DeleteHierarchyRuleAsync(id, ct);
        return NoContent();
    }

    // ======= 組織「網域」 APIs =======
    [HttpGet("org/domains")]
    public async Task<ActionResult<List<OrgDomainDto>>> GetOrgDomains([FromQuery] int? orgId, CancellationToken ct)
        => Ok(await _adminService.GetOrgDomainsAsync(orgId, ct));

    [HttpGet("org/domains/{id:int}")]
    public async Task<ActionResult<OrgDomainDto>> GetOrgDomain([FromRoute] int id, CancellationToken ct)
    {
        var domain = await _adminService.GetOrgDomainAsync(id, ct);
        return domain is null ? NotFound() : Ok(domain);
    }

    [HttpPost("org/domains")]
    public async Task<IActionResult> CreateOrgDomain([FromBody] OrgDomainUpsertDto dto, CancellationToken ct)
    {
        var id = await _adminService.CreateOrgDomainAsync(dto, ct);
        return CreatedAtAction(nameof(GetOrgDomain), new { id }, new { id });
    }

    [HttpPut("org/domains/{id:int}")]
    public async Task<IActionResult> UpdateOrgDomain([FromRoute] int id, [FromBody] OrgDomainUpsertDto dto, CancellationToken ct)
    {
        await _adminService.UpdateOrgDomainAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("org/domains/{id:int}")]
    public async Task<IActionResult> DeleteOrgDomain([FromRoute] int id, CancellationToken ct)
    {
        await _adminService.DeleteOrgDomainAsync(id, ct);
        return NoContent();
    }

    // ===== Fields =====
    [HttpGet("kpi/fields")]
    public async Task<ActionResult<IEnumerable<KpiFieldDto>>> GetFields(CancellationToken ct)
        => Ok(await _adminService.GetFieldsAsync(ct));

    [HttpPost("kpi/fields")]
    public async Task<IActionResult> CreateField([FromBody] KpiFieldUpsertDto dto, CancellationToken ct)
    {
        var id = await _adminService.CreateFieldAsync(dto, ct);
        return CreatedAtAction(nameof(GetFields), new { id }, new { id });
    }

    [HttpPut("kpi/fields/{id:int}")]
    public async Task<IActionResult> UpdateField(int id, [FromBody] KpiFieldUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateFieldAsync(id, dto, ct); return NoContent(); }

    [HttpDelete("kpi/fields/{id:int}")]
    public async Task<IActionResult> DeleteField(int id, CancellationToken ct)
    { await _adminService.DeleteFieldAsync(id, ct); return NoContent(); }

    // ===== Items (list) =====
    [HttpGet("kpi/items")]
    public async Task<ActionResult<PagedResult<KpiItemRowDto>>> SearchItems(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] int? fieldId = null, [FromQuery] int? category = null,
        [FromQuery] int? orgId = null, [FromQuery] string? q = null, CancellationToken ct = default)
        => Ok(await _adminService.SearchItemsAsync(page, pageSize, fieldId, category, orgId, q, ct));

    // ===== Item detail =====
    [HttpGet("kpi/items/{id:int}")]
    public async Task<ActionResult<KpiItemDetailDto>> GetItem(int id, CancellationToken ct)
        => Ok(await _adminService.GetItemAsync(id, ct));

    [HttpPost("kpi/items")]
    public async Task<IActionResult> CreateItem([FromBody] KpiItemUpsertDto dto, CancellationToken ct)
    {
        var id = await _adminService.CreateItemAsync(dto, ct);
        return CreatedAtAction(nameof(GetItem), new { id }, new { id });
    }

    [HttpPut("kpi/items/{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] KpiItemUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateItemAsync(id, dto, ct); return NoContent(); }

    [HttpDelete("kpi/items/{id:int}")]
    public async Task<IActionResult> DeleteItem(int id, CancellationToken ct)
    { await _adminService.DeleteItemAsync(id, ct); return NoContent(); }

    // ===== ItemName（版本）=====
    [HttpPost("kpi/items/{id:int}/names")]
    public async Task<IActionResult> AddItemName(int id, [FromBody] KpiItemNameUpsertDto dto, CancellationToken ct)
    { await _adminService.AddItemNameAsync(id, dto, User.Identity?.Name ?? "", ct); return NoContent(); }

    [HttpPut("kpi/items/{id:int}/names/{nameId:int}")]
    public async Task<IActionResult> UpdateItemName(int id, int nameId, [FromBody] KpiItemNameUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateItemNameAsync(id, nameId, dto, ct); return NoContent(); }

    [HttpDelete("kpi/items/{id:int}/names/{nameId:int}")]
    public async Task<IActionResult> DeleteItemName(int id, int nameId, CancellationToken ct)
    { await _adminService.DeleteItemNameAsync(id, nameId, ct); return NoContent(); }

    // ===== DetailItem =====
    [HttpPost("kpi/items/{id:int}/details")]
    public async Task<IActionResult> AddDetailItem(int id, [FromBody] KpiDetailItemUpsertDto dto, CancellationToken ct)
    { var detailId = await _adminService.AddDetailItemAsync(id, dto, ct); return Created("", new { id = detailId }); }

    [HttpPut("kpi/details/{detailId:int}")]
    public async Task<IActionResult> UpdateDetailItem(int detailId, [FromBody] KpiDetailItemUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateDetailItemAsync(detailId, dto, ct); return NoContent(); }

    [HttpDelete("kpi/details/{detailId:int}")]
    public async Task<IActionResult> DeleteDetailItem(int detailId, CancellationToken ct)
    { await _adminService.DeleteDetailItemAsync(detailId, ct); return NoContent(); }

    // ===== DetailItemName（版本）=====
    [HttpPost("kpi/details/{detailId:int}/names")]
    public async Task<IActionResult> AddDetailItemName(int detailId, [FromBody] KpiDetailItemNameUpsertDto dto, CancellationToken ct)
    { await _adminService.AddDetailItemNameAsync(detailId, dto, User.Identity?.Name ?? "", ct); return NoContent(); }

    [HttpPut("kpi/details/{detailId:int}/names/{nameId:int}")]
    public async Task<IActionResult> UpdateDetailItemName(int detailId, int nameId, [FromBody] KpiDetailItemNameUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateDetailItemNameAsync(detailId, nameId, dto, ct); return NoContent(); }

    [HttpDelete("kpi/details/{detailId:int}/names/{nameId:int}")]
    public async Task<IActionResult> DeleteDetailItemName(int detailId, int nameId, CancellationToken ct)
    { await _adminService.DeleteDetailItemNameAsync(detailId, nameId, ct); return NoContent(); }

    // ===== KpiData（基線/目標）=====
    [HttpGet("kpi/details/{detailId:int}/data")]
    public async Task<ActionResult<IEnumerable<KpiDataDto>>> ListKpiData(int detailId, CancellationToken ct)
        => Ok(await _adminService.ListKpiDataAsync(detailId, ct));

    [HttpPost("kpi/details/{detailId:int}/data")]
    public async Task<IActionResult> AddKpiData(int detailId, [FromBody] KpiDataUpsertDto dto, CancellationToken ct)
    { var id = await _adminService.AddKpiDataAsync(detailId, dto, ct); return Created("", new { id }); }

    [HttpPut("kpi/data/{dataId:int}")]
    public async Task<IActionResult> UpdateKpiData(int dataId, [FromBody] KpiDataUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateKpiDataAsync(dataId, dto, ct); return NoContent(); }

    [HttpDelete("kpi/data/{dataId:int}")]
    public async Task<IActionResult> DeleteKpiData(int dataId, CancellationToken ct)
    { await _adminService.DeleteKpiDataAsync(dataId, ct); return NoContent(); }

    // ===== KpiReport（含狀態流轉）=====
    [HttpGet("kpi/data/{dataId:int}/reports")]
    public async Task<ActionResult<IEnumerable<KpiReportDto>>> ListReports(int dataId, CancellationToken ct)
        => Ok(await _adminService.ListReportsAsync(dataId, ct));

    [HttpPost("kpi/data/{dataId:int}/reports")]
    public async Task<IActionResult> AddReport(int dataId, [FromBody] KpiReportUpsertDto dto, CancellationToken ct)
    { var id = await _adminService.AddReportAsync(dataId, dto, ct); return Created("", new { id }); }

    [HttpPut("kpi/reports/{reportId:int}")]
    public async Task<IActionResult> UpdateReport(int reportId, [FromBody] KpiReportUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateReportAsync(reportId, dto, ct); return NoContent(); }

    [HttpDelete("kpi/reports/{reportId:int}")]
    public async Task<IActionResult> DeleteReport(int reportId, CancellationToken ct)
    { await _adminService.DeleteReportAsync(reportId, ct); return NoContent(); }

    // 狀態流轉：submit/review/return/finalize
    [HttpPatch("kpi/reports/{reportId:int}/status")]
    public async Task<IActionResult> ChangeReportStatus(int reportId, [FromBody] byte newStatus, CancellationToken ct)
    { await _adminService.ChangeReportStatusAsync(reportId, newStatus, ct); return NoContent(); }
    
    // ===== KpiCycles =====
    [HttpGet("kpi/cycles")]
    public async Task<ActionResult<IEnumerable<KpiCycleDto>>> ListCycles(CancellationToken ct)
        => Ok(await _adminService.ListCyclesAsync(ct));

    [HttpPost("kpi/cycles")]
    public async Task<IActionResult> CreateCycle([FromBody] KpiCycleUpsertDto dto, CancellationToken ct)
    {
        var id = await _adminService.CreateCycleAsync(dto, ct);
        return Created("", new { id });
    }

    [HttpPut("kpi/cycles/{id:int}")]
    public async Task<IActionResult> UpdateCycle(int id, [FromBody] KpiCycleUpsertDto dto, CancellationToken ct)
    {
        await _adminService.UpdateCycleAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("kpi/cycles/{id:int}")]
    public async Task<IActionResult> DeleteCycle(int id, CancellationToken ct)
    {
        await _adminService.DeleteCycleAsync(id, ct);
        return NoContent();
    }
    
    
    // ===== 下拉 =====
    [HttpGet("suggest/event-types")]
    public async Task<ActionResult<IEnumerable<SuggestEventTypeDto>>> ListEventTypes(CancellationToken ct)
        => Ok(await _adminService.ListEventTypesAsync(ct));

    [HttpGet("suggest/suggestion-types")]
    public async Task<ActionResult<IEnumerable<SuggestionTypeDto>>> ListSuggestionTypes(CancellationToken ct)
        => Ok(await _adminService.ListSuggestionTypesAsync(ct));

    [HttpGet("suggest/is-adopted-options")]
    public async Task<ActionResult<IEnumerable<IsAdoptedOption>>> ListIsAdoptedOptions()
        => Ok(await _adminService.ListIsAdoptedOptionsAsync());

    // ===== SuggestDate（督導主檔）=====
    [HttpGet("suggest/dates")]
    public async Task<ActionResult<PagedResult<SuggestDateRowDto>>> SearchDates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? orgId = null,
        [FromQuery] int? eventTypeId = null,
        [FromQuery] int? year = null,
        CancellationToken ct = default)
        => Ok(await _adminService.SearchSuggestDatesAsync(page, pageSize, orgId, eventTypeId, year, ct));

    [HttpGet("suggest/dates/{id:int}")]
    public async Task<ActionResult<SuggestDateDetailDto>> GetDate(int id, CancellationToken ct)
        => Ok(await _adminService.GetSuggestDateAsync(id, ct));

    [HttpPost("suggest/dates")]
    public async Task<IActionResult> CreateDate([FromBody] SuggestDateUpsertDto dto, CancellationToken ct)
    {
        var id = await _adminService.CreateSuggestDateAsync(dto, ct);
        return CreatedAtAction(nameof(GetDate), new { id }, new { id });
    }

    [HttpPut("suggest/dates/{id:int}")]
    public async Task<IActionResult> UpdateDate(int id, [FromBody] SuggestDateUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateSuggestDateAsync(id, dto, ct); return NoContent(); }

    [HttpDelete("suggest/dates/{id:int}")]
    public async Task<IActionResult> DeleteDate(int id, CancellationToken ct)
    { await _adminService.DeleteSuggestDateAsync(id, ct); return NoContent(); }

    // ===== SuggestReport（建議報告）=====
    [HttpGet("suggest/dates/{dateId:int}/reports")]
    public async Task<ActionResult<IEnumerable<SuggestReportRowDto>>> ListReportsByDate(int dateId, CancellationToken ct)
        => Ok(await _adminService.ListReportsByDateAsync(dateId, ct));

    [HttpGet("suggest/reports/{id:int}")]
    public async Task<ActionResult<SuggestReportRowDto>> GetReport(int id, CancellationToken ct)
        => Ok(await _adminService.GetReportAsync(id, ct));

    [HttpPost("suggest/dates/{dateId:int}/reports")]
    public async Task<IActionResult> CreateReport(int dateId, [FromBody] SuggestReportUpsertDto dto, CancellationToken ct)
    {
        var userId = GetUserIdOrThrow();
        var id = await _adminService.CreateReportAsync(dateId, dto, userId, ct);
        return CreatedAtAction(nameof(GetReport), new { id }, new { id });
    }

    [HttpPut("suggest/reports/{id:int}")]
    public async Task<IActionResult> UpdateReport(int id, [FromBody] SuggestReportUpsertDto dto, CancellationToken ct)
    { await _adminService.UpdateReportAsync(id, dto, ct); return NoContent(); }

    // 跨日期搜尋（儀表板/統計）
    [HttpGet("suggest/reports/search")]
    public async Task<ActionResult<PagedResult<SuggestReportRowDto>>> SearchReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? orgId = null,
        [FromQuery] int? suggestionTypeId = null,
        [FromQuery] int? kpiFieldId = null,
        [FromQuery] int? year = null,
        [FromQuery] string? q = null,
        CancellationToken ct = default)
        => Ok(await _adminService.SearchReportsAsync(page, pageSize, orgId, suggestionTypeId, kpiFieldId, year, q, ct));

    private Guid GetUserIdOrThrow()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            throw new InvalidOperationException("無法解析使用者識別");
        return userId;
    }
}