using Loco1.Data.Models;
using Loco1.Repositories.Interfaces;
using Loco1.Service.Abstractions;
using Loco1.ViewModels.Locomotives;

namespace Loco1.Service
{
    public class LocomotiveService(ILocomotiveRepository repo) : ILocomotiveService
    {
        private readonly ILocomotiveRepository _repo = repo;

        // List page (Index) - can be slimmed to Id/Number if you prefer
        public async Task<List<LocoListVm>> GetAllAsync()
        {
            // Repo should return only non-deleted items (global filter or explicit where)
            var items = await _repo.GetAllAsync();

            return [.. items
                .Select(x => new LocoListVm
                {
                    Id = x.Id,
                    Number = x.Number,
                    Type = x.Type,
                    MeasuringUnit = x.MeasuringUnit,
                    AxleCount = x.AxleCount,
                    TotalEngineHours = x.TotalEngineHours
                })];
        }

        // Load for edit/details
        public async Task<LocoEditVm?> GetForEditAsync(int id)
        {
            // If repo respects global filter, deleted rows won't be returned anyway.
            var x = await _repo.GetByIdAsync(id);
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
                InterAxleMeasurementPeriodDays = x.InterAxleMeasurementPeriodDays,
                IsDeleted = x.IsDeleted,
                DateDeleted = x.DateDeleted,
                DeletedBy = x.DeletedBy

            };
        }

        // Create
        public async Task<int> CreateAsync(LocoEditVm vm, string actor)
        {
            // Normalize number
            var number = (vm.Number ?? string.Empty).Trim();

            // Unique check (exclude null id)
            if (await _repo.ExistsByNumberAsync(number, null))
                throw new InvalidOperationException("Validation_Unique_Locomotive_Number");

            var entity = new Locomotive
            {
                Number = number,
                Type = vm.Type,
                MeasuringUnit = vm.MeasuringUnit,
                FuelCapacity = vm.FuelCapacity,
                AxleCount = vm.AxleCount,
                TotalEngineHours = vm.TotalEngineHours,
                TotalWorkingDays = vm.TotalWorkingDays,
                LastPlannedRepairType = vm.LastPlannedRepairType,
                LastPlannedRepairDate = vm.LastPlannedRepairDate,
                LastAxleMeasurementDate = vm.LastAxleMeasurementDate,
                InterAxleMeasurementPeriodDays = vm.InterAxleMeasurementPeriodDays

                // Audit fields (Created*/IsDeleted) are handled by DbContext.ApplyAudit()
            };

            await _repo.AddAsync(entity);
            return entity.Id;
        }

        // Update
        public async Task<bool> UpdateAsync(LocoEditVm vm, string actor)
        {
            if (vm.Id is null) return false;

            var entity = await _repo.GetByIdAsync(vm.Id.Value);
            if (entity == null || entity.IsDeleted) return false;

            var newNumber = (vm.Number ?? string.Empty).Trim();

            // Unique check (exclude current Id)
            if (await _repo.ExistsByNumberAsync(newNumber, entity.Id))
                throw new InvalidOperationException("Validation_Unique_Locomotive_Number");

            entity.Number = newNumber;
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

            // Audit fields (Modified*) are handled by DbContext.ApplyAudit()
            await _repo.UpdateAsync(entity);
            return true;
        }

        // Soft delete (admin only)
        public async Task<bool> DeleteAsync(int id, string actor, string? note = null)
        {
            // Repo should mark IsDeleted = true and optionally set Note (and SaveChanges)
            return await _repo.DeleteAsync(id, actor, note);
        }

        // Soft un-delete (admin only)
        public async Task<bool> UndeleteAsync(int id, string actor)
        {
            // Repo should IgnoreQueryFilters(), clear IsDeleted, set Modified*, and SaveChanges
            return await _repo.UndeleteAsync(id, actor);
        }
    }
}