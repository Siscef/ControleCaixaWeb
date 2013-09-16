    function submitenter(myfield, e) {
        var keycode;
        if (window.event) keycode = window.event.keyCode;
        else if (e) keycode = e.which;
        else return true;

        if (keycode == 13) {
            res();
       
            return false;
        }
        else
            return true;
    }

    document.onkeyup = teclaLiberada;
    document.captureEvents(Event.KEYUP);

    function validar() {
        var numero = document.getElementById("campoValorOperacaoCaixa").value.replace(",", ".");
        var numeroFormatado = numero.replace(",", ".");
        y = (document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value = numeroFormatado);
        return false;
    }
    function teclaLiberada(e) {
        var keycode;
        keycode = e.which;
        if (keycode == 110) {
            var valor = document.getElementById("campoValorOperacaoCaixa").value;
            document.getElementById("campoValorOperacaoCaixa").value = valor.replace(",", ".");      

        }
   

   
    }

    function submeter(e) {
        var keycode;
        keycode = e.which;
        if (keycode == 13) {
                   
            res();
            return false;

        }
    }
    
    document.onkeyup = submeter;
    document.captureEvents(Event.KEYUP);

    function noenter() {
        res();
        return (window.event && window.event.keyCode == 13);
        console.log("tecla: " + window.event.KeyCode);

    }
    function res() {

        var exp = /^(-|\+)? *[0-9]+(\.[0-9]+)?( *(-|\+|\*|\/) *[0-9]+(\.[0-9]+)?)*$/;

        x = document.getElementById("campoValorOperacaoCaixa").value = eval(document.getElementById("campoValorOperacaoCaixa").value);
        if (x == "") {
            alert("O valor informado é inválido!");

        }

        if (exp.test(x) == false) {
            alert("valor informado é inválido, substitua , por . ", "atenção!");

        }
        else {
            var x1 = x + '';
            x2 = x1.replace(".",",");

            document.getElementById("campoValorOperacaoCaixa1").value = document.getElementById("campoValorOperacaoCaixa").value = x2;

        }

    }
    function raiz() {
        var exp = /^(-|\+)? *[0-9]+(\.[0-9]+)?( *(-|\+|\*|\/) *[0-9]+(\.[0-9]+)?)*$/;

        x = Math.sqrt(document.getElementById("campoValorOperacaoCaixa").value);
   
        if (x == "") {
            alert("O valor informado é inválido!");

        }
       
        if (exp.test(x) == false) {
            alert("valor informado é inválido, substitua , por . ", "atenção!");

        }
        else {
            x = Math.sqrt(document.getElementById("campoValorOperacaoCaixa").value);
       
            y = (document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value = x);

        }
    }
    function soma() {

        var exp = /^(-|\+)? *[0-9]+(\.[0-9]+)?( *(-|\+|\*|\/) *[0-9]+(\.[0-9]+)?)*$/;

        var x = Number((document.getElementById("campoValorOperacaoCaixa").value));
        if (x == "") {
            alert("O valor informado é inválido!");

        }

        if (exp.test(x) == false) {
            alert("valor informado é inválido, substitua , por . ");

        }
        else {


            var k = (document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value + "+");

            var y = (document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value = x + "+");

            r = Number(soma().x) + Number(soma().y);
        }
    }
    function sub() {
        var exp = /^(-|\+)? *[0-9]+(\.[0-9]+)?( *(-|\+|\*|\/) *[0-9]+(\.[0-9]+)?)*$/;

        var x = eval(document.getElementById("campoValorOperacaoCaixa").value);
        if (x == "") {
            alert("O valor informado é inválido!");

        }
        if (exp.test(x) == false) {
            alert("valor informado é inválido, substitua , por . ");

        }
        else {

            var k = (document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value + "-");

            var y = eval(document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value = x + "-");
            r = Number(sub().x) - Number(sub().y);
        }
    }
    function mult() {
        var exp = /^(-|\+)? *[0-9]+(\.[0-9]+)?( *(-|\+|\*|\/) *[0-9]+(\.[0-9]+)?)*$/;
        var x = eval(document.getElementById("campoValorOperacaoCaixa").value);

        if (x == "") {
            alert("O valor informado é inválido!");

        }
        if (exp.test(x) == false) {
            alert("valor informado é inválido, substitua , por . ");

        }
        else {
            var k = (document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value + "*");
            var y = eval(document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value = x + "*");
            r = Number(mult().x) * Number(mult().y);

        }



    }

    function divi() {
        var exp = /^(-|\+)? *[0-9]+(\.[0-9]+)?( *(-|\+|\*|\/) *[0-9]+(\.[0-9]+)?)*$/;

        var x = eval(document.getElementById("campoValorOperacaoCaixa").value);
        if (x == "") {
            alert("O valor informado é inválido!");

        }
        if (exp.test(x) == false) {
            alert("valor informado é inválido, substitua , por . ");

        }
        else {
            var k = (document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value + "/");
            var y = eval(document.getElementById("campoValorOperacaoCaixa").value = document.getElementById("campoValorOperacaoCaixa").value = x + "/");
            r = Number(mult().x) / Number(mult().y);

        }


    }