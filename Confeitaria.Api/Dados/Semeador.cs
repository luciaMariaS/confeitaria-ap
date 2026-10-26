using Confeitaria.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Confeitaria.Api.Dados;

public static class Semeador
{
    public static async Task SemearAsync(AppDbContext db, IConfiguration config)
    {
        if (!await db.Usuarios.AnyAsync())
        {
            var senhaAdmin = config["Seed:AdminSenha"];
            if (string.IsNullOrWhiteSpace(senhaAdmin))
                throw new InvalidOperationException("Configure a variável de ambiente Seed__AdminSenha.");

            db.Usuarios.AddRange(
                new Usuario("Administradora", "admin@exemplo.com", BCrypt.Net.BCrypt.HashPassword(senhaAdmin), Usuario.PapelAdmin),
                new Usuario("Ana Souza", "ana@exemplo.com", BCrypt.Net.BCrypt.HashPassword("Cliente@2026"), Usuario.PapelCliente),
                new Usuario("Bruno Lima", "bruno@exemplo.com", BCrypt.Net.BCrypt.HashPassword("Cliente@2026"), Usuario.PapelCliente));
        }

        if (!await db.Produtos.AnyAsync())
        {
            db.Produtos.AddRange(
                new Produto("Bolo de Cenoura com Chocolate", "Massa fofinha com cobertura cremosa de chocolate. Serve 12 fatias.", 68m, "Bolo"),
                new Produto("Bolo Red Velvet", "Massa aveludada com recheio de cream cheese. Serve 12 fatias.", 98m, "Bolo"),
                new Produto("Naked Cake de Frutas Vermelhas", "Camadas de massa branca, chantilly e frutas vermelhas. Serve 15 fatias.", 135m, "Bolo"),
                new Produto("Torta de Limão", "Base crocante, creme de limão e merengue maçaricado. 8 fatias.", 72m, "Torta"),
                new Produto("Torta Holandesa", "Biscoito, creme branco e cobertura de chocolate meio amargo. 8 fatias.", 85m, "Torta"),
                new Produto("Brigadeiro Gourmet (cento)", "Chocolate belga, enrolado na hora. Pedido mínimo de um cento.", 180m, "Doce"),
                new Produto("Beijinho de Coco (cento)", "Coco fresco e leite condensado, finalizado com cravo.", 160m, "Doce"),
                new Produto("Caixa de Brownies (6 unidades)", "Brownie denso, com nozes e casquinha crocante.", 54m, "Doce"),
                new Produto("Cookies Recheados (caixa com 8)", "Cookies macios recheados com chocolate ao leite.", 62m, "Cookie"),
                new Produto("Cupcakes Decorados (caixa com 6)", "Sabores variados com cobertura decorada à mão.", 66m, "Cupcake"));
        }

        await db.SaveChangesAsync();
    }
}