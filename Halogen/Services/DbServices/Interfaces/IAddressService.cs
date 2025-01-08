using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IAddressService {
    
    /// <summary>
    /// To get all the address associated to a Profile.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <returns>AddressBookVM?</returns>
    Task<AddressBookVM?> GetAddressBookByProfileId(string profileId);
    
    /// <summary>
    /// To insert an Address entity.
    /// </summary>
    /// <param name="address">DbModels.Address</param>
    /// <returns>bool?</returns>
    Task<bool?> InsertNewAddress(Address address);
    
    /// <summary>
    /// To insert a ProfileAddress entity.
    /// </summary>
    /// <param name="profileAddress">ProfileAddress</param>
    /// <returns>bool?</returns>
    Task<bool?> InsertNewProfileAddress(ProfileAddress profileAddress);
    
    /// <summary>
    /// To get an Address entity by addressId.
    /// </summary>
    /// <param name="addressId">string</param>
    /// <returns>DbModels.Address?</returns>
    Task<Address?> GetAddress(string addressId);
    
    /// <summary>
    /// To update an Address entity.
    /// </summary>
    /// <param name="address">DbModels.Address</param>
    /// <returns>bool?</returns>
    Task<bool?> UpdateAddress(Address address);
    
    /// <summary>
    /// To delete an Address entity.
    /// </summary>
    /// <param name="addressId">string</param>
    /// <returns>bool?</returns>
    Task<bool?> DeleteAddress(string addressId);
    
    /// <summary>
    /// To delete a ProfileAddress entity.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <param name="addressId">string</param>
    /// <returns>bool?</returns>
    Task<bool?> DeleteProfileAddress(string profileId, string addressId);
    
    /// <summary>
    /// To get a profile Address entity using composite keys.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <param name="addressId">string</param>
    /// <returns></returns>
    Task<ProfileAddress?> GetProfileAddress(string profileId, string addressId);
    
    /// <summary>
    /// To update a ProfileAddress entity.
    /// </summary>
    /// <param name="profileAddress">string</param>
    /// <returns>bool?</returns>
    Task<bool?> UpdateProfileAddress(ProfileAddress profileAddress);
}