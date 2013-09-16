using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class Estabelecimento
    {
        public virtual long Codigo { get; set; }

        [Required(ErrorMessage = "O campo Razao Social é obrigatório"), MaxLength(60, ErrorMessage = "O campo Razao Social não pode ter mais de 60 letras"), MinLength(5, ErrorMessage = "O campo Razao Social tem que ter mais de 5 letras")]
        public virtual string RazaoSocial { get; set; }
        [Required(ErrorMessage = "O campo CNPJ é obrigatório"), MaxLength(14, ErrorMessage = "O campo CNPJ não pode ter mais de 14 letras não use traço nem pontos"), MinLength(14, ErrorMessage = "O campo CNPJ tem que ter 14 letras")]
        public virtual string CNPJ { get; set; }
        [Required(ErrorMessage = "O campo Inscricao Estadual é obrigatório"), MaxLength(9, ErrorMessage = "O campo Inscricao Estadual não pode ter mais de 9 letras não use traço nem pontos"), MinLength(9, ErrorMessage = "O campo Inscricao Estadual tem que ter 9 letras")]
        public virtual string InscricaoEstadual { get; set; }
        [DisplayFormat(DataFormatString = "{0:99999999}",ApplyFormatInEditMode = true)]
        public virtual string Telefone { get; set; }
        public virtual Endereco EnderecoEstabelecimento { get; set; }
        public virtual CadastrarUsuario UsuarioResponsavel { get; set; }
        public virtual IList<CadastrarUsuario> UsuariosFuncionarios { get; set; }
        public virtual IList<ContaCorrente> ContaCorrenteEstabelecimento { get; set; }
        public virtual IList<OperacaoCaixa> OperacoesEstabelecimento { get; set; }
        public virtual IList<Pagamento> PagamentosDoEstabelecimento { get; set; }
        public virtual IList<FechamentoCaixa> FaturamentoEstabelecimento { get; set; }
        
        
    }
}