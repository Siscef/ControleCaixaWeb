ALTER TABLE controlecaixaweb_operacaocaixa ADD COLUMN usuariodatahorainsercao character varying(255);
ALTER TABLE controlecaixaweb_operacaocaixa ADD COLUMN datahorainsercao bigint;

--controlecaixaweb_despejooperacaocaixa
ALTER TABLE controlecaixaweb_despejooperacaocaixa ADD COLUMN usuariodatahorainsercao character varying(255);
ALTER TABLE controlecaixaweb_despejooperacaocaixa ADD COLUMN datahorainsercao bigint;
--controlecaixaweb_fechamentocaixa

ALTER TABLE controlecaixaweb_fechamentocaixa ADD COLUMN usuariodatahorainsercao character varying(255);
ALTER TABLE controlecaixaweb_fechamentocaixa ADD COLUMN datahorainsercao bigint;
--controlecaixaweb_operacaofinanceira

ALTER TABLE controlecaixaweb_operacaofinanceira ADD COLUMN usuariodatahorainsercao character varying(255);
ALTER TABLE controlecaixaweb_operacaofinanceira ADD COLUMN datahorainsercao bigint;
--controlecaixaweb_pagamento

ALTER TABLE controlecaixaweb_pagamento ADD COLUMN usuariodatahorainsercao character varying(255);
ALTER TABLE controlecaixaweb_pagamento ADD COLUMN datahorainsercao bigint;


