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


## 🔐 Configuración Local (Secretos y API Keys)

Por seguridad y buenas prácticas, las credenciales, tokens y contraseñas de bases de datos han sido excluidas de este repositorio (`.gitignore`). 

Para ejecutar el proyecto en tu máquina, debes crear tus propios archivos de configuración:

### 1. Variables del Backend (C# .NET)
Crea un archivo llamado `appsettings.json` dentro de la carpeta raíz del proyecto API (`Clarificador.Api/`) y agrega la siguiente estructura con tus propios datos:

json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR; Database=TU_BD; User=TU_USUARIO; Password=TU_PASSWORD;"
  },
  "WhatsApp": {
    "VerifyToken": "TU_TOKEN_INVENDADO_PARA_WEBHOOKS",
    "MetaAccessToken": "TU_TOKEN_REAL_DE_META",
    "PhoneNumberId": "TU_ID_DE_NUMERO_DE_META"
  },
  "DeepSeek": {
    "ApiKey": "TU_API_KEY_DE_DEEPSEEK"
  }
}

### 2. Conexión con Meta (Configuración del Webhook)

Para que WhatsApp logre comunicarse con esta API, debes vincularlos desde el panel de **Meta for Developers**:

1. Ve a la configuración de WhatsApp > Configuración de la API.
2. En la sección de **Webhook**, haz clic en "Editar".
3. **URL de devolución de llamada (Callback URL):** Ingresa la ruta pública donde está alojada tu API. Si usas el controlador por defecto, la ruta termina en `/api/webhook` (ej. `https://tu-dominio.com/api/webhook`).
    * *Nota para desarrollo:* Si corres el proyecto en local, usa **Ngrok** para generar una URL pública temporal (ej. `https://tu-ngrok.app/api/webhook`).
4. **Token de verificación:** Ingresa exactamente el mismo texto que configuraste en tu `appsettings.json` bajo la variable `VerifyToken`.
5. Guarda los cambios y haz clic en **Administrar** para suscribir el webhook al evento `messages`. Si no haces esto, Meta no te reenviará los mensajes de los usuarios.

### 3 🚀 Pruebas en Local (El puente con Ngrok)

WhatsApp Cloud API exige una URL pública y con certificado SSL (HTTPS) para enviarte los mensajes. Si estás desarrollando y corriendo la API en tu propia computadora, necesitas crear un túnel para exponer tu puerto local a internet. La herramienta estándar para esto es **Ngrok**.

1. Asegúrate de tener [Ngrok](https://ngrok.com/) instalado en tu equipo.
2. Identifica en qué puerto está corriendo tu API de C# (frecuentemente el `5000`, `5001` o el que te asigne Visual Studio).
3. Abre una terminal nueva y levanta el túnel con este comando:
   ```bash
   ngrok http 5000

