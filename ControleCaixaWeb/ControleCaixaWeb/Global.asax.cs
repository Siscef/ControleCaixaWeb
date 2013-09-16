using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using ControleCaixaWeb.Models.Context;
using ControleCaixaWeb.Models;


namespace ControleCaixaWeb
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute( ));
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "ControleCaixaWeb.Controllers" }// Parameter defaults
            );

        }
        protected void Application_Start( )
        {
            AreaRegistration.RegisterAllAreas( );

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            if (System.Web.Security.Roles.GetAllRoles( ).Length == 0)
            {
                using (IContextoDados ctx = new ContextoDadosNH( ))
                {
                    ctx.setup( );

                    System.Web.Security.Roles.CreateRole("OperadorCaixa");
                    System.Web.Security.Roles.CreateRole("Escritorio");
                    System.Web.Security.Roles.CreateRole("Administrador");


                    MembershipCreateStatus status;
                    Membership.CreateUser("Desenvolvedor", "sudoademi88324192...", "ademivieria@gmail.com", null, null, true, out status);

                    if (status == MembershipCreateStatus.Success)
                    {
                        Papel papelAdministrador = new Papel( );
                        papelAdministrador.NomePapel = "Administrador";
                        ctx.Add<Papel>(papelAdministrador);
                        ctx.SaveChanges( );

                        Papel papelEscritorio = new Papel( );
                        papelEscritorio.NomePapel = "Escritorio";
                        ctx.Add<Papel>(papelEscritorio);
                        ctx.SaveChanges( );

                        Papel papelOperadorCaixa = new Papel( );
                        papelOperadorCaixa.NomePapel = "OperadorCaixa";
                        ctx.Add<Papel>(papelOperadorCaixa);
                        ctx.SaveChanges( );

                        Endereco endereco = new Endereco( );
                        endereco.Bairro = "Jaguaribe";
                        endereco.NomeRua = "Professor fco de souza rangel,720";
                        endereco.CEP = "58015730";
                        endereco.Cidade = "Joao Pessoa ";
                        endereco.Estado = "Paraiba";
                        endereco.Complemento = "Proximo Ao Cefet";
                        ctx.Add<Endereco>(endereco);
                        ctx.SaveChanges( );

                        CadastrarUsuario usuario = new CadastrarUsuario( );
                        usuario.Nome = "Desenvolvedor";
                        usuario.Senha = "sudoademi88324192...";
                        usuario.ConfirmeSenha = "sudoademi88324192...";
                        usuario.Email = "ademivieria@gmail.com";
                        usuario.Telefone = "88250896";
                        usuario.EnderecoUsuario = ctx.Get<Endereco>(1);
                        usuario.NomeFuncao = ctx.Get<Papel>(1);
                        ctx.Add<CadastrarUsuario>(usuario);
                        ctx.SaveChanges( );





                        Estabelecimento estabelecimento = new Estabelecimento( );
                        estabelecimento.CNPJ = "02745276000234";
                        estabelecimento.RazaoSocial = "Estabelecimento Treinamento";
                        estabelecimento.InscricaoEstadual = "123459876";
                        estabelecimento.Telefone = "88324192";
                        estabelecimento.UsuarioResponsavel = (from c in ctx.GetAll<CadastrarUsuario>( ).Where(x => x.Codigo == 1)
                                                              select c).First( );
                        estabelecimento.EnderecoEstabelecimento = (from c in ctx.GetAll<Endereco>( ).Where(x => x.Codigo == 1)
                                                                   select c).First( );
                        ctx.Add<Estabelecimento>(estabelecimento);
                        ctx.SaveChanges( );


                        Usuario novoUsuario = new Usuario( );
                        novoUsuario.Nome = "Desenvolvedor";
                        novoUsuario.Senha = "sudoademi88324192...";
                        novoUsuario.Lembrar = false;
                        ctx.Add<Usuario>(novoUsuario);
                        ctx.SaveChanges( );

                        Roles.AddUserToRole("Desenvolvedor", "Administrador");

                        ctx.Dispose( );


                    }


                }


            }


        }
    }
}