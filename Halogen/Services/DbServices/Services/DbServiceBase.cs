﻿using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

public class DbServiceBase: ServiceBase, IDbServiceBase {

    protected readonly HalogenDbContext _dbContext;

    protected DbServiceBase(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger) {
        _dbContext = dbContext;
    }
}