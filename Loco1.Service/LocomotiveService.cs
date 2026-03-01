using Loco1.Data.Models;
using Loco1.Repositories.Interfaces;
using Loco1.Service.Abstractions;
using Loco1.ViewModels.Locomotives;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Service
    {
    public class LocomotiveService : ILocomotiveService
        {
        private readonly ILocomotiveRepository _repo;

        public LocomotiveService(ILocomotiveRepository repo)
            {
            _repo = repo;
            }

        // List page
        public async Task<List<LocoListVm>> GetAllAsync()
            {
            var items = await _repo.GetAllAsync(); // repo already filters IsDeleted == false
            return items.Select(x => new LocoListVm
                {
                Id = x.Id,
                Number = x.Number,
                Type = x.Type,
                MeasuringUnit = x.MeasuringUnit,
                AxleCount = x.AxleCount,
                TotalEngineHours = x.TotalEngineHours
                })
            .ToList();
            }

        // Load for edit
        public async Task<LocoEditVm?> GetForEditAsync(int id)
            {
            var x = await _repo.GetByIdAsync(id); // may return deleted
            if (x == null || x.IsDeleted) return null;

            return new LocoEditVm
                {
                Id = x.Id,
                Number = x.Number,
                Type = x.Type,
                MeasuringUnit = x.MeasuringUnit,
                FuelCapacity = x.FuelCapacity,
                AxleCount = x.AxleCount,
                TotalEngineHours = x.TotalEngineHours,
                TotalWorkingDays = x.TotalWorkingDays,
                LastPlannedRepairType = x.LastPlannedRepairType,
                LastPlannedRepairDate = x.LastPlannedRepairDate,
                LastAxleMeasurementDate = x.LastAxleMeasurementDate,
                InterAxleMeasurementPeriodDays = x.InterAxleMeasurementPeriodDays
                };
            }

        // Create
        public async Task<int> CreateAsync(LocoEditVm vm, string actor)
            {
            // unique check
            if (await _repo.ExistsByNumberAsync(vm.Number.Trim(), null))
                throw new InvalidOperationException("Validation_Unique_Locomotive_Number"); // or return a code you prefer

            var entity = new Locomotive
                {
                Number = vm.Number.Trim(),
                Type = vm.Type,
                MeasuringUnit = vm.MeasuringUnit,
                FuelCapacity = vm.FuelCapacity,
                AxleCount = vm.AxleCount,
                TotalEngineHours = vm.TotalEngineHours,
                TotalWorkingDays = vm.TotalWorkingDays,
                LastPlannedRepairType = vm.LastPlannedRepairType,
                LastPlannedRepairDate = vm.LastPlannedRepairDate,
                LastAxleMeasurementDate = vm.LastAxleMeasurementDate,
                InterAxleMeasurementPeriodDays = vm.InterAxleMeasurementPeriodDays,

                CreatedOn = DateTime.UtcNow,
                CreatedBy = string.IsNullOrWhiteSpace(actor) ? "system" : actor,
                IsDeleted = false
                };

            await _repo.AddAsync(entity);
            return entity.Id;
            }

        // Update
        public async Task<bool> UpdateAsync(LocoEditVm vm, string actor)
            {
            var entity = await _repo.GetByIdAsync(vm.Id ?? 0);
            if (entity == null || entity.IsDeleted) return false;

            // unique check (exclude current Id)
            if (await _repo.ExistsByNumberAsync(vm.Number.Trim(), entity.Id))
                throw new InvalidOperationException("Validation_Unique_Locomotive_Number"); // or return a code you prefer

            entity.Number = vm.Number.Trim();
            entity.Type = vm.Type;
            entity.MeasuringUnit = vm.MeasuringUnit;
            entity.FuelCapacity = vm.FuelCapacity;
            entity.AxleCount = vm.AxleCount;
            entity.TotalEngineHours = vm.TotalEngineHours;
            entity.TotalWorkingDays = vm.TotalWorkingDays;
            entity.LastPlannedRepairType = vm.LastPlannedRepairType;
            entity.LastPlannedRepairDate = vm.LastPlannedRepairDate;
            entity.LastAxleMeasurementDate = vm.LastAxleMeasurementDate;
            entity.InterAxleMeasurementPeriodDays = vm.InterAxleMeasurementPeriodDays;

            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = string.IsNullOrWhiteSpace(actor) ? "system" : actor;

            await _repo.UpdateAsync(entity);
            return true;
            }

        // Soft delete (admin only)
        public async Task<bool> DeleteAsync(int id, string actor, string? note = null)
            {
            return await _repo.DeleteAsync(id, actor, note);
            }

        // Soft un-delete (admin only)
        public async Task<bool> UndeleteAsync(int id, string actor)
            {
            return await _repo.UndeleteAsync(id, actor);
            }
        }
    }