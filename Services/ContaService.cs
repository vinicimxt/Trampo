using BD_TRAMPO.Contracts;
using BD_TRAMPO.Models;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace BD_TRAMPO.Services;

public sealed class ContaService(UsuarioDAO usuarios, AutenticacaoService autenticacao)
{
    public static void ValidarPerfil(string? nome, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Length > 100 || (telefone?.Length ?? 0) > 20)
            throw new FalhaOperacao(TipoFalha.Validacao, "Dados de perfil inválidos.");
    }
    public static void ValidarCadastro(string? nome, string? email, string? senha, string? telefone)
    {
        ValidarPerfil(nome, telefone);
        if (string.IsNullOrWhiteSpace(email) || email.Length > 100 || !new EmailAddressAttribute().IsValid(email) ||
            string.IsNullOrWhiteSpace(senha) || senha.Length < 8 || senha.Length > 1024)
            throw new FalhaOperacao(TipoFalha.Validacao, "Confira o email e a senha (8 a 1024 caracteres).");
    }
    public Usuario CadastrarCliente(string nome, string email, string senha, string? telefone)
    {
        nome = nome?.Trim() ?? ""; email = email?.Trim() ?? "";
        ValidarCadastro(nome, email, senha, telefone);
        if (usuarios.EmailExiste(email)) throw new FalhaOperacao(TipoFalha.Conflito, "Email já cadastrado.");
        try {
            int id = usuarios.InserirCliente(nome, email, Seguranca.GerarHash(senha), telefone);
            return new Usuario { Id=id, Nome=nome, Email=email, Tipo="cliente", Telefone=telefone };
        } catch (SqlException ex) when (ex.Number is 2601 or 2627) {
            throw new FalhaOperacao(TipoFalha.Conflito, "Email já cadastrado.");
        }
    }
    public Usuario Perfil(UsuarioContexto contexto) => autenticacao.Atual(contexto);
    public void Atualizar(UsuarioContexto contexto, string nome, string? telefone)
    {
        Perfil(contexto);
        ValidarPerfil(nome, telefone);
        usuarios.AtualizarConta(contexto.UsuarioId, nome.Trim(), telefone ?? "");
    }
    public void ValidarSenha(int id, string? atual, string? nova, string? confirmacao)
    {
        if (string.IsNullOrWhiteSpace(atual) || atual.Length > 1024 || string.IsNullOrWhiteSpace(nova) ||
            nova.Length < 8 || nova.Length > 1024 || nova != confirmacao || !usuarios.VerificarSenha(id, atual))
            throw new FalhaOperacao(TipoFalha.Validacao, "Confira a senha atual e a confirmação. A nova senha deve ter ao menos 8 caracteres.");
    }
    public void AlterarSenha(UsuarioContexto contexto, string atual, string nova, string confirmacao)
    {
        Perfil(contexto);
        ValidarSenha(contexto.UsuarioId, atual, nova, confirmacao);
        usuarios.AtualizarSenha(contexto.UsuarioId, Seguranca.GerarHash(nova));
    }
}
