import type { Transaction } from "../types/transaction";

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export async function postTransaction(tx: Transaction): Promise<void> {
  const res = await fetch(`${BASE_URL}/api/transactions`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(tx),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    if (body?.errors) {
      const messages = Object.entries(body.errors as Record<string, string[]>)
        .map(([field, msgs]) => `${field}: ${msgs.join(", ")}`)
        .join(" | ");
      throw new Error(messages);
    }
    throw new Error(body?.detail ?? body?.title ?? `Request failed (${res.status})`);
  }
}

export async function getRecent(limit = 100): Promise<Transaction[]> {
  const res = await fetch(`${BASE_URL}/api/transactions/recent?limit=${limit}`);
  if (!res.ok) throw new Error(`GET recent failed (${res.status})`);
  return res.json();
}