export interface PaymentScheduleDto {
  policyNumber: string;
  frequency: string;
  installmentAmount: number;
  totalInstallments: number;
  totalPayable: number;
  invoices: InvoiceResponse[];
}

export interface InvoiceResponse {
  invoiceId: number;
  invoiceNumber: string;
  policyNumber: string;
  periodYear: number;
  periodNumber: number;
  frequency: string;
  amountDue: number;
  dueDate: string;
  graceEndDate: string | null;
  status: string;
  paidOn: string | null;
  remarks: string | null;
  payment: PaymentResponse | null;
}

export interface MakePaymentDto {
  invoiceId: number;
  paymentMethod: string;
  transactionReference?: string;
}

export interface PaymentResponse {
  paymentId: number;
  policyNumber: string;
  amountPaid: number;
  status: string;
  paymentMethod: string;
  transactionReference: string | null;
  paymentDate: string;
}
