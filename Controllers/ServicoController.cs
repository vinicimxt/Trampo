using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers;

[Perfil("profissional")]
public class ServicoController(ServicoService service) : BaseController
{
    private UsuarioContexto Usuario => new(UsuarioAtualId, HttpContext.Session.GetString("UsuarioTipo") ?? "");

    public IActionResult Criar() => ExecutarAplicacao(() => {
        var formulario = service.PrepararCriacao(Usuario);
        ViewBag.Locais = formulario.Locais;
        ViewBag.Categorias = formulario.Categorias;
        ViewBag.Subcategorias = new List<Subcategoria>();
        ViewBag.TotalServicos = formulario.Total;
        ViewBag.PedidosPendentes = formulario.Pendentes;
        return View();
    });

    [AllowAnonymous]
    public JsonResult GetSubcategorias(int categoriaId) => Json(service.Subcategorias(categoriaId));

    [HttpPost]
    public IActionResult Salvar(SalvarServicoRequest dados) => ExecutarAplicacao(() => {
        if (!ModelState.IsValid) return BadRequest("Dados do serviço inválidos.");
        service.Criar(Usuario, dados);
        TempData["Sucesso"] = "Serviço criado com sucesso ✔";
        return RedirectToAction("MeusServicos", "Profissional");
    });

    public IActionResult Editar(int id) => ExecutarAplicacao(() => {
        service.BuscarProprio(Usuario, id);
        return RedirectToAction("MeusServicos", "Profissional");
    });

    [HttpPost]
    public IActionResult Editar(SalvarServicoRequest dados) => ExecutarAplicacao(() => {
        if (!ModelState.IsValid) return BadRequest("Dados do serviço inválidos.");
        service.Editar(Usuario, dados);
        TempData["Sucesso"] = "Serviço atualizado com sucesso ✏️";
        return RedirectToAction("MeusServicos", "Profissional");
    });

    [HttpPost]
    public IActionResult Excluir(int id) => ExecutarAplicacao(() => {
        service.Remover(Usuario, id);
        TempData["Sucesso"] = "Serviço removido da oferta. O histórico existente foi preservado.";
        return RedirectToAction("MeusServicos", "Profissional");
    });

    [HttpPost]
    public IActionResult Desativar(int id) => ExecutarAplicacao(() => {
        service.Desativar(Usuario, id);
        TempData["Sucesso"] = "Serviço removido da oferta. O histórico existente foi preservado.";
        return RedirectToAction("MeusServicos", "Profissional");
    });

    [AllowAnonymous]
    public JsonResult Subcategorias(int categoriaId) => GetSubcategorias(categoriaId);

    // Alias antigo usa a tela de criação existente.
    public IActionResult Novo() => RedirectToAction("Criar");
}