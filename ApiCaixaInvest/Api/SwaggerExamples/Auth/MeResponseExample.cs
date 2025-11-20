using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

[NotMapped]
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
