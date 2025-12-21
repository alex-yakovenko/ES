using ES.Core;

namespace ES.Test.EventStorage;

public class SagaStepAllowedVerificationService : ISagaStepAllowedVerificationService
{
    public Task<bool> IsStepAllowedAsync<TSaga>(TSaga saga, string stepName) where TSaga : ISaga
    {
        return Task.FromResult(true);
    }
}
