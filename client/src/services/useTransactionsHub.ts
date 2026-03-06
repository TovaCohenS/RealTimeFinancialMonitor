/**
 * useTransactionsHub
 *
 * Alternative to the singleton TransactionsHubService.
 * This hook owns the SignalR connection lifecycle inside a React component,
 * dispatching batches and status changes directly to the Redux store via useDispatch.
 *
 * Usage (e.g. in App.tsx or a layout component):
 *
 *   const { start, stop, status } = useTransactionsHub();
 *   useEffect(() => { start(); return () => { stop(); }; }, []);
 */

import { useRef,  useCallback } from "react";
import * as signalR from "@microsoft/signalr";
import { useDispatch } from "react-redux";
import type { AppDispatch } from "../stores/store";
import { addBatch, setConnectionStatus } from "../stores/transactions/transactionsSlice";
import type { Transaction } from "../types/transaction";
import type { HubConnectionStatus } from "./signalRService";

const HUB_URL = import.meta.env.VITE_HUB_URL || "/hubs/transactions";
const FLUSH_INTERVAL_MS = 100;

export function useTransactionsHub() {
    const dispatch = useDispatch<AppDispatch>();

    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const bufferRef = useRef<Transaction[]>([]);
    const timerRef = useRef<number | null>(null);

    // ── helpers ──────────────────────────────────────────────────────────────

    const updateStatus = useCallback(
        (s: HubConnectionStatus) => {
            dispatch(setConnectionStatus(s));
        },
        [dispatch]
    );

    const startFlush = useCallback(() => {
        if (timerRef.current !== null) return;
        timerRef.current = window.setInterval(() => {
            if (bufferRef.current.length > 0) {
                const batch = [...bufferRef.current].sort((a, b) =>
                    a.timestamp < b.timestamp ? 1 : -1
                );
                bufferRef.current = [];
                dispatch(addBatch(batch));
            }
        }, FLUSH_INTERVAL_MS);
    }, [dispatch]);

    const stopFlush = useCallback(() => {
        if (timerRef.current !== null) {
            window.clearInterval(timerRef.current);
            timerRef.current = null;
        }
    }, []);

    const buildConnection = useCallback((): signalR.HubConnection => {
        const conn = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, { withCredentials: true })
            .withAutomaticReconnect()
            .build();

        conn.on("transactionReceived", (tx: Transaction) => {
            bufferRef.current.push(tx);
        });

        conn.onreconnecting(() => updateStatus("connecting"));
        conn.onreconnected(() => updateStatus("connected"));
        conn.onclose(() => updateStatus("disconnected"));

        return conn;
    }, [updateStatus]);

    // ── public API ───────────────────────────────────────────────────────────

    const start = useCallback(async () => {
        // Reuse existing connection if already active
        if (connectionRef.current) {
            const s = connectionRef.current.state;
            if (
                s === signalR.HubConnectionState.Connected ||
                s === signalR.HubConnectionState.Connecting ||
                s === signalR.HubConnectionState.Reconnecting
            ) {
                return;
            }
        }

        connectionRef.current = buildConnection();
        updateStatus("connecting");
        startFlush();

        try {
            await connectionRef.current.start();
            updateStatus("connected");
        } catch (err) {
            console.error("[useTransactionsHub] connection failed:", err);
            updateStatus("error");
            stopFlush();
        }
    }, [buildConnection, updateStatus, startFlush, stopFlush]);

    const stop = useCallback(async () => {
        stopFlush();
        if (connectionRef.current) {
            try {
                await connectionRef.current.stop();
            } finally {
                connectionRef.current = null;
                updateStatus("disconnected");
            }
        }
    }, [stopFlush, updateStatus]);

    return { start, stop };
}
