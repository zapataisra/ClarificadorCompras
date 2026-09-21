import sys
import json
import os
import time
import undetected_chromedriver as uc
from selenium.webdriver.common.by import By
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.common.keys import Keys

import sys
import json
import os
import time
import undetected_chromedriver as uc
from selenium.webdriver.common.by import By
import re

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
USER_DATA_DIR = os.path.join(SCRIPT_DIR, "tiktok-profile-uc")

def scrape(url):
    options = uc.ChromeOptions()
    options.add_argument(f"--user-data-dir={USER_DATA_DIR}")
    options.add_argument('--user-agent=Mozilla/5.0 (Linux; Android 13; SM-S918B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Mobile Safari/537.36')
    options.add_argument('--window-size=412,915')
    options.add_argument('--disable-blink-features=AutomationControlled')
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    options.add_argument("--disable-gpu")

    driver = None
    try:
        driver = uc.Chrome(options=options, headless=True, use_subprocess=False, version_main=151)
        driver.set_page_load_timeout(15)
        
        driver.get(url)
        time.sleep(3.5) 

        # --- ESCUDO ANTI-POPUP AUTOMÁTICO (VERSIÓN SELENIUM) ---
        try:
            # Mandamos la tecla "Escape" al navegador para cerrar ventanas emergentes
            ActionChains(driver).send_keys(Keys.ESCAPE).perform()
            
            # La pausa táctica para dejar que el fantasma desaparezca
            time.sleep(4) 
            
            # Un segundo "Escape" de seguridad
            ActionChains(driver).send_keys(Keys.ESCAPE).perform()
        except:
            pass # Si falla por alguna razón, simplemente lo ignora y sigue
        # -------------------------------------------------------

        try: driver.execute_script("window.scrollBy(0, 500);")
        except: pass
        time.sleep(0.5)

        try:
            texto_pantalla = driver.find_element(By.TAG_NAME, "body").text
        except Exception as e:
            return {"status": "error", "text": None, "error": f"Fallo al extraer texto: {str(e)}"}

        if "Verify to continue" in texto_pantalla or "puzzle piece" in texto_pantalla.lower():
            return {"status": "captcha_detected", "text": None, "error": None}

        # 1. Limpiamos el texto y lo cortamos para ahorrar tokens en la IA
        texto_limpio = " ".join(texto_pantalla.split())[:2000]

        # 2. Extraemos un precio base rápido (C# nos lo exige en el modelo ScrapedData).
        # Buscamos un patrón de moneda básico como "$ 123.45". Si no, mandamos "0".
        match = re.search(r'\$\s*[\d,]+\.\d{2}', texto_pantalla)
        precio_base = match.group(0) if match else "0"

        # 3. Armamos EXACTAMENTE el diccionario que espera la Opción B en C#
        datos_extraidos = {
            "precio_base": precio_base,
            "texto_pagina": texto_limpio
        }

        # 4. Serializamos a JSON
        return {"status": "success", "text": json.dumps(datos_extraidos), "error": None}

    except Exception as e:
        return {"status": "error", "text": None, "error": str(e)}
    finally:
        if driver:
            try: driver.quit()
            except: pass

if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    if len(sys.argv) > 1:
        print(json.dumps(scrape(sys.argv[1])))
    else:
        print(json.dumps({"status": "error", "text": None, "error": "No URL provided"}))

#SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
#USER_DATA_DIR = os.path.join(SCRIPT_DIR, "tiktok-profile-uc")
#
#def detectar_selector_opciones(driver, texto_pagina):
#    patron_etiquetas = re.search(
#        r'\b(color|talla|tama[ñn]o|size|modelo|estilo|style)\s*:',
#        texto_pagina, re.IGNORECASE
#    )
#    if patron_etiquetas:
#        debug(f"Selector detectado por etiqueta: '{patron_etiquetas.group(0)}'")
#        return True
#    try:
#        grupos_miniaturas = driver.execute_script("""
#            let imgs = Array.from(document.querySelectorAll('img'));
#            let candidatas = imgs.filter(img => {
#                let r = img.getBoundingClientRect();
#                return r.width >= 30 && r.width <= 150 && r.height >= 30 && r.height <= 150;
#            });
#            let grupos = {};
#            candidatas.forEach(img => {
#                let r = img.getBoundingClientRect();
#                let clave = Math.round(r.width / 10) + '-' + Math.round(r.height / 10);
#                grupos[clave] = (grupos[clave] || 0) + 1;
#            });
#            return Math.max(0, ...Object.values(grupos));
#        """)
#        if grupos_miniaturas >= 2:
#            debug(f"Selector detectado por miniaturas agrupadas: {grupos_miniaturas}")
#            return True
#    except Exception as e:
#        debug(f"Error detectando miniaturas: {str(e)}")
#    return False
#
#
#def extraer_descripcion_larga(driver):
#    try:
#        texto = driver.execute_script("""
#            let candidatos = Array.from(document.querySelectorAll('*')).filter(el =>
#                el.innerText && /descripci[oó]n del producto|product description/i.test(el.innerText) && el.innerText.length < 100
#            );
#            if (candidatos.length === 0) return null;
#            let encabezado = candidatos[0];
#            let contenedor = encabezado.closest('div');
#            let siguiente = contenedor ? contenedor.nextElementSibling : null;
#            return siguiente ? siguiente.innerText.trim() : (contenedor ? contenedor.innerText.trim() : null);
#        """)
#        return texto[:1200] if texto else None
#    except Exception as e:
#        debug(f"Error extrayendo descripción larga: {str(e)}")
#        return None
#
#
#
#def scrape(url):
#    options = uc.ChromeOptions()
#    options.add_argument(f"--user-data-dir={USER_DATA_DIR}")
#    options.add_argument('--user-agent=Mozilla/5.0 (Linux; Android 13; SM-S918B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Mobile Safari/537.36')
#    options.add_argument('--window-size=412,915')
#    options.add_argument('--disable-blink-features=AutomationControlled')
#    options.add_argument("--no-sandbox")
#    options.add_argument("--disable-dev-shm-usage")
#    options.add_argument("--disable-gpu")
#
#    driver = None
#    try:
#        driver = uc.Chrome(options=options, headless=False, use_subprocess=False, version_main=151)
#        driver.set_page_load_timeout(15)
#        
#        driver.get(url)
#        time.sleep(3.5) 
#
#        try: driver.execute_script("window.scrollBy(0, 500);")
#        except: pass
#        time.sleep(0.5)
#
#        try:
#            texto_pantalla = driver.find_element(By.TAG_NAME, "body").text
#        except Exception as e:
#            return {"status": "error", "text": None, "error": f"Fallo al leer: {str(e)}"}
#
#        if "Verify to continue" in texto_pantalla or "puzzle piece" in texto_pantalla.lower():
#            return {"status": "captcha_detected", "text": None, "error": None}
#
#        # 1. Limpiamos el texto y lo cortamos a 2000 caracteres
#        texto_limpio = " ".join(texto_pantalla.split())[:2000]
#
#        # 2. Armamos el diccionario exactamente con las variables que C# espera leer.
#        tiene_selector_opciones = detectar_selector_opciones(driver, texto_pagina)
#        descripcion_larga = extraer_descripcion_larga(driver)
#
#        resultado = {
#            "precio_base": precio_base,
#            "rango_precio": rango_precio,
#            "tiene_selector_opciones": tiene_selector_opciones,
#            "tiene_oferta_relampago": tiene_oferta_relampago,
#            "tiene_cupon": tiene_cupon,
#            "descripcion_producto": descripcion_larga or texto_pagina[:800],
#            "texto_pagina": texto_pagina[:800]
#        }
#
#        return {"status": "success", "text": json.dumps(resultado, ensure_ascii=False), "error": None}
#
#    except Exception as e:
#        return {"status": "error", "text": None, "error": str(e)}
#    finally:
#        if driver:
#            try: driver.quit()
#            except: pass
#
#if __name__ == '__main__':
#    sys.stdout.reconfigure(encoding='utf-8')
#    if len(sys.argv) > 1:
#        print(json.dumps(scrape(sys.argv[1])))
#    else:
#        print(json.dumps({"status": "error", "text": None, "error": "No URL provided"}))

