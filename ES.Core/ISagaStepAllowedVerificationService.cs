namespace ES.Core;

public interface ISagaStepAllowedVerificationService
{
    Task<bool> IsStepAllowedAsync<TSaga>(TSaga saga, string stepName)
        where TSaga : ISaga;
}
