using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using Telerik.Web.Mvc;

namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
    [Authorize(Roles="Escritorio")]
	[HandleError(View = "Error")]
    public class RelatorioController : Controller
    {
        private IContextoDados _contextoRelatorios = new ContextoDadosNH( );

        public ActionResult Index()
        {
            return View();
        }

        
      

        [HandleError(View = "Error")]
        public ActionResult TodosLancamentos()
        {
            string nomeUsuario = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoRelatorios.GetAll<CadastrarUsuario>( )
                                        .Where(x => x.Nome == nomeUsuario)
                                          select c.EstabelecimentoTrabalho.Codigo).First( );

            IList<OperacaoCaixa> listaPorData = null;

            listaPorData = _contextoRelatorios.GetAll<OperacaoCaixa>( )
                .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento).OrderByDescending(X => X.DataLancamento).ToList();

            ViewBag.Saldo = (from c in _contextoRelatorios.GetAll<OperacaoCaixa>( )
                    .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento)
                                    select c.Valor).Sum( );
           
            return View(listaPorData);
        }

        [GridAction]
        public ActionResult _TodosLancamentos( )
        {
            string nomeUsuario = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoRelatorios.GetAll<CadastrarUsuario>( )
                                        .Where(x => x.Nome == nomeUsuario)
                                          select c.EstabelecimentoTrabalho.Codigo).First( );

            var listaOperacaocaixa = (from c in _contextoRelatorios.GetAll<OperacaoCaixa>( )
                                      join u in _contextoRelatorios.GetAll<CadastrarUsuario>( )
                                      on c.UsuarioQueLancou.Codigo equals u.Codigo
                                      join f in _contextoRelatorios.GetAll<FormaPagamentoEstabelecimento>()
                                      on c.FormaPagamentoUtilizada.Codigo equals f.Codigo
                                      where c.EstabelecimentoOperacao.Codigo == codigoEstabelecimento
                                      select new {c.DataLancamento, c.Descricao,f.NomeTipoFormaPagamento, c.Valor, u.Nome }).ToList( );


            return View(new GridModel(listaOperacaocaixa));
        }

        
        public ActionResult PorFormaPagamento(string forma, DateTime? id)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoRelatorios.GetAll<CadastrarUsuario>( )
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First( );

            IList<OperacaoCaixa> listaPorForma = null;

            listaPorForma = _contextoRelatorios.GetAll<OperacaoCaixa>( )
                .Where(x => x.DataLancamento == id && x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.FormaPagamentoUtilizada.NomeTipoFormaPagamento == forma).OrderByDescending(x => x.DataLancamento).ThenByDescending(x => x.UsuarioQueLancou.Nome).ToList( );
         
            return View(listaPorForma);
        }





        public ActionResult VerDataOperacao(DateTime? id)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoRelatorios.GetAll<CadastrarUsuario>( )
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First( );

            IList<OperacaoCaixa> listaPorData = null;

            listaPorData = _contextoRelatorios.GetAll<OperacaoCaixa>( )
                .Where(x => x.DataLancamento == id && x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento).OrderByDescending(x => x.DataLancamento).ToList( );

            //ViewBag.ValorSangria = (from c in _contextoRelatorios.GetAll<OperacaoCaixa>( )
            //        .Where(x => x.DataLancamento == dataSelecionada && x.UsuarioQueLancou.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento)
            //                        select c.Valor).Sum( );
           
            return View(listaPorData );
        }

        public ActionResult PorUsuario(string usuario, DateTime? data)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            IList<OperacaoCaixa> listaPorUsuario = null;

            listaPorUsuario = _contextoRelatorios.GetAll<OperacaoCaixa>( )
                .Where(x => x.UsuarioQueLancou.Nome == usuario && x.DataLancamento == data).OrderByDescending(x => x.DataLancamento).ToList( );

            return View(listaPorUsuario);
            
        }
      
      
    }
}
