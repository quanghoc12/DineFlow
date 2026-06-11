import { Link } from 'react-router-dom';
import { getClientToken, getTableTokenFromUrl } from '../features/table/tableStorage';

export default function QrLandingPage() {
  const tableToken = getTableTokenFromUrl();
  const clientToken = getClientToken();

  return (
    <main className="page">
      <h1>DineFlow</h1>
      <p>Table token: {tableToken ?? 'missing'}</p>
      <p>Client token: {clientToken}</p>
      <Link to="/menu">Xem menu</Link>
    </main>
  );
}
