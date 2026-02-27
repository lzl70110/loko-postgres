using Loco1.ViewModels.Locomotives;

namespace Loco1.Service.Abstractions
    {
    public interface ILocomotiveService
        {
        Task<List<LocoListVm>> GetAllAsync();
        Task<LocoEditVm?> GetForEditAsync(int id);

        Task<int> CreateAsync(LocoEditVm vm, string actor);
        Task<bool> UpdateAsync(LocoEditVm vm, string actor);

        Task<bool> DeleteAsync(int id, string actor, string? note = null); // soft delete

        Task<bool> UndeleteAsync(int id, string actor); //   
        }
    }