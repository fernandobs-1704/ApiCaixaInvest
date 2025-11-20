using Swashbuckle.AspNetCore.Filters;

public class MeResponseExample : IExamplesProvider<object>
{
    public object GetExamples()
    {
        return new
        {
            Usuario = "caixaverso@caixa.gov.br",
            Perfil = "Usuario"
        };
    }
}
