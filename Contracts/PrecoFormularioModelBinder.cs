using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BD_TRAMPO.Contracts;

// input[type=number] envia o separador decimal como ponto, inclusive em pt-BR.
public sealed class PrecoFormularioModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valor = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valor == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valor);
        var texto = valor.FirstValue;
        if (string.IsNullOrWhiteSpace(texto))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        const NumberStyles estilo = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        if (decimal.TryParse(texto, estilo, CultureInfo.InvariantCulture, out var preco) ||
            decimal.TryParse(texto, estilo, valor.Culture, out preco))
            bindingContext.Result = ModelBindingResult.Success(preco);
        else
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Preço inválido.");

        return Task.CompletedTask;
    }
}
