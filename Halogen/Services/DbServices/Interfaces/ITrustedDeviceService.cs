using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface ITrustedDeviceService {

    Task<TrustedDevice[]?> GetTrustedDevicesForAccount(string accountId);
}