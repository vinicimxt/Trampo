using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

internal static class LocalFormRegression
{
    public static async Task Executar(string root, Action<bool, string> verificar)
    {
        async Task<(bool Valido, object? Id)> Vincular(string valor)
        {
            var contexto = DefaultModelBindingContext.CreateBindingContext(
                new ActionContext { HttpContext = new DefaultHttpContext() },
                new FormValueProvider(BindingSource.Form,
                    new FormCollection(new Dictionary<string, StringValues> { ["id"] = valor }),
                    CultureInfo.InvariantCulture),
                new EmptyModelMetadataProvider().GetMetadataForType(typeof(int)), null, "id");
            await new SimpleTypeModelBinder(typeof(int), NullLoggerFactory.Instance).BindModelAsync(contexto);
            return (contexto.ModelState.ErrorCount == 0 && contexto.Result.IsModelSet, contexto.Result.Model);
        }

        verificar(!(await Vincular("")).Valido,
            "local: id vazio reproduz a rejeicao antes de validar o CEP");
        var view = await File.ReadAllTextAsync(Path.Combine(root, "Views", "Local", "Lista.cshtml"));
        var inicial = Regex.Match(view, "id=\"localId\" value=\"([^\"]*)\"");
        verificar(inicial.Success, "local: identificador presente no formulario");
        var novo = await Vincular(inicial.Groups[1].Value);
        verificar(novo.Valido && Equals(novo.Id, 0), "local: formulario novo envia id valido para criacao");
        var resets = Regex.Matches(view, @"getElementById\('localId'\)\.value = '([^']*)'");
        verificar(resets.Count == 2, "local: verifica as duas entradas de cadastro");
        foreach (Match reset in resets)
        {
            var resultado = await Vincular(reset.Groups[1].Value);
            verificar(resultado.Valido && Equals(resultado.Id, 0), "local: reabrir cadastro restaura id zero");
        }
        var edicao = await Vincular("42");
        verificar(edicao.Valido && Equals(edicao.Id, 42), "local: edicao aceita identificador numerico");
        verificar(!(await Vincular("abc")).Valido, "local: identificador malformado continua rejeitado");
    }
}
