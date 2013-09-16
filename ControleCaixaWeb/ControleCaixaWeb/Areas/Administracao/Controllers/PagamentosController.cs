using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using Telerik.Web.Mvc;

namespace ControleCaixaWeb.Areas.Administracao.Controllers
{
    [Authorize(Roles = "Administrador")]
    [HandleError(View = "Error")]
    public class PagamentosController : Controller
    {
        private IContextoDados _contextoPagamento = new ContextoDadosNH();

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Detalhes(int id)
        {
            Pagamento detalhesPagamento = _contextoPagamento.Get<Pagamento>(id);

            return View(detalhesPagamento);
        }

        public ActionResult FazerPagamento()
        {

            ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido");
            ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.Estabelecimento = new SelectList(_contextoPagamento.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial");

            return View();
        }


        [HttpPost]
        public ActionResult FazerPagamento(Pagamento pagamento)
        {
            string NomeUsuarioLogado = User.Identity.Name;


            long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();


            Pagamento novoPagamento = new Pagamento();
            novoPagamento.DataPagamento = pagamento.DataPagamento.Date;
            novoPagamento.EstabelecimentoQuePagou = _contextoPagamento.Get<Estabelecimento>(pagamento.EstabelecimentoQuePagou.Codigo);
            novoPagamento.FavorecidoPagamento = _contextoPagamento.Get<Favorecido>(pagamento.FavorecidoPagamento.Codigo);
            if (pagamento.Observacao != null)
            {
                pagamento.Observacao = pagamento.Observacao.ToUpper();
            }
            novoPagamento.Observacao = pagamento.Observacao;
            novoPagamento.Valor = pagamento.Valor;
            novoPagamento.FormaPagamento = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
            novoPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado).First();
            novoPagamento.UsuarioQueAlterouEDataEComputador = "Lançado por:" + User.Identity.Name + " Em " + DateTime.Now.Date + " No " + Environment.MachineName;
            novoPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            novoPagamento.DataHoraInsercao = DateTime.Now;
            if (ModelState.IsValid)
            {

                _contextoPagamento.Add<Pagamento>(novoPagamento);
                _contextoPagamento.SaveChanges();

                OperacaoCaixa novaOperacao = new OperacaoCaixa();

                novaOperacao.DataLancamento = pagamento.DataPagamento.Date;
                novaOperacao.Descricao = "Codigo Pagamento: " + novoPagamento.Codigo.ToString();
                novaOperacao.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(pagamento.EstabelecimentoQuePagou.Codigo);
                novaOperacao.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                novaOperacao.Observacao = "PAGAMENTO RETIRADA DO CAIXA, DETALHES: " + pagamento.Observacao;
                novaOperacao.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).First();
                novaOperacao.Valor = pagamento.Valor * -1;
                novaOperacao.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                novaOperacao.DataHoraInsercao = DateTime.Now;
                novaOperacao.TipoOperacao = EnumTipoOperacao.Pagamento;
                _contextoPagamento.Add<OperacaoCaixa>(novaOperacao);
                _contextoPagamento.SaveChanges();

                return RedirectToAction("Sucesso", "Home");
            }


            ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido", pagamento.FavorecidoPagamento);
            ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", pagamento.FormaPagamento);
            ViewBag.Estabelecimento = new SelectList(_contextoPagamento.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", pagamento.EstabelecimentoQuePagou);
            return View();
        }



