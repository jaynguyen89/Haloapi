using HelperLibrary.Shared.Logger;
using MediaLibrary.DbContexts;
using MediaLibrary.Services.Interfaces;

namespace MediaLibrary.Services.DbServices;

public sealed class AvatarService: ServiceBase, IAvatarService {

    internal AvatarService(
        ILoggerService logger,
        MediaLibraryDbContext dbContext,
        MediaRoutePath routePath
    ): base(logger, dbContext, routePath) { }
}
