using BD_TRAMPO.Api;
using BD_TRAMPO;
using BD_TRAMPO.DAO;
using BD_TRAMPO.Services;


var builder = WebApplication.CreateBuilder(args);
Conexao.Configurar(builder.Configuration);
builder.Services.AddHealthChecks();

//  REGISTRAR SERVIÇOS
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(); 

builder.Services.AddScoped<ContaService>();
builder.Services.AddScoped<ClienteAtendimentoService>();
builder.Services.AddScoped<NotificacaoDAO>();
builder.Services.AddScoped<SuporteDAO>();
builder.Services.AddScoped<AgendamentoService>();
builder.Services.AddScoped<ServicoService>();
builder.Services.AddScoped<AgendamentoDAO>();
builder.Services.AddScoped<ServicoDAO>();
builder.Services.AddScoped<ClienteDAO>();
builder.Services.AddScoped<ProfissionalDAO>();
builder.Services.AddScoped<DisponibilidadeDAO>();
builder.Services.AddScoped<LocalDAO>();
builder.Services.AddScoped<SubcategoriaDAO>();
builder.Services.AddScoped<CategoriaDAO>();
builder.Services.AddScoped<AvaliacaoDAO>();
builder.Services.AddScoped<UsuarioDAO>();
builder.AdicionarApi();
builder.AdicionarEnderecos();
var app = builder.Build();
app.UseMiddleware<ErrosApiMiddleware>();

//  PIPELINE

if (!app.Environment.IsDevelopment())
{
    app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), branch => branch.UseExceptionHandler("/Home/Error"));
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();  

app.UseRouting();

app.UseSession(); 

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "TRAMPO API v1"));
}
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();