using Loco1.Service.Abstractions;
using Loco1.ViewModels.Locomotives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GCommon; // Perm

namespace Loco1.Web.Controllers
    {
    [Authorize(Policy = Perm.Loco_View)]
    public class LocomotiveController : Controller
        {
        private readonly ILocomotiveService _svc;

        public LocomotiveController(ILocomotiveService svc)
            {
            _svc = svc;
            }

        public async Task<IActionResult> Index()
            {
            var model = await _svc.GetAllAsync();
            return View(model);
            }

        [Authorize(Policy = Perm.Loco_Add)]
        public IActionResult Create()
            {
            return View(new LocoEditVm());
            }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Add)]
        public async Task<IActionResult> Create(LocoEditVm vm)
            {
            if (!ModelState.IsValid) return View(vm);

            var actor = User?.Identity?.Name ?? "system";
            await _svc.CreateAsync(vm, actor);
            return RedirectToAction(nameof(Index));
            }

        [Authorize(Policy = Perm.Loco_Edit)]
        public async Task<IActionResult> Edit(int id)
            {
            var vm = await _svc.GetForEditAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
            }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Edit)]
        public async Task<IActionResult> Edit(LocoEditVm vm)
            {
            if (!ModelState.IsValid) return View(vm);

            var actor = User?.Identity?.Name ?? "system";
            var ok = await _svc.UpdateAsync(vm, actor);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
            }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Loco_Delete)]
        public async Task<IActionResult> Delete(int id, string? note)
            {
            var actor = User?.Identity?.Name ?? "system";
            var ok = await _svc.DeleteAsync(id, actor, note);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
            }
        }
    }