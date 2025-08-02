namespace OllamaClient.Models.Json
{
    public enum Role
    {
        user,
        system,
        assistant,
        tool
    }

    public enum ModelParameterKey
    {
        mirostat,
        mirostat_eta,
        mirostat_tau,
        num_ctx,
        repeat_last_n,
        repeat_penalty,
        temperature,
        seed,
        stop,
        num_predict,
        num_keep,
        top_k,
        top_p,
        min_p
    }

    public enum QuantizationType
    {
        q2_K,
        q3_K_L,
        q3_K_M,
        q3_K_S,
        q4_0,
        q4_1,
        q4_K_M,
        q4_K_S,
        q5_0,
        q5_1,
        q5_K_M,
        q5_K_S,
        q6_K,
        q8_0,
    }
}
