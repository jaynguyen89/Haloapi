﻿using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

internal class ServiceBase: IServiceBase {

    protected readonly ILoggerService _logger;
    protected readonly HalogenDbContext _dbContext;

    internal ServiceBase(
        ILoggerService logger,
        HalogenDbContext dbContext
    ) {
        _logger = logger;
        _dbContext = dbContext;
    }
}