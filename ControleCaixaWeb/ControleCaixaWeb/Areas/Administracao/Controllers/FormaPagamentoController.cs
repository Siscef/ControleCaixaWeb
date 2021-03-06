﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using Telerik.Web.Mvc;
using System.Web.UI.WebControls;

namespace ControleCaixaWeb.Areas.Administracao.Controllers
{
    [Authorize(Roles = "Administrador")]
	[HandleError(View = "Error")]
    public class FormaPagamentoController : Controller
    {
        private IContextoDados _contextoFormaPagamento = new ContextoDadosNH( );

       
        public ActionResult Index()
        {
            return View();
        }

     

        public ActionResult Detalhes(int id)
        {
            FormaPagamentoEstabelecimento detalhesFormaPagamento = _contextoFormaPagamento.Get<FormaPagamentoEstabelecimento>(id);

            return View(detalhesFormaPagamento);
        }

        [GridAction]
        public ActionResult ListarFormaPagamento( )
        {
            IList<FormaPagamentoEstabelecimento> listaFormaPagamento = null;
            listaFormaPagamento = _contextoFormaPagamento.GetAll<FormaPagamentoEstabelecimento>( ).ToList();
            return View(listaFormaPagamento);
        }

       

        public ActionResult CadastrarFormaPagamento()
        {
            ViewBag.ContaCorrenteForma = new SelectList(_contextoFormaPagamento.GetAll<ContaCorrente>( ), "Codigo", "Numero");
           
            return View();
        } 

       

        [HttpPost]
        public ActionResult CadastrarFormaPagamento(FormaPagamentoEstabelecimento formaPagamento)
        {
           
            ModelState["ContaCorrenteFormaPagamento.Banco"].Errors.Clear( );
            ModelState["ContaCorrenteFormaPagamento.Agencia"].Errors.Clear( );
            ModelState["ContaCorrenteFormaPagamento.Numero"].Errors.Clear( );

            if(ModelState.IsValid)
            {
                formaPagamento.ContaCorrenteFormaPagamento = _contextoFormaPagamento.Get<ContaCorrente>(formaPagamento.ContaCorrenteFormaPagamento.Codigo);
               
                _contextoFormaPagamento.Add<FormaPagamentoEstabelecimento>(formaPagamento);
                _contextoFormaPagamento.SaveChanges( );
                return RedirectToAction("Sucesso", "Home");
            }
            ViewBag.ContaCorrenteForma = new SelectList(_contextoFormaPagamento.GetAll<ContaCorrente>( ), "Codigo", "Numero",formaPagamento.ContaCorrenteFormaPagamento);
           
            return View( );
        }
        
      
 
        public ActionResult AlterarFormaPagamento(int id)
        {
            FormaPagamentoEstabelecimento formaPagamentoParaAlterar = _contextoFormaPagamento.Get<FormaPagamentoEstabelecimento>(id);
            ViewBag.ContaCorrenteForma = new SelectList(_contextoFormaPagamento.GetAll<ContaCorrente>( ), "Codigo", "Numero");
                    
            return View(formaPagamentoParaAlterar);
        }

       

        [HttpPost]
        public ActionResult AlterarFormaPagamento(FormaPagamentoEstabelecimento formaPagamento)
        {
            FormaPagamentoEstabelecimento formaPagamentoAlterada = _contextoFormaPagamento.Get<FormaPagamentoEstabelecimento>(formaPagamento.Codigo);
            formaPagamentoAlterada.ContaCorrenteFormaPagamento = _contextoFormaPagamento.Get<ContaCorrente>(formaPagamento.ContaCorrenteFormaPagamento.Codigo);
            formaPagamentoAlterada.NomeTipoFormaPagamento = formaPagamento.NomeTipoFormaPagamento;
            formaPagamentoAlterada.TaxaFormaPagamento = formaPagamento.TaxaFormaPagamento;
			formaPagamentoAlterada.DespejoAutomatico = formaPagamento.DespejoAutomatico;
			
            
            _contextoFormaPagamento.SaveChanges( );

            return RedirectToAction( "Sucesso","Home");
        }

     
 
        public ActionResult ExcluirFormaPagamento(int id)
        {
            FormaPagamentoEstabelecimento formaParaExcluir = _contextoFormaPagamento.Get<FormaPagamentoEstabelecimento>(id);
            return View(formaParaExcluir);
        }

        

        [HttpPost]
        public ActionResult ExcluirFormaPagamento(FormaPagamentoEstabelecimento formaPagamento)
        {
            FormaPagamentoEstabelecimento formaPagamentoExcluida = _contextoFormaPagamento.Get<FormaPagamentoEstabelecimento>(formaPagamento.Codigo);
            _contextoFormaPagamento.Delete<FormaPagamentoEstabelecimento>(formaPagamentoExcluida);
            _contextoFormaPagamento.SaveChanges( );
            return RedirectToAction("Sucesso","Home" );
        }
    }
}
