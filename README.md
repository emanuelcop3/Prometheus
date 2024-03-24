# Coletando métricas do Prometheus de uma aplicação Flask usando o OpenTelemetry Collector

Esta é uma aplicação Flask simples que expõe métricas no formato Prometheus usando o [prometheus_flask_exporter](https://github.com/rycus86/prometheus_flask_exporter). O código faz uso do Prometheus e do OpenTelemetry. O Prometheus é uma plataforma de monitoramento de código aberto projetada para coletar e armazenar métricas de sistemas e serviços distribuídos, permitindo consultas flexíveis e geração de alertas com base em regras definidas pelo usuário. O OpenTelemetry, por sua vez, é um projeto de código aberto que padroniza a instrumentação de aplicativos para coletar telemetria, incluindo rastreamento de solicitações, métricas e logs.

## Conceitos Aprendidos

Ao realizar essa ponderada, aprendi como o Prometheus funciona, o que será útil para implementá-lo em projetos futuros. Também entendi a estrutura necessária para criar uma métrica. Além disso, pude complementar meu conhecimento sobre logs, uma vez que eles mostravam o registro das métricas medidas.

## Passos para executar o aplicativo Flask

1. Ative seu ambiente virtual (se ainda não estiver ativado)
```
source venv/bin/activate   # No macOS ou Linux
```

2. Instale os pacotes necessários
```
python -m pip install flask prometheus_flask_exporter
python -m pip install requests
```

3. Inicie a aplicação Flask
```
python app.py
```

Os diferentes endpoints para a aplicação são `127.0.0.1:5000/one`, `127.0.0.1:5000/two`, `127.0.0.1:5000/three`, `127.0.0.1:5000/four` e `127.0.0.1:5000/error`.

4. Em um novo terminal, inicie o gerador
```
python app-generator.py
```
Ele será usado para gerar solicitações para a aplicação Flask nos diferentes endpoints.

5. Envie as métricas para uma ferramenta de monitoramento para visualização. Você pode fazer isso com OpenTelemetry e SigNoz, leia o artigo aqui.

![Descrição da imagem](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/cbzf58qelvvm2tmb4mn6.png)