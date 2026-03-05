import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { roleGuard } from './core/guards/role-guard';

import { Home } from './pages/home/home';
import { Layout } from './pages/layout/layout';
import { Login } from './pages/auth/login/login';
import { Register } from './pages/auth/register/register';
import { ChangePassword } from './pages/auth/change-password/change-password';
import { ForgotPassword } from './pages/auth/forgot-password/forgot-password';
import { GlobalError } from './pages/error/global-error/global-error';

import { AdminDashboard } from './pages/dashboard/admin-dashboard/admin-dashboard';
import { AgentDashboard } from './pages/dashboard/agent-dashboard/agent-dashboard';
import { CustomerDashboard } from './pages/dashboard/customer-dashboard/customer-dashboard';
import { ClaimsOfficerDashboard } from './pages/dashboard/claims-officer-dashboard/claims-officer-dashboard';

import { PlanList } from './pages/policy/plan-list/plan-list';
import { PlanDetail } from './pages/policy/plan-detail/plan-detail';
import { RequestPolicy } from './pages/policy/request-policy/request-policy';
import { MyPolicies } from './pages/policy/my-policies/my-policies';
import { PolicyDetail } from './pages/policy/policy-detail/policy-detail';
import { AgentPolicies } from './pages/policy/agent-policies/agent-policies';
import { AgentPolicyReview } from './pages/policy/agent-policy-review/agent-policy-review';
import { AllPolicies } from './pages/policy/all-policies/all-policies';

// --- Claim Components ---
import { RaiseClaim } from './pages/claim/raise-claim/raise-claim';
import { MyClaims } from './pages/claim/my-claims/my-claims';
import { ClaimDetail } from './pages/claim/claim-detail/claim-detail';
import { OfficerClaims } from './pages/claim/officer-claims/officer-claims';
import { AllClaims } from './pages/claim/all-claims/all-claims';



import { RequestEndorsement } from './pages/endorsement/request-endorsement/request-endorsement';
import { MyEndorsements } from './pages/endorsement/my-endorsements/my-endorsements';
import { EndorsementDetail } from './pages/endorsement/endorsement-detail/endorsement-detail';
import { PendingEndorsements } from './pages/endorsement/pending-endorsements/pending-endorsements';
import { AllEndorsements } from './pages/endorsement/all-endorsements/all-endorsements';

import { PaymentSchedule } from './pages/payment/payment-schedule/payment-schedule';
import { Invoices } from './pages/payment/invoices/invoices';
import { MakePayment } from './pages/payment/make-payment/make-payment';
import { PaymentHistory } from './pages/payment/payment-history/payment-history';
import { MyPayments } from './pages/payment/my-payments/my-payments';

import { UserList } from './pages/admin/user-list/user-list';
import { UserDetail } from './pages/admin/user-detail/user-detail';
import { CreateStaff } from './pages/admin/create-staff/create-staff';
import { EditStaff } from './pages/admin/edit-staff/edit-staff';
import { PlanManagement } from './pages/admin/plan-management/plan-management';
import { CreatePlan } from './pages/admin/create-plan/create-plan';
import { EditPlan } from './pages/admin/edit-plan/edit-plan';

export const routes: Routes = [
  // ───────── Public Routes ─────────
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'forgot-password', component: ForgotPassword },

  // ───────── Authenticated Routes ─────────
  {
    path: 'app',
    component: Layout,
    canActivate: [authGuard],
    children: [

      // ───── Dashboard ─────
      { path: 'dashboard/admin', component: AdminDashboard, canActivate: [roleGuard], data: { roles: ['Admin'] } },
      { path: 'dashboard/agent', component: AgentDashboard, canActivate: [roleGuard], data: { roles: ['Agent'] } },
      { path: 'dashboard/customer', component: CustomerDashboard, canActivate: [roleGuard], data: { roles: ['Customer'] } },
      { path: 'dashboard/claims-officer', component: ClaimsOfficerDashboard, canActivate: [roleGuard], data: { roles: ['ClaimsOfficer'] } },

      // ───── Common ─────
      { path: 'change-password', component: ChangePassword },

      // ───── Policy ─────
      { path: 'plans', component: PlanList },
      { path: 'plans/:id', component: PlanDetail },
      { path: 'request-policy/:planId', component: RequestPolicy, canActivate: [roleGuard], data: { roles: ['Customer'] } },
      { path: 'my-policies', component: MyPolicies, canActivate: [roleGuard], data: { roles: ['Customer'] } },
      { path: 'policy/:id', component: PolicyDetail },
      { path: 'agent-policies', component: AgentPolicies, canActivate: [roleGuard], data: { roles: ['Agent'] } },
      { path: 'agent-policy-review/:id', component: AgentPolicyReview, canActivate: [roleGuard], data: { roles: ['Agent'] } },
      { path: 'all-policies', component: AllPolicies, canActivate: [roleGuard], data: { roles: ['Admin'] } },

      // ───── Claims ─────
      { path: 'raise-claim', component: RaiseClaim, canActivate: [roleGuard], data: { roles: ['Customer'] } },
      { path: 'my-claims', component: MyClaims, canActivate: [roleGuard], data: { roles: ['Customer'] } },
      { path: 'claim/:id', component: ClaimDetail },
      { path: 'officer-claims', component: OfficerClaims, canActivate: [roleGuard], data: { roles: ['ClaimsOfficer'] } },
      { path: 'all-claims', component: AllClaims, canActivate: [roleGuard], data: { roles: ['Admin'] } },

      // --- Endorsements ---
      { path: 'endorsement/request', component: RequestEndorsement, canActivate: [roleGuard], data: { roles: ['Customer'] } },
      { path: 'endorsement/my-endorsements', component: MyEndorsements, canActivate: [roleGuard], data: { roles: ['Customer'] } },
      { path: 'endorsement/pending', component: PendingEndorsements, canActivate: [roleGuard], data: { roles: ['Agent'] } },
      { path: 'admin/endorsements', component: AllEndorsements, canActivate: [roleGuard], data: { roles: ['Admin'] } },
      { path: 'endorsement/:id', component: EndorsementDetail }, // Shared view

      // --- Payment ---
      { path: 'payment-schedule/:policyId', component: PaymentSchedule },
      { path: 'invoices/:policyId', component: Invoices },
      { path: 'make-payment/:invoiceId', component: MakePayment, canActivate: [roleGuard], data: { roles: ['Customer'] } },
      { path: 'payment-history/:policyId', component: PaymentHistory },
      { path: 'my-payments', component: MyPayments, canActivate: [roleGuard], data: { roles: ['Customer'] } },

      // ───── Admin ─────
      { path: 'admin/users', component: UserList, canActivate: [roleGuard], data: { roles: ['Admin'] } },
      { path: 'admin/users/create', component: CreateStaff, canActivate: [roleGuard], data: { roles: ['Admin'] } },
      { path: 'admin/users/edit/:id', component: EditStaff, canActivate: [roleGuard], data: { roles: ['Admin'] } },
      { path: 'admin/users/:id', component: UserDetail, canActivate: [roleGuard], data: { roles: ['Admin'] } },
      { path: 'admin/plans', component: PlanManagement, canActivate: [roleGuard], data: { roles: ['Admin'] } },
      { path: 'admin/plans/create', component: CreatePlan, canActivate: [roleGuard], data: { roles: ['Admin'] } },
      { path: 'admin/plans/edit/:id', component: EditPlan, canActivate: [roleGuard], data: { roles: ['Admin'] } },

    ]
  },

  // ───────── Wildcard & Error ─────────
  { path: 'error', component: GlobalError },
  { path: '**', redirectTo: '' }
];