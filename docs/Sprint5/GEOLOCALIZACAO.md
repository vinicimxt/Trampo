# Geolocalização — Sprint 5

Endereço postal e geocodificação são operações separadas. ViaCEP consulta CEP/logradouro e não fornece coordenadas. EnderecoService usa IGeocodificacaoProvider para coordenadas de locais profissionais; a implementação registrada é GeocodificacaoNaoConfigurada, que retorna NULL.

Não há credencial, fornecedor pago ou chamada real de geocodificação. O cadastro funciona manualmente e Latitude/Longitude ficam NULL. Nenhuma coordenada foi inventada ou derivada do CEP. Os testes de provider usam coordenadas simuladas e não configuram um fornecedor real.

Uma implementação futura deve permanecer na integração, receber endereço validado, respeitar cancelamento/timeout e proteger suas credenciais fora do Git. A aplicação limita a espera a quatro segundos, valida os intervalos e arredonda para seis casas. Indisponibilidade ou resultado inválido deixa o local sem coordenadas. O provider deve também cancelar seu trabalho interno; o limite de espera não substitui essa responsabilidade.

DECIMAL(9,6) suporta latitude e longitude dentro dos intervalos geográficos; a precisão de armazenamento não representa garantia de precisão do fornecedor. Requests de cadastro não contêm latitude/longitude: a API recusa membros desconhecidos. Domicílios não são enviados ao geocoder e não recebem coordenadas.

## Decisões de escopo

Busca por proximidade, ordenação por distância, mapas e botão Usar minha localização foram avaliados e adiados. Sem um conjunto confiável de coordenadas e um caso de uso de distância implementado, pedir localização ao navegador não produziria resultado útil e coletaria um dado desnecessário. A busca textual existente e a entrada manual continuam disponíveis.

Quando houver busca geográfica, validar latitude/longitude e raio máximo; excluir locais sem coordenadas confiáveis. Distância em linha reta poderá ser calculada localmente; não apresentá-la como distância ou tempo de carro. Avaliar geography e índices espaciais do SQL Server somente com consultas e volume que os justifiquem.

Qualquer futuro uso da Geolocation API deve ocorrer por ação clara, com permissão do navegador, tratamento de recusa e alternativa manual. Coordenadas de uma consulta temporária não se tornam localização oficial de local profissional.
