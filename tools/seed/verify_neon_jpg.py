import psycopg2
import sys
import os

def main():
    conn_str = os.environ.get("NEON_CONN_STR") or (sys.argv[1] if len(sys.argv) > 1 else None)
    if not conn_str:
        print("Usage: python verify_neon_jpg.py <CONNECTION_STRING>")
        return

    conn = psycopg2.connect(conn_str)
    with conn.cursor() as cur:
        cur.execute('SELECT COUNT(*) FROM "MenuItems" WHERE "ImageUrl" ILIKE \'%.webp%\';')
        webp_count = cur.fetchone()[0]
        cur.execute('SELECT COUNT(*) FROM "MenuItems" WHERE "ImageUrl" ILIKE \'%.jpg%\';')
        jpg_count = cur.fetchone()[0]
        print(f"Neon DB Check: MenuItems with .webp = {webp_count}, MenuItems with .jpg = {jpg_count}")
    conn.close()

if __name__ == "__main__":
    main()
