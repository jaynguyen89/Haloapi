using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers;

[ApiController]
[Route("addresses")]
[AutoValidateAntiforgeryToken]
public sealed class AddressController: AppController {

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
    public async Task<IActionResult> GetAddresses([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(GetAddresses) });
    }

    [HttpPost("add-address")]
    public async Task<IActionResult> AddAddress([FromHeader] string accountId, [FromBody] AddressData addressData) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(AddAddress) });
    }

    [HttpPut("update-address")]
    public async Task<IActionResult> UpdateAddress([FromHeader] string accountId, [FromBody] AddressData addressData) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(UpdateAddress) });
    }

    [HttpDelete("delete-address/{addressId}")]
    public async Task<IActionResult> DeleteAddress([FromHeader] string accountId, [FromRoute] string addressId) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(DeleteAddress) });
    }

    [HttpPatch("set-as-delivery-address/{addressId}")]
    public async Task<IActionResult> SetAsDeliveryAddress([FromHeader] string accountId, [FromRoute] string addressId) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(SetAsDeliveryAddress) });
    }

    [HttpPatch("set-as-shipping-address/{addressId}")]
    public async Task<IActionResult> SetAsShippingAddress([FromHeader] string accountId, [FromRoute] string addressId) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(SetAsShippingAddress) });
    }
    
    [HttpPatch("set-as-returning-address/{addressId}")]
    public async Task<IActionResult> SetAsReturningAddress([FromHeader] string accountId, [FromRoute] string addressId) {
        _logger.Log(new LoggerBinding<AddressController> { Location = nameof(SetAsReturningAddress) });
    }
}