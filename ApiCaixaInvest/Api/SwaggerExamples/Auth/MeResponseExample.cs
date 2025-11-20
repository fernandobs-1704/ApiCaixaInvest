using Swashbuckle.AspNetCore.Filters;

public class MeResponseExample : IExamplesProvider<object>
{
    public object GetExamples()
    {
        return new
        {
            Usuario = "cliente@caixa.gov.br",
            Perfil = "Cliente"
        };
    }
}
