﻿using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

public sealed class AddressService: DbServiceBase, IAddressService {
    
    public AddressService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
}