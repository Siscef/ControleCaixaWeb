using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ControleCaixaWeb.Models
{
  public  class Usuario
    {
      public virtual long Codigo { get; set; }

      [Required(ErrorMessage="O Nome do Usuário é Obrigatório."),MaxLength(20,ErrorMessage="O Nome não pode ter mais de 20 caracteres"),MinLength(3,ErrorMessage="O Campo Nome Requer Pelo Menos Três Caracteres")]
      public virtual string Nome { get; set; }
       [DataType(DataType.Password)]
      [Required(ErrorMessage = "A Senha do Usuário é Obrigatória."), MinLength(6, ErrorMessage = "O Campo Senha Requer no Mínimo 6 Caracteres"), MaxLength(25, ErrorMessage = "O Campo Senha Requer no Máximo 12 Caracteres")]
      public virtual string Senha { get; set; }
      public virtual bool Lembrar { get; set; }

    }

  public class CadastrarUsuario
  {
      public virtual long Codigo { get; set; }

      [Required(ErrorMessage = "Nome Não Pode Ser Vazio")]
      public virtual string Nome { get; set; }

      [Required(ErrorMessage = "Email Não Pode Ser Vazio")]
      [DataType(DataType.EmailAddress)]
      public virtual string Email { get; set; }

      [Required(ErrorMessage="Senha Não Pode Ser Vazio"),MinLength(6,ErrorMessage="A senha deve ter no mínimo 6 caracteres")]
      
      [DataType(DataType.Password)]
      public virtual string Senha { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirme a Senha")]
      [Compare("Senha", ErrorMessage = "As Senhas Não Conferem.")]
      [Required(ErrorMessage="O campo compare a senha é obrigatório"),StringLength(25)]
      public virtual string ConfirmeSenha { get; set; }

      public virtual string Telefone { get; set; }

      [Required(ErrorMessage="O endereco é obrigatório")]
      public virtual Endereco EnderecoUsuario { get; set; }      
      public virtual Estabelecimento EstabelecimentoTrabalho { get; set; }      
      public virtual Papel NomeFuncao { get; set; }
      public virtual IList<OperacaoCaixa> OpereacoesDoUsuario { get; set; }
      public virtual IList<FechamentoCaixa> FechamentoDeCaixa { get; set; }
      public virtual bool Privilegiado { get; set; }

  }
  public class TrocarSenha
  {
      public virtual long Codigo { get; set; }
      [Required]
      [DataType(DataType.Password)]
      [Display(Name = "SenhaAtual")]
      public virtual string SenhaAtual { get; set; }

      [Required]
      [StringLength(100, ErrorMessage = "A senha tem que ter no mínimo 6 letras.", MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "Nova Senha")]
      public virtual string NovaSenha { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirma Nova Senha")]
      [Compare("NovaSenha", ErrorMessage = "A nova senha e a senha fornecida não conferem.")]
      public virtual string ConfirmaSenha { get; set; }
  }
}
