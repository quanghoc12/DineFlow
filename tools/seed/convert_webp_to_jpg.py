import os
from PIL import Image

def main():
    base_dir = os.path.dirname(os.path.abspath(__file__))
    webp_dir = os.path.join(base_dir, "webps")
    jpg_dir = os.path.join(base_dir, "jpgs")
    
    os.makedirs(jpg_dir, exist_ok=True)
    
    files = [f for f in os.listdir(webp_dir) if f.endswith('.webp')]
    print(f"Found {len(files)} webp files to convert to JPG...")
    
    converted_count = 0
    for f in files:
        webp_path = os.path.join(webp_dir, f)
        jpg_name = f.replace('.webp', '.jpg')
        jpg_path = os.path.join(jpg_dir, jpg_name)
        
        with Image.open(webp_path) as img:
            # Convert RGBA/P to RGB for JPEG
            if img.mode in ('RGBA', 'LA', 'P'):
                rgb_img = Image.new('RGB', img.size, (255, 255, 255))
                if img.mode == 'P':
                    img = img.convert('RGBA')
                rgb_img.paste(img, mask=img.split()[-1] if img.mode == 'RGBA' else None)
                rgb_img.save(jpg_path, 'JPEG', quality=90, optimize=True)
            else:
                img.convert('RGB').save(jpg_path, 'JPEG', quality=90, optimize=True)
        
        size_kb = os.path.getsize(jpg_path) / 1024.0
        print(f"Converted: {jpg_name} ({size_kb:.1f} KB)")
        converted_count += 1
        
    print(f"Finished converting {converted_count} files to JPG in '{jpg_dir}'.")

if __name__ == "__main__":
    main()
