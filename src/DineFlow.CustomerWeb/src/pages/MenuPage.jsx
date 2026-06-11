import { useEffect, useState } from 'react';
import { getCustomerMenu } from '../features/menu/menuApi';

export default function MenuPage() {
  const [menu, setMenu] = useState({ categories: [], items: [] });
  const [error, setError] = useState('');

  useEffect(() => {
    getCustomerMenu()
      .then(setMenu)
      .catch((err) => setError(err.message));
  }, []);

  return (
    <main className="page">
      <h1>Menu</h1>
      {error && <p className="error">{error}</p>}
      {menu.categories.map((category) => (
        <section key={category.categoryId}>
          <h2>{category.categoryName}</h2>
          {menu.items
            .filter((item) => item.categoryId === category.categoryId)
            .map((item) => (
              <article key={item.menuItemId} className="card">
                <strong>{item.itemName}</strong>
                <span>{item.price}</span>
              </article>
            ))}
        </section>
      ))}
    </main>
  );
}
