#!/usr/bin/env python3
import json
import os
import sys

def replace_appsettings_values(api_url, version_tag):
   try:
       # Client appsettings.json
       client_path = "../src/Client/Client/appsettings.json"
       with open(client_path, 'r', encoding='utf-8') as f:
           client_config = json.load(f)
       
       client_config["HttpClient"]["SunnyEast"] = api_url
       
       with open(client_path, 'w', encoding='utf-8') as f:
           json.dump(client_config, f, indent=2, ensure_ascii=False)
       
       print(f"✅ Updated {client_path}: HttpClient.SunnyEast = {api_url}")
       
       # WebApi appsettings.json  
       api_path = "../src/WebApi/appsettings.json"
       with open(api_path, 'r', encoding='utf-8') as f:
           api_config = json.load(f)
       
       api_config["ServerVersion"] = version_tag
       
       with open(api_path, 'w', encoding='utf-8') as f:
           json.dump(api_config, f, indent=2, ensure_ascii=False)
           
       print(f"✅ Updated {api_path}: ServerVersion = {version_tag}")
       
   except FileNotFoundError as e:
       print(f"❌ File not found: {e}")
       sys.exit(1)
   except KeyError as e:
       print(f"❌ Key not found in JSON: {e}")
       sys.exit(1)
   except json.JSONDecodeError as e:
       print(f"❌ Invalid JSON: {e}")
       sys.exit(1)
   except Exception as e:
       print(f"❌ Error: {e}")
       sys.exit(1)

if __name__ == "__main__":
   api_url = os.environ.get('API_URL')
   version_tag = os.environ.get('VERSION_TAG')
   
   if not api_url:
       print("❌ API_URL environment variable not set")
       sys.exit(1)
       
   if not version_tag:
       print("❌ VERSION_TAG environment variable not set") 
       sys.exit(1)
   
   replace_appsettings_values(api_url, version_tag)