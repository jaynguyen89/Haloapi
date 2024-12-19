using Autofac;
using Halogen.Bindings.ApiBindings;
using Halogen.DbContexts;
using Halogen.Auxiliaries;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services;

public static class ServiceCollection {
    
    public static void RegisterHalogenServices(this ContainerBuilder builder) {
        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
        builder.RegisterType<HalogenDbContext>().SingleInstance();
        builder.RegisterType<LoggerService>().As<ILoggerService>().SingleInstance();
        
        builder.RegisterType<HaloServiceFactory>().As<IHaloServiceFactory>().SingleInstance();
        builder.RegisterType<HaloConfigProvider>().As<IHaloConfigProvider>().SingleInstance();
        
        builder.RegisterType<RegionalizedPhoneNumberHandler>().SingleInstance();
        builder.RegisterType<ProfileUpdateDataHandler>().SingleInstance();
        builder.RegisterType<PreferenceUpdateDataHandler>().SingleInstance();
    }
}
