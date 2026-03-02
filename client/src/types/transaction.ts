import type { TransactionStatus } from "./transactionStatus";


export type Transaction={ 
  transactionId: string;
  amount: number;
  currency: string;
  status: TransactionStatus;
  timestamp: string; 
};