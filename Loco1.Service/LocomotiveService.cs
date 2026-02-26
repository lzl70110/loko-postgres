using Loco1.Data;
using Loco1.Data.Models;
using Loco1.Service.Abstractions;
using Loco1.ViewModels.Locomotives;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Service
    {
    public class LocomotiveService : ILocomotiveService
        {
        private readonly LocoDbContext _db;

        public LocomotiveService(LocoDbContext db)
            {
            _db = db;
            }

        // List page
        public async Task<List<LocoListVm>> GetAllAsync()
            {
            return await _db.Locomotives
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Number)
                .Select(x => new LocoListVm
                    {
                    Id = x.Id,
                    Number = x.Number,
                    Type = x.Type,
                    MeasuringUnit = x.MeasuringUnit,
                    AxleCount = x.AxleCount,
                    TotalEngineHours = x.TotalEngineHours
                    })
                .ToListAsync();
            }

        // Load for edit
        public async Task<LocoEditVm?> GetForEditAsync(int id)
            {
            var x = await _db.Locomotives
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (x == null) return null;

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
                CreatedBy = string.IsNullOrWhiteSpace(actor) ? "system" : actor
                };

            _db.Locomotives.Add(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
            }

        // Update
        public async Task<bool> UpdateAsync(LocoEditVm vm, string actor)
            {
            var entity = await _db.Locomotives.FirstOrDefaultAsync(l => l.Id == vm.Id && !l.IsDeleted);
            if (entity == null) return false;

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

            await _db.SaveChangesAsync();
            return true;
            }

        // Soft delete (admin only)
        public async Task<bool> DeleteAsync(int id, string actor, string? note = null)
            {
            var entity = await _db.Locomotives.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.Note = note; // admin/system note if provided
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = string.IsNullOrWhiteSpace(actor) ? "system" : actor;

            await _db.SaveChangesAsync();
            return true;
            }
        }
    }