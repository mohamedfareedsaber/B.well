import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type {
  BenefitRedemption,
  CheckoutSession,
  DashboardOverview,
  LoginResponse,
  LookupItem,
  LoyaltyProgram,
  OrderSummary,
  PatientDetails,
  PatientListItem,
  PatientPortal,
  PaymentConfig,
  PricingCard,
  PlanType,
} from '../types/api';
import type { RootState } from '../app/store';
import { API_BASE_URL } from '../services/http';

export const bwellApi = createApi({
  reducerPath: 'bwellApi',
  baseQuery: fetchBaseQuery({
    baseUrl: API_BASE_URL,
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) headers.set('Authorization', `Bearer ${token}`);
      return headers;
    },
  }),
  tagTypes: ['Programs', 'Patients', 'Dashboard', 'BenefitRedemptions'],
  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, { email: string; password: string }>({
      query: (body) => ({ url: '/api/auth/login', method: 'POST', body }),
      transformResponse: (r: { data: LoginResponse }) => r.data,
    }),
    getPricing: builder.query<PricingCard[], PlanType>({
      query: (planType) => `/api/programs/pricing?planType=${planType}`,
      transformResponse: (r: { data: PricingCard[] }) => r.data,
    }),
    getPrograms: builder.query<LoyaltyProgram[], void>({
      query: () => '/api/programs',
      transformResponse: (r: { data: LoyaltyProgram[] }) => r.data,
      providesTags: ['Programs'],
    }),
    getProgram: builder.query<LoyaltyProgram, string>({
      query: (id) => `/api/programs/${id}`,
      transformResponse: (r: { data: LoyaltyProgram }) => r.data,
    }),
    createProgram: builder.mutation<LoyaltyProgram, Record<string, unknown>>({
      query: (body) => ({ url: '/api/programs', method: 'POST', body }),
      transformResponse: (r: { data: LoyaltyProgram }) => r.data,
      invalidatesTags: ['Programs', 'Dashboard'],
    }),
    deleteProgram: builder.mutation<{ deleted: boolean }, string>({
      query: (id) => ({ url: `/api/programs/${id}`, method: 'DELETE' }),
      transformResponse: (r: { data: { deleted: boolean } }) => r.data,
      invalidatesTags: ['Programs', 'Dashboard'],
    }),
    startCheckout: builder.mutation<CheckoutSession, Record<string, unknown>>({
      query: (body) => ({ url: '/api/subscriptions/checkout/start', method: 'POST', body }),
      transformResponse: (r: { data: CheckoutSession }) => r.data,
    }),
    completePayment: builder.mutation<CheckoutSession, Record<string, unknown>>({
      query: (body) => ({
        url: '/api/subscriptions/checkout/complete-payment',
        method: 'POST',
        body,
      }),
      transformResponse: (r: { data: CheckoutSession }) => r.data,
    }),
    confirmPayment: builder.mutation<OrderSummary, { subscriptionId: string }>({
      query: (body) => ({
        url: '/api/subscriptions/checkout/confirm-payment',
        method: 'POST',
        body,
      }),
      transformResponse: (r: { data: OrderSummary }) => r.data,
    }),
    markPaymentFailed: builder.mutation<OrderSummary, { subscriptionId: string; failureReason?: string }>({
      query: (body) => ({
        url: '/api/subscriptions/checkout/mark-failed',
        method: 'POST',
        body,
      }),
      transformResponse: (r: { data: OrderSummary }) => r.data,
    }),
    getOrderSummary: builder.query<OrderSummary, string>({
      query: (id) => `/api/subscriptions/${id}/order-summary`,
      transformResponse: (r: { data: OrderSummary }) => r.data,
    }),
    getDashboard: builder.query<DashboardOverview, void>({
      query: () => '/api/analytics/dashboard',
      transformResponse: (r: { data: DashboardOverview }) => r.data,
      providesTags: ['Dashboard'],
    }),
    getChronicDiseases: builder.query<LookupItem[], void>({
      query: () => '/api/analytics/lookups/chronic-diseases',
      transformResponse: (r: { data: LookupItem[] }) => r.data,
    }),
    getMedicalServices: builder.query<LookupItem[], void>({
      query: () => '/api/analytics/lookups/medical-services',
      transformResponse: (r: { data: LookupItem[] }) => r.data,
    }),
    getGovernorates: builder.query<LookupItem[], void>({
      query: () => '/api/analytics/lookups/governorates',
      transformResponse: (r: { data: LookupItem[] }) => r.data,
    }),
    getPatients: builder.query<PatientListItem[], string | void>({
      query: (q) => `/api/patients${q ? `?q=${encodeURIComponent(q)}` : ''}`,
      transformResponse: (r: { data: PatientListItem[] }) => r.data,
      providesTags: ['Patients'],
    }),
    getPatientsOverview: builder.query<{ totalPatients: number; totalVisits: number }, void>({
      query: () => '/api/patients/overview',
      transformResponse: (r: { data: { totalPatients: number; totalVisits: number } }) => r.data,
    }),
    getPatient: builder.query<PatientDetails, string>({
      query: (id) => `/api/patients/${id}`,
      transformResponse: (r: { data: PatientDetails }) => r.data,
    }),
    getMyPortal: builder.query<PatientPortal, void>({
      query: () => '/api/portal/me',
      transformResponse: (r: { data: PatientPortal }) => r.data,
    }),
    getMyBenefitRedemptions: builder.query<BenefitRedemption[], void>({
      query: () => '/api/portal/benefit-redemptions',
      transformResponse: (r: { data: BenefitRedemption[] }) => r.data,
      providesTags: ['BenefitRedemptions'],
    }),
    requestBenefitRedemption: builder.mutation<
      BenefitRedemption,
      { benefitName: string; familyMemberId?: string | null; notes?: string }
    >({
      query: (body) => ({ url: '/api/portal/benefit-redemptions', method: 'POST', body }),
      transformResponse: (r: { data: BenefitRedemption }) => r.data,
      invalidatesTags: ['BenefitRedemptions'],
    }),
    getPaymentConfig: builder.query<PaymentConfig, void>({
      query: () => '/api/subscriptions/payment-config',
      transformResponse: (r: { data: PaymentConfig }) => r.data,
    }),
  }),
});

export const {
  useLoginMutation,
  useGetPricingQuery,
  useGetProgramsQuery,
  useGetProgramQuery,
  useCreateProgramMutation,
  useDeleteProgramMutation,
  useStartCheckoutMutation,
  useCompletePaymentMutation,
  useConfirmPaymentMutation,
  useMarkPaymentFailedMutation,
  useGetOrderSummaryQuery,
  useGetDashboardQuery,
  useGetChronicDiseasesQuery,
  useGetMedicalServicesQuery,
  useGetGovernoratesQuery,
  useGetPatientsQuery,
  useGetPatientsOverviewQuery,
  useGetPatientQuery,
  useGetMyPortalQuery,
  useGetMyBenefitRedemptionsQuery,
  useRequestBenefitRedemptionMutation,
  useGetPaymentConfigQuery,
} = bwellApi;
