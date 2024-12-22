using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

public sealed class AddressService: DbServiceBase, IAddressService {
    
    public AddressService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<AddressBookVM?> GetAddressBookByProfileId(string profileId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(GetAddressBookByProfileId) });
    }

    public async Task<string?> InsertNewAddress(Address address) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(InsertNewAddress) });
    }

    public async Task<string?> InsertNewProfileAddress(ProfileAddress profileAddress) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(InsertNewProfileAddress) });
    }

    public async Task<Address?> GetAddress(string addressId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(GetAddress) });
    }

    public async Task<bool?> UpdateAddress(Address address) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(UpdateAddress) });
    }

    public async Task<bool?> DeleteAddress(string addressId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(DeleteAddress) });
    }

    public async Task<bool?> DeleteProfileAddress(string profileId, string addressId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(DeleteProfileAddress) });
    }

    public async Task<ProfileAddress?> GetProfileAddress(string profileId, string addressId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(GetProfileAddress) });
    }

    public async Task<bool?> UpdateProfileAddress(ProfileAddress profileAddress) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(UpdateProfileAddress) });
    }
}