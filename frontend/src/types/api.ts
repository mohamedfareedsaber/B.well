export type PlanType = 'Individual' | 'Family';
export type BillingCycle = 'Annual' | 'Monthly';
export type ProgramIcon =
  | 'Crown'
  | 'Star'
  | 'Medal'
  | 'Shield'
  | 'Heart'
  | 'Stethoscope'
  | 'Pill'
  | 'MedicalCross'
  | 'Sparkles'
  | 'Award'
  | 'Gift'
  | 'Zap'
  | 'Flame';
export type ColorGradient = 'Purple' | 'Gold' | 'Silver' | 'Bronze' | 'Blue' | 'Green' | 'Red';

export type PricingCard = {
  id: string;
  name: string;
  description: string;
  icon: ProgramIcon;
  colorGradient: ColorGradient;
  priceEgp: number;
  planType: PlanType;
  benefits: string[];
  extraBenefitsCount: number;
};

export type LoyaltyProgram = {
  id: string;
  name: string;
  description: string;
  icon: ProgramIcon;
  colorGradient: ColorGradient;
  individualPriceEgp: number;
  familyPriceEgp: number;
  offerExpiryDate?: string | null;
  isActive: boolean;
  sortOrder: number;
  memberCount: number;
  revenueEgp: number;
  benefits: { id: string; appliesTo: PlanType; text: string; sortOrder: number }[];
  serviceDiscounts: {
    id: string;
    medicalServiceId: string;
    medicalServiceName: string;
    firstVisitDiscountPercent: number;
    secondVisitDiscountPercent: number;
    additionalVisitDiscountPercent: number;
  }[];
};

export type CheckoutSession = {
  subscriptionId: string;
  subscriptionNumber: string;
  planName: string;
  planType: PlanType;
  billingCycle: BillingCycle;
  amountEgp: number;
  clientSecret?: string | null;
  publicKey?: string | null;
  checkoutUrl?: string | null;
  status: string;
};

export type OrderSummary = {
  subscriptionId: string;
  subscriptionNumber: string;
  planName: string;
  planType: PlanType;
  billingCycle: BillingCycle;
  totalPaidEgp: number;
  nextRenewalDateUtc?: string | null;
  status: string;
  paymentStatus?: string | null;
  failureReason?: string | null;
  paymentMethod?: string | null;
};

export type LoginResponse = {
  token: string;
  fullName: string;
  email: string;
  role: string;
  expiresAtUtc: string;
  patientId?: string | null;
};

export type PaymentMethodOption = {
  method: string;
  label: string;
  available: boolean;
  unavailableReason?: string | null;
};

export type PaymentConfig = {
  provider: string;
  publicKey: string;
  isConfigured: boolean;
  useMockPayments: boolean;
  currency: string;
  checkoutBaseHint: string;
  methods: PaymentMethodOption[];
};

export type PatientPortal = {
  patientId: string;
  patientCode: string;
  fullName: string;
  mobileNumber: string;
  email?: string | null;
  age?: number | null;
  nationality?: string | null;
  chronicDiseases: string[];
  plan?: {
    planName: string;
    planType: PlanType;
    billingCycle: BillingCycle;
    status: string;
    amountEgp: number;
    startDateUtc?: string | null;
    nextRenewalDateUtc?: string | null;
    benefits: string[];
    serviceQuotas: {
      serviceName: string;
      visitCount: number;
      lastDiscountPercent?: number | null;
      firstVisitDiscountPercent: number;
      secondVisitDiscountPercent: number;
      additionalVisitDiscountPercent: number;
    }[];
  } | null;
  usage: {
    totalVisits: number;
    visitsThisMonth: number;
    familyMemberCount: number;
    estimatedSavingsEgp: number;
  };
  familyMembers: {
    id: string;
    fullName: string;
    age?: number | null;
    relationship?: string | null;
    visitCount: number;
    visits: {
      id: string;
      visitDateUtc: string;
      medicalServiceName?: string | null;
      visitSequenceForService: number;
      discountAppliedPercent?: number | null;
      memberName?: string | null;
      notes?: string | null;
    }[];
  }[];
  recentVisits: {
    id: string;
    visitDateUtc: string;
    medicalServiceName?: string | null;
    visitSequenceForService: number;
    discountAppliedPercent?: number | null;
    memberName?: string | null;
    notes?: string | null;
  }[];
  deliveryAddress?: {
    country: string;
    governorate: string;
    postalCode: string;
    addressDetails: string;
  } | null;
};

export type DashboardOverview = {
  totalRevenueEgp: number;
  totalSubscriptions: number;
  activePrograms: number;
  averageValueEgp: number;
  programPerformance: {
    programId: string;
    name: string;
    icon: string;
    color: string;
    memberCount: number;
    revenueEgp: number;
    sharePercent: number;
  }[];
  chronicDiseaseStats: {
    percentWithConditions: number;
    membersWithConditions: number;
    totalMembers: number;
    breakdown: { name: string; memberCount: number; percent: number }[];
  };
  geographicDistribution: {
    governorateId: string;
    name: string;
    nameAr: string;
    latitude: number;
    longitude: number;
    memberCount: number;
    densityLevel: string;
  }[];
};

export type PatientListItem = {
  id: string;
  patientCode: string;
  fullName: string;
  mobileNumber: string;
  email?: string | null;
  subscribedAtUtc: string;
  visitCount: number;
  planName?: string | null;
  planType?: PlanType | null;
  chronicDiseases: string[];
};

export type PatientDetails = {
  id: string;
  patientCode: string;
  fullName: string;
  mobileNumber: string;
  email?: string | null;
  age?: number | null;
  nationality?: string | null;
  nationalIdOrPassport?: string | null;
  medicalNotes?: string | null;
  createdAtUtc: string;
  activePlanName?: string | null;
  activePlanType?: PlanType | null;
  subscriptionStatus?: string | null;
  nextRenewalDateUtc?: string | null;
  chronicDiseases: string[];
  familyMembers: {
    id: string;
    fullName: string;
    age?: number | null;
    relationship?: string | null;
    nationalIdOrPassport?: string | null;
  }[];
  visits: {
    id: string;
    visitDateUtc: string;
    medicalServiceName?: string | null;
    visitSequenceForService: number;
    discountAppliedPercent?: number | null;
    notes?: string | null;
  }[];
  deliveryAddress?: {
    country: string;
    governorate: string;
    postalCode: string;
    addressDetails: string;
  } | null;
};

export type LookupItem = { id: string; name: string; code?: string; description?: string | null };
