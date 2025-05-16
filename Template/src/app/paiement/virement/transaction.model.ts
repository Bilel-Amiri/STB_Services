// transaction.model.ts
export interface Transaction {
  transactionId: string;
  accountId: number;
  amount: number;
  transactionType: string;
  transactionDate: Date;  
  targetRib?: string;     
  targetAccountId?: number;
}