import { useState } from "react";
import { v4 as uuid } from "uuid";
import type { Transaction } from "../../types/transaction";
import { postTransaction } from "../../services/apiClient";
import type { TransactionStatus } from "../../types/transactionStatus";

const statuses: TransactionStatus[] = ["Pending", "Completed", "Failed"];

function validate(amount: number, currency: string, status: string): string[] {
  const errors: string[] = [];

  if (amount <= 0) errors.push("Amount must be greater than 0.");
  if (amount >= 1_000_000_000) errors.push("Amount must be less than 1 billion.");

  const trimmedCurrency = currency.trim();
  if (!trimmedCurrency) errors.push("Currency is required.");
  else if (trimmedCurrency.length < 3 || trimmedCurrency.length > 8) errors.push("Currency must be 3–8 characters.");
  else if (!/^[a-zA-Z]+$/.test(trimmedCurrency)) errors.push("Currency must be letters only (e.g., USD, EUR).");

  if (!statuses.includes(status as TransactionStatus))
    errors.push("Status must be one of: Pending, Completed, Failed.");

  return errors;
}

export function AddPage() {
  const [amount, setAmount] = useState(1500.5);
  const [currency, setCurrency] = useState("USD");
  const [status, setStatus] = useState<TransactionStatus>("Pending");
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<{ text: string; isError: boolean } | null>(null);

  const buildTx = (): Transaction => ({
    transactionId: uuid(),
    amount: Number(amount),
    currency,
    status,
    timestamp: new Date().toISOString(),
  });

  const sendOne = async () => {
    const errors = validate(amount, currency, status);
    if (errors.length) { setMessage({ text: errors.join("\n"), isError: true }); return; }

    setBusy(true);
    setMessage(null);
    try {
      const tx = buildTx();
      await postTransaction(tx);
      setMessage({ text: "Sent!", isError: false });
    } catch (e: any) {
      setMessage({ text: e?.message ?? "Failed", isError: true });
    } finally {
      setBusy(false);
    }
  };

  const sendBurst = async (count: number) => {
    const errors = validate(amount, currency, status);
    if (errors.length) { setMessage({ text: errors.join("\n"), isError: true }); return; }

    setBusy(true);
    setMessage(null);
    try {
      for (let i = 0; i < count; i++) {
        const tx = buildTx();
        await postTransaction(tx);
      }
      setMessage({ text: `Sent ${count} transactions`, isError: false });
    } catch (e: any) {
      setMessage({ text: e?.message ?? "Failed", isError: true });
    } finally {
      setBusy(false);
    }
  };

  return (
    <div style={{ padding: 16, display: "grid", gap: 12, maxWidth: 720 }}>
      <h2>/add — Transaction Simulator</h2>

      <label>
        Amount:
        <input
          type="number"
          value={amount}
          step={0.01}
          onChange={(e) => setAmount(Number(e.target.value))}
          style={{ marginLeft: 8 }}
        />
      </label>

      <label>
        Currency:
        <input value={currency} onChange={(e) => setCurrency(e.target.value)} style={{ marginLeft: 8 }} />
      </label>

      <label>
        Status:
        <select value={status} onChange={(e) => setStatus(e.target.value as TransactionStatus)} style={{ marginLeft: 8 }}>
          {statuses.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
      </label>

      <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
        <button disabled={busy} onClick={sendOne}>
          Send 1
        </button>
        <button disabled={busy} onClick={() => sendBurst(20)}>
          Send 20
        </button>
        <button disabled={busy} onClick={() => sendBurst(100)}>
          Send 100
        </button>
      </div>

      {message && (
        <div
          style={{
            padding: 8,
            borderRadius: 8,
            border: `1px solid ${message.isError ? "#f44336" : "#4caf50"}`,
            color: message.isError ? "#f44336" : "#2e7d32",
            background: message.isError ? "#fff5f5" : "#f5fff5",
            whiteSpace: "pre-wrap",
          }}
        >
          {message.isError ? "Error: " : ""}{message.text}
        </div>
      )}
    </div>
  );
}