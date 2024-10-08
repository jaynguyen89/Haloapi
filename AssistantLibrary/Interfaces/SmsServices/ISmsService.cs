﻿using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces.IServiceFactory; 

public interface ISmsService {
    
    Task<string[]?> SendSingleSms(SingleSmsBinding binding);

    Task<string[]?> SendMultipleSms(MultipleSmsBinding bindings);
}