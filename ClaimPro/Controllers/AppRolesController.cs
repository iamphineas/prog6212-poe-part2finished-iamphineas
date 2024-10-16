using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClaimPro.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AppRolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppRolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // List all the roles created by users
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList(); // Fetch all roles
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                if (!await _roleManager.RoleExistsAsync(model.Name))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Name));
                }
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}


