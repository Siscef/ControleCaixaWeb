CREATE TABLE controlecaixaweb_configuracao
(
  codigo bigserial NOT NULL,
  casasdecimais integer NOT NULL,
  fazerlancamentocontacorrente boolean,
  enviaremailcaixaalterado boolean,
  estabelecimentopadrao bigint NOT NULL,
  CONSTRAINT controlecaixaweb_configuracao_pkey PRIMARY KEY (codigo),
  CONSTRAINT fk_configuracao_tem_estabelecimentopadrao FOREIGN KEY (estabelecimentopadrao)
      REFERENCES controlecaixaweb_estabelecimento (codigo) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE controlecaixaweb_configuracao
  OWNER TO postgres;




  ALTER TABLE controlecaixaweb_formapagamento_estabelecimento ADD COLUMN diasrecebimento integer;
ALTER TABLE controlecaixaweb_formapagamento_estabelecimento ALTER COLUMN diasrecebimento SET NOT NULL;

ALTER TABLE controlecaixaweb_operacao_caixa ADD COLUMN tipooperacao character varying(255);
ALTER TABLE controlecaixaweb_operacao_caixa ALTER COLUMN tipooperacao SET NOT NULL;

ALTER TABLE controlecaixaweb_operacao_financeira ADD COLUMN taxa numeric(19,5);

ALTER TABLE controlecaixaweb_operacao_financeira ADD COLUMN valorliquido numeric(19,5);
ALTER TABLE controlecaixaweb_operacao_financeira ADD COLUMN desconto numeric(19,5);
