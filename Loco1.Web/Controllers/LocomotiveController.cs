using GCommon;                              // Perm
using Loco1.Localizer;                      // SharedResource
using Loco1.Service.Abstractions;           // ILocomotiveService
using Loco1.ViewModels.Locomotives;         // LocoEditVm, LocoListVm
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
 

namespace Loco1.Web.Controllers
{
    // Auth gate for the whole controller: any authenticated user can access.
    // Per-action policies below will protect Details/Create/Edit/Delete.
    [Authorize]
    public class LocomotiveController(ILocomotiveService svc, IStringLocalizer<SharedResource> localizer) : Controller
    {
        private readonly ILocomotiveService _svc = svc;
        private readonly IStringLocalizer<SharedResource> _L = localizer;

        // GET: /Locomotive
        // Everyone authenticated sees the list (numbers only in the Index view).
        public async Task<IActionResult> Index()
        {
            // Service should return a lightweight list VM (Id, Number)
            IEnumerable<LocoListVm> model = await _svc.GetAllAsync();
            return View(model);
        }

        // GET: /Locomotive/Details/{id}
        // Only users with Loco_View can open details (buttons Edit/Delete will be shown there based on rights).
        [Authorize(Policy = Perm.Loco_View)]
        public async Task<IActionResult> Details(int id)
        {
            var vm = await _svc.GetForEditAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // GET: /Locomotive/Create
        // Only users with Loco_Add can open create form (Index shows a Create button conditionally).
        [Authorize(Policy = Perm.Loco_Add)]
        public IActionResult Create()
        {
            return View(new LocoEditVm());
        }

        // POST: /Locomotive/Create
        // Only users with Loco_Add can submit create.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Add)]
        public async Task<IActionResult> Create(LocoEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Actor used for auditing
            var actor = User?.Identity?.Name ?? "system";

            try
            {
                var id = await _svc.CreateAsync(vm, actor);
                TempData["Success"] = _L["Created successfully"].Value;
                return RedirectToAction(nameof(Details), new { id });
            }
            // Service throws InvalidOperationException with message key "Validation_Unique_Locomotive_Number"
            // when locomotive number must be unique.
            catch (InvalidOperationException ex) when (ex.Message == "Validation_Unique_Locomotive_Number")
            {
                ModelState.AddModelError(nameof(vm.Number), _L["Validation_Unique_Locomotive_Number"].Value);
                return View(vm);
            }
        }

        // GET: /Locomotive/Edit/{id}
        // Only users with Loco_Edit can edit.
        [Authorize(Policy = Perm.Loco_Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _svc.GetForEditAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: /Locomotive/Edit/{id}
        // Only users with Loco_Edit can submit edit.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Edit)]
        public async Task<IActionResult> Edit(int id, LocoEditVm vm)
        {
            if (vm.Id != id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var actor = User?.Identity?.Name ?? "system";

            try
            {
                var ok = await _svc.UpdateAsync(vm, actor);
                if (!ok) return NotFound();

                TempData["Success"] = _L["Saved successfully"].Value;
                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }
            catch (InvalidOperationException ex) when (ex.Message == "Validation_Unique_Locomotive_Number")
            {
                ModelState.AddModelError(nameof(vm.Number), _L["Validation_Unique_Locomotive_Number"].Value);
                return View(vm);
            }
        }

        // POST: /Locomotive/Delete/{id}
        // Soft delete protected by Loco_Delete.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Delete)]
        public async Task<IActionResult> Delete(int id, string? note)
        {
            var actor = User?.Identity?.Name ?? "system";
            var ok = await _svc.DeleteAsync(id, actor, note);
            if (!ok) return NotFound();

            TempData["Success"] = _L["Deleted successfully"].Value;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Locomotive/Undelete/{id}
        // Soft undelete protected by Loco_Delete (use a separate policy if needed).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Delete)]
        public async Task<IActionResult> Undelete(int id)
        {
            var actor = User?.Identity?.Name ?? "system";
            var ok = await _svc.UndeleteAsync(id, actor);
            if (!ok) return NotFound();

            TempData["Success"] = _L["Restored successfully"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}