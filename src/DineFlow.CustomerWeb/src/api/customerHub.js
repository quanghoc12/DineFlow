import * as signalR from '@microsoft/signalr';

export function createCustomerHubConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(import.meta.env.VITE_CUSTOMER_HUB_URL ?? 'https://localhost:7001/hubs/customer')
    .withAutomaticReconnect()
    .build();
}
