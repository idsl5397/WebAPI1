using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class EnterpriseController : ControllerBase
    {
        private readonly isha_sys_devContext _db;
        
        public EnterpriseController(isha_sys_devContext db)
        {
            _db = db;
        }
        public class TreeNode
        {
            public string Id { get; set; }       // 節點的 ID
            public string Name { get; set; }     // 節點的名稱
            public List<TreeNode> Children { get; set; }  // 子節點集合
        }
        
        [HttpGet("GetEnterprise")]
        public IActionResult GetEnterprise(int? companyId = null)
        {
           
            // 預先載入企業、公司及工廠的關聯資料
            var enterprises = _db.EnterpriseNames
                .Include(e => e.CompanyNames)
                .ThenInclude(c => c.FactoryNames)
                .ToList();

            // 如果有傳入 companyId，則固定選擇對應的企業及公司名稱
            if (companyId.HasValue)
            {
                var company = _db.CompanyNames
                    .Include(c => c.Enterprise)
                    .Where(c => c.Id == companyId.Value)
                    .FirstOrDefault();

                if (company != null)
                {
                    var enterpriseName = company.Enterprise.enterprise;
                    var companyName = company.company;

                    var enterpriseTree = new TreeNode
                    {
                        Id = company.Enterprise.Id.ToString(),
                        Name = enterpriseName,
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                Id = company.Id.ToString(),
                                Name = companyName,
                                Children = company.FactoryNames != null ? company.FactoryNames.Select(f => new TreeNode
                                {
                                    Id = f.Id.ToString(),
                                    Name = f.factory,
                                    Children = new List<TreeNode>() // 工廠沒有子節點
                                }).ToList() : new List<TreeNode>() // 如果 FactoryNames 為 null，給一個空陣列
                            }
                        }
                    };

                    return Ok(new { enterpriseTree });
                }
                else
                {
                    return NotFound("公司未找到");
                }
            }
            else
            {
                // 如果沒有傳入 companyId，則返回整個樹狀結構
                var enterprise_name = enterprises.Select(e => new TreeNode
                {
                    Id = e.Id.ToString(),
                    Name = e.enterprise,
                    Children = e.CompanyNames.Select(c => new TreeNode
                    {
                        Id = c.Id.ToString(),
                        Name = c.company,
                        Children = c.FactoryNames.Select(f => new TreeNode
                        {
                            Id = f.Id.ToString(),
                            Name = f.factory,
                            Children = new List<TreeNode>() // 工廠沒有子節點
                        }).ToList()
                    }).ToList()
                }).ToList();

                return Ok(enterprise_name);
            }
        }
        
    }
    
}