        public ActionResult AlterarPagamento(int id)
        {
            Pagamento pagamentoParaAlterar = _contextoPagamento.Get<Pagamento>(id);
            ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido");
            ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.Estabelecimento = new SelectList(_contextoPagamento.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial");
            return View(pagamentoParaAlterar);
        }



        [HttpPost]
        public ActionResult AlterarPagamento(Pagamento pagamento)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                         .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();

            Pagamento pagamentoAlterado = _contextoPagamento.Get<Pagamento>(pagamento.Codigo);

            pagamentoAlterado.DataPagamento = pagamento.DataPagamento.Date;
            pagamentoAlterado.FavorecidoPagamento = _contextoPagamento.Get<Favorecido>(pagamento.FavorecidoPagamento.Codigo);
            pagamentoAlterado.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).First();
            pagamentoAlterado.Valor = pagamento.Valor;
            pagamentoAlterado.FormaPagamento = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
            pagamentoAlterado.EstabelecimentoQuePagou = _contextoPagamento.Get<Estabelecimento>(pagamento.EstabelecimentoQuePagou.Codigo);
            pagamentoAlterado.UsuarioQueAlterouEDataEComputador = "Alterado por:" + User.Identity.Name + "Em " + DateTime.Now.Date + "No " + Environment.MachineName;
            if (pagamento.Observacao != null)
            {
                pagamento.Observacao = pagamento.Observacao.ToUpper();
            }
            pagamentoAlterado.Observacao = pagamento.Observacao;
            pagamentoAlterado.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            pagamentoAlterado.DataHoraInsercao = DateTime.Now;
            _contextoPagamento.SaveChanges();

            OperacaoCaixa operacaoParaAlterar = _contextoPagamento.GetAll<OperacaoCaixa>().Where(x => x.Descricao == pagamento.Codigo.ToString()).First();

            operacaoParaAlterar.DataLancamento = pagamento.DataPagamento.Date;
            operacaoParaAlterar.Descricao = pagamento.Codigo.ToString();
            operacaoParaAlterar.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(pagamento.EstabelecimentoQuePagou.Codigo);
            operacaoParaAlterar.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
            operacaoParaAlterar.Observacao = pagamento.Observacao;
            operacaoParaAlterar.Valor = pagamento.Valor * -1;
            operacaoParaAlterar.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).First();
            operacaoParaAlterar.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            operacaoParaAlterar.DataHoraInsercao = DateTime.Now;
            _contextoPagamento.SaveChanges();


            return RedirectToAction("Sucesso", "Home");
        }


        public ActionResult ExcluirPagamento(int id)
        {
            Pagamento pagamentoParaExcluir = _contextoPagamento.Get<Pagamento>(id);

            return View(pagamentoParaExcluir);
        }



        [HttpPost]
        public ActionResult ExcluirPagamento(Pagamento pagamento)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                         .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();

            long codigoFormaPagamento = (from c in _contextoPagamento.GetAll<Pagamento>()
                                         .Where(x => x.Codigo == pagamento.Codigo)
                                         select c.FormaPagamento.Codigo).First();



            Pagamento pagamentoExcluido = _contextoPagamento.Get<Pagamento>(pagamento.Codigo);


            OperacaoCaixa operacaoParaEstornar = new OperacaoCaixa();

            operacaoParaEstornar.DataLancamento = pagamento.DataPagamento.Date;
            operacaoParaEstornar.Descricao = pagamento.Codigo.ToString();
            operacaoParaEstornar.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
            operacaoParaEstornar.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(codigoFormaPagamento);
            operacaoParaEstornar.Observacao = "Estorno de Pagamento";
            operacaoParaEstornar.Valor = pagamentoExcluido.Valor;
            operacaoParaEstornar.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).First();

            _contextoPagamento.Add<OperacaoCaixa>(operacaoParaEstornar);
            _contextoPagamento.SaveChanges();
            _contextoPagamento.Delete<Pagamento>(pagamentoExcluido);
            _contextoPagamento.SaveChanges();

            return RedirectToAction("Sucesso", "Home");
        }


        public ActionResult PagamentoFavorecido(int id)
        {
            IList<Pagamento> pagamentosDesteFavorecido = null;
            pagamentosDesteFavorecido = _contextoPagamento.GetAll<Pagamento>().Where(x => x.FavorecidoPagamento.Codigo == id).ToList();

            return View(pagamentosDesteFavorecido);
        }

        public ActionResult ListaPagamentos()
        {
            return View(_contextoPagamento.GetAll<Pagamento>().OrderByDescending(x => x.DataPagamento).ToList());
        }

        public ActionResult PagamentoPorData(DateTime? data)
        {
            IList<Pagamento> listaPorData = null;
            listaPorData = _contextoPagamento.GetAll<Pagamento>()
                           .Where(x => x.DataPagamento.Date == data).ToList();
            return View(listaPorData);
        }

    }
}
