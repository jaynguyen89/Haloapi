using Autofac;

namespace AssistantLibrary;
    
public static class ServiceCollection {

    public static void RegisterAssistantLibraryServices(this ContainerBuilder builder) {
        builder.RegisterType<AssistantServiceFactory>().As<IAssistantServiceFactory>().SingleInstance();
    }
}
