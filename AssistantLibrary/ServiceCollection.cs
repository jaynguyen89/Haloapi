using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using Autofac;

namespace AssistantLibrary;
    
public static class ServiceCollection {

    public static void RegisterAssistantLibraryServices(this ContainerBuilder builder) {
        builder.RegisterType<AssistantService>().As<IAssistantService>();
        builder.RegisterType<CryptoService>().As<ICryptoService>();
        builder.RegisterType<MailService>().As<IMailService>();
        builder.RegisterType<TwoFactorService>().As<ITwoFactorService>();
    }
}
