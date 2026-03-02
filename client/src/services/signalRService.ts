import * as signalR from "@microsoft/signalr";
import type { Transaction } from "../types/transaction";
import { store } from "../stores/store";
import { addBatch, setConnectionStatus } from "../stores/transactions/transactionsSlice";

export type HubConnectionStatus = "disconnected" | "connecting" | "connected" | "error";

export type TransactionsHubOptions = {
    onBatch: (transactions: Transaction[]) => void;
    onStatusChange: (status: HubConnectionStatus) => void;
    flushIntervalMs?: number;
};


 class TransactionsHubService {
    private connection: signalR.HubConnection;
    private readonly options: Required<TransactionsHubOptions>;
    private buffer: Transaction[] = [];
    private timer: number | null = null;

    constructor(hubUrl: string, options: TransactionsHubOptions) {
        this.options = { flushIntervalMs: 100, ...options };

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { withCredentials: true })
            .withAutomaticReconnect()
            .build();

        this.connection.on("transactionReceived", (tx: Transaction) => {
            console.log("[SignalR] transactionReceived:", tx);
            this.buffer.push(tx);
        });

        this.connection.onreconnecting(() => {
            this.options.onStatusChange("connecting");
        });

        this.connection.onreconnected(() => {
            this.options.onStatusChange("connected");
        });

        this.connection.onclose(() => {
            this.options.onStatusChange("disconnected");
        });

        window.addEventListener("beforeunload", () => {
            this.stop();
        });
    }

    async start(): Promise<void> {
        const state = this.connection.state;
        if (
            state === signalR.HubConnectionState.Connected ||
            state === signalR.HubConnectionState.Connecting ||
            state === signalR.HubConnectionState.Reconnecting
        ) {
            console.log("[SignalR] already active, skipping start");
            return;
        }

        this.options.onStatusChange("connecting");

        if (this.timer === null) {
            this.timer = window.setInterval(() => {
                if (this.buffer.length > 0) {
                    console.log("[SignalR] flushing buffer, count:", this.buffer.length);
                    const batch = [...this.buffer].sort((a, b) =>
                        a.timestamp < b.timestamp ? 1 : -1
                    );
                    this.buffer = [];
                    this.options.onBatch(batch);
                }
            }, this.options.flushIntervalMs);
        }

        try {
            console.log("[SignalR] starting connection to hub");
            await this.connection.start();
            console.log(
                "[SignalR] connected. Transport:",
                (this.connection as any).connection?.transport?.name ?? "unknown"
            );
            this.options.onStatusChange("connected");
        } catch (err) {
            console.error("[SignalR] connection failed:", err);
            this.options.onStatusChange("error");
        }
    }

    async stop(): Promise<void> {
        if (this.timer !== null) {
            window.clearInterval(this.timer);
            this.timer = null;
        }
        try {
            await this.connection.stop();
        } finally {
            this.options.onStatusChange("disconnected");
        }
    }
}
export default new TransactionsHubService(import.meta.env.VITE_HUB_URL as string, {
    onBatch: (txs) => store.dispatch(addBatch(txs)),
    onStatusChange: (status) => store.dispatch(setConnectionStatus(status)),
});