$(document).ready(function () {

    $("#Loja").change(function () {


        $.get('/Escritorio/OperacaoCaixa/FormasPagamentoEstabelecimento/' + $(this).val(), function (response) {

            var Formas = $.parseJSON(response);
            var FormaPagamentoSelecionada = $("#FormaPagto");

            // clear all previous options 
            $("#FormaPagto > option").remove();

            // populate the products 

            for (i = 0; i < Formas.length; i++) {

                FormaPagamentoSelecionada.append($("<option />").val(Formas[i].Value).text(Formas[i].Text));
            }


        });


    });

});
