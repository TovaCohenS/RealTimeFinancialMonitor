import { useState } from "react";
import { v4 as uuid } from "uuid";
import type { Transaction } from "../../types/transaction";
import { postTransaction } from "../../services/apiClient";
import type { TransactionStatus } from "../../types/transactionStatus";

const statuses: TransactionStatus[] = ["Pending", "Completed", "Failed"];

export function AddPage() {
  const [amount, setAmount] = useState(1500.5);
  const [currency, setCurrency] = useState("USD");
  const [status, setStatus] = useState<TransactionStatus>("Pending");
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<string>("");

  const buildTx = (): Transaction => ({
    transactionId: uuid(),
    amount: Number(amount),
    currency,
    status,
    timestamp: new Date().toISOString(),
  });

  const sendOne = async () => {
    setBusy(true);
    setMessage("");
    try {
      const tx = buildTx();
      await postTransaction(tx);
      setMessage("Sent!");
    } catch (e: any) {
      setMessage(e?.message ?? "Failed");
    } finally {
      setBusy(false);
    }
  };

  const sendBurst = async (count: number) => {
    setBusy(true);
    setMessage("");
    try {
      for (let i = 0; i < count; i++) {
        const tx = buildTx();
        await postTransaction(tx);
      }
      setMessage(`Sent ${count} transactions`);
    } catch (e: any) {
      setMessage(e?.message ?? "Failed");
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

      {message && <div style={{ padding: 8, border: "1px solid #ddd", borderRadius: 8 }}>{message}</div>}
    </div>
  );
}