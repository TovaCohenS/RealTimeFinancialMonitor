import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../stores/hooks";
import { getRecent } from "../../services/apiClient";
import { initialSnapshot, setShowOnlyErrors } from "../../stores/transactions/transactionsSlice";
import { TransactionTable } from "../../components/TransactionTable";


export function MonitorPage() {
  const dispatch = useAppDispatch();
  const { items, showOnlyErrors, connectionStatus } = useAppSelector((s) => s.transactions);


  useEffect(() => {
    (async () => {
      try {
        const recent = await getRecent(100);
        recent.sort((a, b) => (a.timestamp < b.timestamp ? 1 : -1));
        dispatch(initialSnapshot(recent));
      } catch {
        console.log("failed to load initial snapshot");
      }
    })();
  }, [dispatch]);

  const filtered = showOnlyErrors ? items.filter((x) => x.status === "Failed") : items;

  return (
    <div style={{ padding: 16, display: "grid", gap: 12 }}>
      <h2>/monitor — Live Dashboard</h2>

      <div style={{ display: "flex", gap: 12, alignItems: "center", flexWrap: "wrap" }}>
        <div>
          Connection: <b>{connectionStatus}</b>
        </div>

        <label style={{ display: "flex", gap: 8, alignItems: "center" }}>
          <input
            type="checkbox"
            checked={showOnlyErrors}
            onChange={(e) => dispatch(setShowOnlyErrors(e.target.checked))}
          />
          Show only Errors
        </label>

        <div>
          Items shown: <b>{filtered.length}</b>
        </div>
      </div>

      <TransactionTable items={filtered} />
    </div>
  );
}