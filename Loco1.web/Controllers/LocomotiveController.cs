using GCommon; // Perm
using Loco1.Localizer;                 // SharedResource
using Loco1.Service.Abstractions;      // ILocomotiveService
using Loco1.ViewModels.Locomotives;    // LocoEditVm, LocoListVm
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Loco1.Web.Controllers
    {
    [Authorize(Policy = Perm.Loco_View)]
    public class LocomotiveController : Controller
        {
        private readonly ILocomotiveService _svc;
        private readonly IStringLocalizer<SharedResource> _L;

        public LocomotiveController(ILocomotiveService svc, IStringLocalizer<SharedResource> localizer)
            {
            _svc = svc;
            _L = localizer;
            }

        // GET: /Locomotive
        public async Task<IActionResult> Index()
            {
            var model = await _svc.GetAllAsync();
            return View(model);
            }

        // GET: /Locomotive/Details/5
        public async Task<IActionResult> Details(int id)
            {
            var vm = await _svc.GetForEditAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
            }

        // GET: /Locomotive/Create
        [Authorize(Policy = Perm.Loco_Add)]
        public IActionResult Create()
            {
            return View(new LocoEditVm());
            }

        // POST: /Locomotive/Create
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Add)]
        public async Task<IActionResult> Create(LocoEditVm vm)
            {
            if (!ModelState.IsValid) return View(vm);

            var actor = User?.Identity?.Name ?? "system";

            try
                {
                var id = await _svc.CreateAsync(vm, actor);
                TempData["Success"] = _L["Created successfully"].Value;
                return RedirectToAction(nameof(Details), new { id });
                }
            catch (InvalidOperationException ex) when (ex.Message == "Validation_Unique_Locomotive_Number")
                {
                ModelState.AddModelError(nameof(vm.Number), _L["Validation_Unique_Locomotive_Number"].Value);
                return View(vm);
                }
            }

        // GET: /Locomotive/Edit/5
        [Authorize(Policy = Perm.Loco_Edit)]
        public async Task<IActionResult> Edit(int id)
            {
            var vm = await _svc.GetForEditAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
            }

        // POST: /Locomotive/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
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
                return RedirectToAction(nameof(Details), new { id });
                }
            catch (InvalidOperationException ex) when (ex.Message == "Validation_Unique_Locomotive_Number")
                {
                ModelState.AddModelError(nameof(vm.Number), _L["Validation_Unique_Locomotive_Number"].Value);
                return View(vm);
                }
            }

        // POST: /Locomotive/Delete/5  (soft delete)
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Delete)]
        public async Task<IActionResult> Delete(int id, string? note)
            {
            var actor = User?.Identity?.Name ?? "system";
            var ok = await _svc.DeleteAsync(id, actor, note);
            if (!ok) return NotFound();

            TempData["Success"] = _L["Deleted successfully"].Value;
            return RedirectToAction(nameof(Index));
            }

        // POST: /Locomotive/Undelete/5  (soft undelete)
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Delete)] // ако имаш отделна политика, смени тук
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
 