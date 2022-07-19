﻿using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

internal sealed class RoleService: ServiceBase, IRoleService {
    
    internal RoleService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
    
    
}