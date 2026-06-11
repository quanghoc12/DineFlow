import httpClient from '../../api/httpClient';

export async function getCustomerMenu() {
  const response = await httpClient.get('/api/customer/menu');
  return response.data;
}
