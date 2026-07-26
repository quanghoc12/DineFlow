import os
import sys
import glob
import urllib.request
import urllib.error
import json
from datetime import datetime

SUPABASE_URL = os.environ.get("SUPABASE_URL", "https://qcnchscanwgqeyyzipgu.supabase.co").rstrip("/")
SUPABASE_KEY = os.environ.get("SUPABASE_SERVICE_ROLE_KEY", "")
BUCKET = os.environ.get("SUPABASE_STORAGE_BUCKET", "menu-images")
IMAGE_DIR = os.environ.get("MENU_IMAGE_DIR", "jpgs")
CONTENT_TYPES = {
    ".jpg": "image/jpeg",
    ".jpeg": "image/jpeg",
    ".png": "image/png",
    ".webp": "image/webp",
}

def upload_image(file_path):
    filename = os.path.basename(file_path)
    date_str = datetime.utcnow().strftime("%Y%m%d")
    object_path = f"menu-items/{date_str}/{filename}"
    upload_url = f"{SUPABASE_URL}/storage/v1/object/{BUCKET}/{object_path}"
    public_url = f"{SUPABASE_URL}/storage/v1/object/public/{BUCKET}/{object_path}"

    if not SUPABASE_KEY:
        print(f"[PUBLIC URL GENERATED] {filename} -> {public_url}")
        return public_url

    with open(file_path, "rb") as f:
        data = f.read()

    req = urllib.request.Request(upload_url, data=data, method="POST")
    req.add_header("Authorization", f"Bearer {SUPABASE_KEY}")
    req.add_header("apikey", SUPABASE_KEY)
    req.add_header("Content-Type", CONTENT_TYPES.get(os.path.splitext(filename)[1].lower(), "application/octet-stream"))
    req.add_header("x-upsert", "true")

    try:
        with urllib.request.urlopen(req) as resp:
            print(f"[UPLOAD OK] {filename} -> {public_url}")
            return public_url
    except urllib.error.HTTPError as e:
        print(f"[UPLOAD WARN] {filename} HTTP {e.code}: {e.read().decode('utf-8', errors='ignore')}")
        return public_url
    except Exception as ex:
        print(f"[UPLOAD ERROR] {filename}: {ex}")
        return public_url

def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    image_dir = os.path.join(script_dir, IMAGE_DIR)
    files = sorted(
        file_path
        for pattern in ("*.jpg", "*.jpeg", "*.png", "*.webp")
        for file_path in glob.glob(os.path.join(image_dir, pattern))
    )
    if not files:
        print("No image files found in", image_dir)
        sys.exit(1)

    print(f"Processing {len(files)} images from {image_dir}...")
    results = {}
    for f in files:
        name = os.path.splitext(os.path.basename(f))[0]
        results[name] = upload_image(f)

    with open(os.path.join(script_dir, "uploaded_urls.json"), "w", encoding="utf-8") as out:
        json.dump(results, out, ensure_ascii=False, indent=2)

    print("Saved uploaded_urls.json")

if __name__ == "__main__":
    main()
