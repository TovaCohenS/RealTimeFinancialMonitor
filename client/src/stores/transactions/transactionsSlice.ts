import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { Transaction } from "../../types/transaction";

export type ConnectionStatus = "disconnected" | "connecting" | "connected" | "error";

type TransactionsState = {
  items: Transaction[];
  showOnlyErrors: boolean;
  connectionStatus: ConnectionStatus;
  maxItems: number;
};

const initialState: TransactionsState = {
  items: [],
  showOnlyErrors: false,
  connectionStatus: "disconnected",
  maxItems: 1000,
};

function mergeAndTrim(existing: Transaction[], incoming: Transaction[], maxItems: number) {
  const merged = [...incoming, ...existing]; 
  return merged.slice(0, maxItems);
}

export const transactionsSlice = createSlice({
  name: "transactions",
  initialState,
  reducers: {
    setShowOnlyErrors(state, action: PayloadAction<boolean>) {
      state.showOnlyErrors = action.payload;
    },
    setConnectionStatus(state, action: PayloadAction<TransactionsState["connectionStatus"]>) {
      state.connectionStatus = action.payload;
    },
    initialSnapshot(state, action: PayloadAction<Transaction[]>) {
      state.items = action.payload;
    },
    addBatch(state, action: PayloadAction<Transaction[]>) {
      state.items = mergeAndTrim(state.items, action.payload, state.maxItems);
    },
    clear(state) {
      state.items = [];
    },
  },
});

export const {
  setShowOnlyErrors,
  setConnectionStatus,
  initialSnapshot,
  addBatch,
  clear,
} = transactionsSlice.actions;

export default transactionsSlice.reducer;