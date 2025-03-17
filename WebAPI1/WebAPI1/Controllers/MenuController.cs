using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class MenuController : ControllerBase
    {
        private readonly isha_sys_devContext _db;

        public MenuController(isha_sys_devContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 取得使用者對應的選單
        /// </summary>
        
        public class MenuItem
        {
            public int Id { get; set; }
            public string Label { get; set; }
            public string Link { get; set; }
            public string? Icon { get; set; }
            public int? ParentId { get; set; }
            public int SortOrder { get; set; }
            public int IsActive { get; set; }
            public string MenuType { get; set; }
            public List<MenuItem> Children { get; set; } = new();
        }
        
        [HttpGet("GetMenus")]
        public IActionResult GetMenus()
        {
            var menuItems = _db.Menus.Where(m => m.IsActive == 0).ToList();

            var menuTree = menuItems
                .Where(m => m.ParentId == null) // 找出所有主選單
                .Select(m => new MenuItem
                {
                    Id = m.Id,
                    Label = m.Lable,
                    Link = m.Link,
                    Icon = m.Icon,
                    ParentId = m.ParentId,
                    SortOrder = m.SortOrder,
                    IsActive = m.IsActive,
                    MenuType = m.MenuType,
                    Children = GetChildren(menuItems, m.Id) // 取得子選單
                })
                .OrderBy(m => m.SortOrder)
                .ToList();

            return Ok(menuTree);
        }
        
        private List<MenuItem> GetChildren(List<Menu> menuItems, int parentId)
        {
            return menuItems
                .Where(m => m.ParentId == parentId)
                .Select(m => new MenuItem
                {
                    Id = m.Id,
                    Label = m.Lable,
                    Link = m.Link,
                    Icon = m.Icon,
                    ParentId = m.ParentId,
                    SortOrder = m.SortOrder,
                    IsActive = m.IsActive,
                    MenuType = m.MenuType,
                    Children = GetChildren(menuItems, m.Id)
                })
                .OrderBy(m => m.SortOrder)
                .ToList();
        }
    }
}