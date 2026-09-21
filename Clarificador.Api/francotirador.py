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

