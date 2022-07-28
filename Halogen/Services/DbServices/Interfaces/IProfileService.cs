using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

internal interface IProfileService {

    Task<string?> InsertNewProfile(Profile newProfile);
}