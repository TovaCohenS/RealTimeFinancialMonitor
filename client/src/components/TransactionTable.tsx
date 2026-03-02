import type { Transaction } from "../types/transaction";

function badgeStyle(status: Transaction["status"]): React.CSSProperties {
  const base: React.CSSProperties = {
    padding: "2px 10px",
    borderRadius: 999,
    fontSize: 12,
    fontWeight: 700,
    border: "1px solid #ddd",
    display: "inline-block",
  };

  switch (status) {
    case "Failed":
      return { ...base, borderColor: "#ffb3b3" };
    case "Completed":
      return { ...base, borderColor: "#b7f0c1" };
    default:
      return { ...base, borderColor: "#cfe3ff" };
  }
}

export function TransactionTable({ items }: { items: Transaction[] }) {
  return (
    <div style={{ overflow: "auto", border: "1px solid #ddd", borderRadius: 8 }}>
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ textAlign: "left" }}>
            <th style={{ padding: 8, borderBottom: "1px solid #eee" }}>Time (UTC)</th>
            <th style={{ padding: 8, borderBottom: "1px solid #eee" }}>TransactionId</th>
            <th style={{ padding: 8, borderBottom: "1px solid #eee" }}>Amount</th>
            <th style={{ padding: 8, borderBottom: "1px solid #eee" }}>Currency</th>
            <th style={{ padding: 8, borderBottom: "1px solid #eee" }}>Status</th>
          </tr>
        </thead>
        <tbody>
          {items.map((x) => (
            <tr key={`${x.transactionId}-${x.timestamp}`}>
              <td style={{ padding: 8, borderBottom: "1px solid #f3f3f3" }}>
                {new Date(x.timestamp).toISOString()}
              </td>
              <td style={{ padding: 8, borderBottom: "1px solid #f3f3f3", fontFamily: "monospace" }}>
                {x.transactionId}
              </td>
              <td style={{ padding: 8, borderBottom: "1px solid #f3f3f3" }}>{x.amount}</td>
              <td style={{ padding: 8, borderBottom: "1px solid #f3f3f3" }}>{x.currency}</td>
              <td style={{ padding: 8, borderBottom: "1px solid #f3f3f3" }}>
                <span style={badgeStyle(x.status)}>{x.status}</span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}