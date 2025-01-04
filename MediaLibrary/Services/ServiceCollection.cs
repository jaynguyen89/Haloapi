using Autofac;
using MediaLibrary.DbContexts;

namespace MediaLibrary.Services;
    
public static class ServiceCollection {

    public static void RegisterMediaLibraryServices(this ContainerBuilder builder) {
        builder.RegisterType<MediaLibraryDbContext>().SingleInstance();
        builder.RegisterType<MediaServiceFactory>().As<IMediaServiceFactory>().SingleInstance();
    }
}
