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

    /// <summary>
    /// To get all addresses for the Authenticated User.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /addresses/all
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="profileId">Mapped from header.</param>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// [
    ///     {
    ///         id: string, // the ID of ProfileAddress
    ///         isForPostage: boolean,
    ///         isForDelivery: boolean,
    ///         isForReturn: boolean,
    ///         address: WesternAddress | EasternAddress,
    ///     }
    /// ]
    ///
    /// WesternAddress
    /// {
    ///     id: string,
    ///     variant: number,
    ///     buildingName?: string,
    ///     poBoxNumber?: string,
    ///     streetAddress: string,
    ///     suburb: string,
    ///     postcode: string,
    ///     division: Division,
    ///     country: Country,
    /// }
    ///
    /// EasternAddress
    /// {
    ///     id: string,
    ///     variant: number,
    ///     buildingName?: string,
    ///     poBoxNumber?: string,
    ///     streetAddress: string,
    ///     lane?: string,
    ///     group?: string,
    ///     quarter?: string,
    ///     hamlet?: string,
    ///     commute?: string,
    ///     ward?: string,
    ///     district?: string,
    ///     town?: string,
    ///     city?: string,
    ///     division: Division,
    ///     country: Country,
    /// }
    ///
    /// Division
    /// {
    ///     id: string,
    ///     countryId: string,
    ///     type: number,
    ///     name: string,
    ///     abbreviation?: string,
    /// }
    ///
    /// Country
    /// {
    ///     id: string,
    ///     name: string,
    ///     region: number,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [HttpGet("all")]
    public async Task<IActionResult> GetAddressBook([FromHeader] string profileId) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(GetAddressBook) });

        var addresses = await _addressService.GetAddressBookByProfileId(profileId);
        return addresses is null
            ? new ErrorResponse()
            : new SuccessResponse(addresses);
    }

    /// <summary>
    /// To add a new address into user's address book.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     POST /addresses/add
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    ///     Body
    ///         {
    ///             isForPostage: boolean,
    ///             isForDelivery: boolean,
    ///             isForReturn: boolean,
    ///             Address: UnifiedAddress {
    ///                 variant: number,
    ///                 buildingName?: string,
    ///                 poBoxNumber?: string,
    ///                 streetAddress: string,
    ///                 lane?: string,
    ///                 group?: string,
    ///                 quarter?: string,
    ///                 hamlet?: string,
    ///                 commute?: string,
    ///                 ward?: string,
    ///                 district?: string,
    ///                 town?: string,
    ///                 city?: string,
    ///                 suburb: string,
    ///                 postcode: string,
    ///                 divisionId: string,
    ///                 countryId: string,
    ///             },
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="profileId">Mapped from header.</param>
    /// <param name="addressData">The data of new address.</param>
    /// <response code="200">Successful request returning the ID of new address.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPost("add")]
    public async Task<IActionResult> AddAddress([FromHeader] string profileId, [FromBody] AddressData addressData) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(AddAddress) });

        var errors = addressData.VerifyAddressData();
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var address = Address.CreateNewAddress(addressData);

        await _contextService.StartTransaction();
        var addressInsertionResult = await _addressService.InsertNewAddress(address);
        if (!addressInsertionResult.HasValue || !addressInsertionResult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var profileAddress = new ProfileAddress {
            Id = StringHelpers.NewGuid(),
            ProfileId = profileId,
            AddressId = address.Id,
            IsForPostage = addressData.IsForPostage,
            IsForDelivery = addressData.IsForDelivery,
            IsForReturn = addressData.IsForReturn,
        };
        
        var profileAddressInsertionRersult = await _addressService.InsertNewProfileAddress(profileAddress);
        if (!profileAddressInsertionRersult.HasValue || !profileAddressInsertionRersult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse(address.Id);
    }

    /// <summary>
    /// To update an address in the address book.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PUT /addresses/update/{addressId}
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    ///     Body
    ///         {
    ///             isForPostage: boolean,
    ///             isForDelivery: boolean,
    ///             isForReturn: boolean,
    ///             Address: UnifiedAddress {
    ///                 variant: number,
    ///                 buildingName?: string,
    ///                 poBoxNumber?: string,
    ///                 streetAddress: string,
    ///                 lane?: string,
    ///                 group?: string,
    ///                 quarter?: string,
    ///                 hamlet?: string,
    ///                 commute?: string,
    ///                 ward?: string,
    ///                 district?: string,
    ///                 town?: string,
    ///                 city?: string,
    ///                 suburb: string,
    ///                 postcode: string,
    ///                 divisionId: string,
    ///                 countryId: string,
    ///             },
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="addressId">ID of the address to be updated.</param>
    /// <param name="addressData">The data to update address.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">BadRequest - The address data is invalid.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPut("update/{addressId}")]
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

    /// <summary>
    /// To delete an address in user's address book.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /addresses/delete/{addressId}
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="profileId">Mapped from header.</param>
    /// <param name="addressId">Mapped from route param.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpDelete("delete/{addressId}")]
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

    /// <summary>
    /// Set an address as for delivery, or postage, or return.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PATCH /addresses/set-purpose/{addressId}/{purpose}
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="profileId">Mapped from header.</param>
    /// <param name="addressId">Mapped from 1st route param.</param>
    /// <param name="purpose">Mapped from 2nd route param.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">BadRequest - The purpose is out of range.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPatch("set-purpose/{addressId}/{purpose}")]
    public async Task<IActionResult> SetAddressPurpose([FromHeader] string profileId, [FromRoute] string addressId, [FromRoute] byte purpose) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(SetAddressPurpose) });
        
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