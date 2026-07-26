import psycopg2
import sys
import os

def main():
    conn_str = os.environ.get("NEON_CONN_STR") or (sys.argv[1] if len(sys.argv) > 1 else None)
    if not conn_str:
        print("Usage: python execute_neon_seed.py <CONNECTION_STRING>")
        return

    print("Connecting to database...")
    conn = psycopg2.connect(conn_str)
    
    script_dir = os.path.dirname(os.path.abspath(__file__))
    sql_file = os.path.join(script_dir, "SeedKoreanMenuExpanded.sql")
    
    with open(sql_file, "r", encoding="utf-8") as f:
        sql = f.read()
    
    print("Executing SeedKoreanMenuExpanded.sql...")
    with conn.cursor() as cur:
        cur.execute(sql)
    conn.commit()
    print("SUCCESS: Seed data executed cleanly!")

    with conn.cursor() as cur:
        cur.execute('SELECT COUNT(*) FROM "Categories";')
        cat_count = cur.fetchone()[0]
        cur.execute('SELECT COUNT(*) FROM "MenuItems";')
        item_count = cur.fetchone()[0]
        cur.execute('SELECT COUNT(*) FROM "ChoiceGroups";')
        group_count = cur.fetchone()[0]
        cur.execute('SELECT COUNT(*) FROM "ChoiceItems";')
        choice_count = cur.fetchone()[0]
        cur.execute('SELECT COUNT(*) FROM "MenuItemChannelPrices";')
        item_price_count = cur.fetchone()[0]
        print(f"Verification Stats: Categories={cat_count}, MenuItems={item_count}, ChoiceGroups={group_count}, ChoiceItems={choice_count}, MenuItemChannelPrices={item_price_count}")

    conn.close()

if __name__ == "__main__":
    main()
