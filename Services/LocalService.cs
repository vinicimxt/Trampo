using BD_TRAMPO.Contracts;
using BD_TRAMPO.DAO;
namespace BD_TRAMPO.Services;

public sealed class LocalService(LocalDAO locais,ServicoService servicos,EnderecoService enderecos)
{
    public List<Local> Listar(UsuarioContexto usuario) => locais.ListarPorProfissional(servicos.Profissional(usuario));
    public Local Buscar(UsuarioContexto usuario,int id)
    {
        int p=servicos.Profissional(usuario);
        var l=locais.BuscarPorId(id)??throw new FalhaOperacao(TipoFalha.NaoEncontrado,"Local não encontrado.");
        if(l.ProfissionalId!=p)throw new FalhaOperacao(TipoFalha.SemPermissao,"Local pertence a outro profissional.");
        return l;
    }
    public async Task<int> Salvar(UsuarioContexto usuario,SalvarLocalRequest dados,CancellationToken ct=default)
    {
        int p=servicos.Profissional(usuario);
        if(dados.Id<0 || (dados.Nome?.Length??0)>100)throw new FalhaOperacao(TipoFalha.Validacao,"Local inválido.");
        if(dados.Id>0)Buscar(usuario,dados.Id);
        var endereco=dados.DadosEndereco==null?null:await enderecos.PrepararLocal(dados.DadosEndereco,ct);
        string texto=endereco?.EnderecoFormatado??dados.Endereco?.Trim()??"";
        if(string.IsNullOrWhiteSpace(texto)||texto.Length>255)throw new FalhaOperacao(TipoFalha.Validacao,"Endereço inválido.");
        var local=new Local{Id=dados.Id,ProfissionalId=p,Nome=dados.Nome?.Trim()??"",Endereco=texto,DadosEndereco=endereco};
        if(dados.Id==0)return locais.Inserir(local);
        if(!locais.Atualizar(local))throw new FalhaOperacao(TipoFalha.Conflito,"Local histórico não pode ser alterado. Cadastre outro local.");
        return dados.Id;
    }
    public void Excluir(UsuarioContexto usuario,int id)
    {
        Buscar(usuario,id);
        if(!locais.Excluir(id))throw new FalhaOperacao(TipoFalha.Conflito,"Local vinculado. Cadastre outro local para novos atendimentos.");
    }
}
