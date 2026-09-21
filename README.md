# Clarificador de Compras (E-commerce Translator) 🛒🤖

Este proyecto es un bot de WhatsApp diseñado para analizar enlaces de TikTok Shop, extraer los datos reales del producto (evadiendo la publicidad de precios "gancho") y explicarle al usuario final el costo real mediante Inteligencia Artificial.

## 🏗️ Arquitectura del Sistema

El sistema opera bajo un enfoque de microservicios y procesamiento asíncrono:

- **Core Backend:** Web API en C# .NET que recibe el webhook de Meta (WhatsApp Cloud API) y orquesta el flujo.
- **Data Extraction:** Microservicio en Python utilizando Selenium para web scraping dinámico y evasión de bloqueos en modo incógnito.
- **Inteligencia Artificial:** Integración con LLM (DeepSeek-V3) mediante Prompt Engineering para traducir estructuras complejas de precios a lenguaje humano y empático.
- **Persistencia de Datos:** Base de datos MySQL relacional que actúa como caché temporal (TTL de 15 minutos) y registro de auditoría.

## ⚙️ Stack Tecnológico
- **Backend:** .NET (C#)
- **Scraping:** Python 3.11 + Selenium
- **Base de Datos:** MySQL (InnoDB) + Entity Framework (Pomelo)
- **Integraciones:** WhatsApp Cloud API, DeepSeek API
