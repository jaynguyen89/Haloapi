using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IAddressService {
    
    Task<AddressBookVM?> GetAddressBookByProfileId(string profileId);
    
    Task<bool?> InsertNewAddress(Address address);
    
    Task<bool?> InsertNewProfileAddress(ProfileAddress profileAddress);
    
    Task<Address?> GetAddress(string addressId);
    
    Task<bool?> UpdateAddress(Address address);
    
    Task<bool?> DeleteAddress(string addressId);
    
    Task<bool?> DeleteProfileAddress(string profileId, string addressId);
    
    Task<ProfileAddress?> GetProfileAddress(string profileId, string addressId);
    
    Task<bool?> UpdateProfileAddress(ProfileAddress profileAddress);
}