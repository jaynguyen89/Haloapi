using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class AddressService: DbServiceBase, IAddressService {
    
    public AddressService(
        ILoggerService logger,
        HalogenDbContext dbContext,
        IHaloServiceFactory haloServiceFactory
    ): base(logger, dbContext, haloServiceFactory) { }

    public async Task<AddressBookVM?> GetAddressBookByProfileId(string profileId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(GetAddressBookByProfileId) });

        try {
            var addresses = await _dbContext.ProfileAddresses
                .Where(a => a.ProfileId == profileId)
                .Join(
                    _dbContext.Addresses,
                    profileAddress => profileAddress.AddressId,
                    address => address.Id,
                    (profileAddress, address) => new {
                        ProfileAddress = profileAddress,
                        Address = address,
                    }
                )
                .Select(x => AddressVM.CreateAddressVm(x.ProfileAddress, x.Address))
                .ToArrayAsync();

            var countryIdsMap = addresses
                .Select(address => address.Address.Division.CountryId)
                .Distinct()
                .Select(async (countryId) => {
                    var country = await _dbContext.Localities.FindAsync(countryId);
                    return new {
                        Id = countryId,
                        Country = country,
                    };
                })
                .Select(task => task.Result)
                .ToDictionary(x => x.Id, x => x.Country);

            var divisionIdsMap = addresses
                .Select(address => address.Address.Division.Id)
                .Distinct()
                .Select(async (divisionId) => {
                    var division = await _dbContext.LocalityDivisions.FindAsync(divisionId);
                    return new {
                        Id = divisionId,
                        Division = division,
                    };
                })
                .Select(task => task.Result)
                .ToDictionary(x => x.Id, x => x.Division);
            
            
            
            foreach (var address in addresses) {
                address.Address.Division = divisionIdsMap[address.Address.Division.Id]!;
                address.Address.Country = countryIdsMap[address.Address.Division.CountryId]!;
            }

            return new AddressBookVM {
                ProfileId = profileId,
                Addresses = addresses,
            };
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(GetAddressBookByProfileId)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> InsertNewAddress(Address address) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(InsertNewAddress) });

        try {
            await _dbContext.Addresses.AddAsync(address);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(InsertNewAddress)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> InsertNewProfileAddress(ProfileAddress profileAddress) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(InsertNewProfileAddress) });

        try {
            await _dbContext.ProfileAddresses.AddAsync(profileAddress);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(InsertNewProfileAddress)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<Address?> GetAddress(string addressId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(GetAddress) });
        return await _dbContext.Addresses.FindAsync(addressId);
    }

    public async Task<bool?> UpdateAddress(Address address) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(UpdateAddress) });
        
        try {
            _dbContext.Update(address);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(UpdateAddress)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> DeleteAddress(string addressId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(DeleteAddress) });

        try {
            _dbContext.Remove(addressId);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(DeleteAddress)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> DeleteProfileAddress(string profileId, string addressId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(DeleteProfileAddress) });

        try {
            var profileAddressId = await _dbContext.ProfileAddresses
                .Where(e => e.ProfileId == profileId && e.AddressId == addressId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (!profileAddressId.IsString()) return default;
            
            _dbContext.Remove(profileAddressId!);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(DeleteProfileAddress)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(DeleteProfileAddress)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<ProfileAddress?> GetProfileAddress(string profileId, string addressId) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(GetProfileAddress) });

        try {
            return await _dbContext.ProfileAddresses.FirstOrDefaultAsync(e => e.ProfileId == profileId && e.AddressId == addressId);
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(GetProfileAddress)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> UpdateProfileAddress(ProfileAddress profileAddress) {
        _logger.Log(new LoggerBinding<AddressService> { Location = nameof(UpdateProfileAddress) });

        try {
            _dbContext.Update(profileAddress);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<AddressService> {
                Location = $"{nameof(UpdateProfileAddress)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}