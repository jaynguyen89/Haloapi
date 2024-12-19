using System.Net;
using Halogen.Attributes;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers;

[ApiController]
[Route("addresses")]
[AutoValidateAntiforgeryToken]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
[ServiceFilter(typeof(TwoFactorAuthorize))]
public sealed class AddressController: AppController {

    private enum AddressPurpose {
        Postage,
        Delivery,
        Return,
    }

    private readonly IContextService _contextService;
    private readonly IAddressService _addressService;

    public AddressController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloConfigProvider haloConfigProvider,
        IHaloServiceFactory haloServiceFactory
    ): base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(ContextService));
        _addressService = haloServiceFactory.GetService<AddressService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(AddressService));
    }
    
    [HttpGet("get-addresses")]
    public async Task<IActionResult> GetAddressBook([FromHeader] string profileId) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(GetAddressBook) });

        var addresses = await _addressService.GetAddressBookByProfileId(profileId);
        return addresses is null
            ? new ErrorResponse()
            : new SuccessResponse(addresses);
    }

    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPost("add-address")]
    public async Task<IActionResult> AddAddress([FromHeader] string profileId, [FromBody] AddressData addressData) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(AddAddress) });

        var errors = addressData.VerifyAddressData();
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var address = Address.CreateNewAddress(addressData);

        await _contextService.StartTransaction();
        var addressId = await _addressService.InsertNewAddress(address);
        if (addressId is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var profileAddress = new ProfileAddress {
            ProfileId = profileId,
            AddressId = addressId,
            IsForPostage = addressData.IsForPostage,
            IsForDelivery = addressData.IsForDelivery,
            IsForReturn = addressData.IsForReturn,
        };
        
        var profileAddressId = await _addressService.InsertNewProfileAddress(profileAddress);
        if (profileAddressId is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse(addressId);
    }

    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPut("update-address/{addressId}")]
    public async Task<IActionResult> UpdateAddress([FromRoute] string addressId, [FromBody] AddressData addressData) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(UpdateAddress) });
        
        var errors = addressData.VerifyAddressData();
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var address = await _addressService.GetAddress(addressId);
        if (address is null) return new ErrorResponse();
        
        address.UpdateAddress(addressData);
        var addressUpdated = await _addressService.UpdateAddress(address);
        return !addressUpdated.HasValue || !addressUpdated.Value ? new ErrorResponse() : new SuccessResponse();
    }

    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpDelete("delete-address/{addressId}")]
    public async Task<IActionResult> DeleteAddress([FromHeader] string profileId, [FromRoute] string addressId) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(DeleteAddress) });
        
        await _contextService.StartTransaction();
        var addressDeleted = await _addressService.DeleteAddress(addressId);
        if (!addressDeleted.HasValue || !addressDeleted.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var profileAddressDeleted = await _addressService.DeleteProfileAddress(profileId, addressId);
        if (!profileAddressDeleted.HasValue || !profileAddressDeleted.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }
        
        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }

    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPatch("toggle-purpose/{addressId}/{purpose}")]
    public async Task<IActionResult> SetAsDeliveryAddress([FromHeader] string profileId, [FromRoute] string addressId, [FromRoute] byte purpose) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(SetAsDeliveryAddress) });
        
        if (purpose >= EnumHelpers.Length<AddressPurpose>()) return new ErrorResponse(HttpStatusCode.BadRequest);
        
        var profileAddress = await _addressService.GetProfileAddress(profileId, addressId);
        if (profileAddress is null) return new ErrorResponse();
        
        switch (purpose) {
            case (byte)AddressPurpose.Postage:
                profileAddress.IsForPostage = !profileAddress.IsForPostage;
                break;
            case (byte)AddressPurpose.Delivery:
                profileAddress.IsForDelivery = !profileAddress.IsForDelivery;
                break;
            case (byte)AddressPurpose.Return:
                profileAddress.IsForReturn = !profileAddress.IsForReturn;
                break;
        }

        var profileAddressUpdated = await _addressService.UpdateProfileAddress(profileAddress);
        return !profileAddressUpdated.HasValue || !profileAddressUpdated.Value ? new ErrorResponse() : new SuccessResponse();
    }
}