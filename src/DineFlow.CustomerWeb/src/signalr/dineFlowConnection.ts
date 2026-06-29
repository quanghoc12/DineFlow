import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { CustomerMessage, CustomerSession } from "../models/customer";

export type RealtimeEvent = {
  tableSessionId: number;
  tableId?: number | null;
  orderId?: number | null;
  requestId?: number | null;
  billId?: number | null;
  eventTime: string;
};

export const realtimeEvents = {
  customerMessageCreated: "CustomerMessageCreated",
  customerOrderStatusChanged: "CustomerOrderStatusChanged",
  tableSessionChanged: "TableSessionChanged"
};

export function createDineFlowConnection(apiBaseUrl: string) {
  return new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/dineflow`, { withCredentials: false })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}

export async function joinCustomerRealtime(connection: HubConnection, session: CustomerSession) {
  if (connection.state !== HubConnectionState.Connected) {
    await connection.start();
  }

  await connection.invoke("JoinCustomer", session.clientToken);
  await connection.invoke("JoinSession", session.tableSessionId);
}

export function upsertCustomerMessage(messages: CustomerMessage[], nextMessage: CustomerMessage) {
  const nextKey = buildMessageKey(nextMessage);
  const exists = messages.some((message) => buildMessageKey(message) === nextKey);

  if (!exists) {
    return [...messages, nextMessage].sort(compareMessages);
  }

  return messages
    .map((message) => buildMessageKey(message) === nextKey ? nextMessage : message)
    .sort(compareMessages);
}

function buildMessageKey(message: CustomerMessage) {
  return `${message.messageType}:${message.sourceId}`;
}

function compareMessages(left: CustomerMessage, right: CustomerMessage) {
  return new Date(left.createdAt).getTime() - new Date(right.createdAt).getTime();
}
