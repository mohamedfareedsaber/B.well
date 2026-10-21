import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { BillingCycle, PlanType } from '../../types/api';

export type FamilyMemberForm = {
  fullName: string;
  age?: number;
  relationship?: string;
  nationalIdOrPassport?: string;
};

export type CheckoutState = {
  step: 1 | 2 | 3;
  planId: string | null;
  planName: string | null;
  planType: PlanType;
  billingCycle: BillingCycle;
  priceEgp: number;
  benefits: string[];
  colorGradient: string | null;
  icon: string | null;
  fullName: string;
  mobileNumber: string;
  age: string;
  email: string;
  nationality: string;
  nationalIdOrPassport: string;
  familyMembers: FamilyMemberForm[];
  chronicDiseaseIds: string[];
  medicalNotes: string;
  country: string;
  governorate: string;
  postalCode: string;
  addressDetails: string;
  paymentMethod: 'CreditCard' | 'DigitalWallet' | 'BankTransfer';
  subscriptionId: string | null;
  checkoutUrl: string | null;
  clientSecret: string | null;
  publicKey: string | null;
};

const initialState: CheckoutState = {
  step: 1,
  planId: null,
  planName: null,
  planType: 'Individual',
  billingCycle: 'Annual',
  priceEgp: 0,
  benefits: [],
  colorGradient: null,
  icon: null,
  fullName: '',
  mobileNumber: '',
  age: '',
  email: '',
  nationality: 'Egyptian',
  nationalIdOrPassport: '',
  familyMembers: [],
  chronicDiseaseIds: [],
  medicalNotes: '',
  country: 'Egypt',
  governorate: '',
  postalCode: '',
  addressDetails: '',
  paymentMethod: 'CreditCard',
  subscriptionId: null,
  checkoutUrl: null,
  clientSecret: null,
  publicKey: null,
};

const checkoutSlice = createSlice({
  name: 'checkout',
  initialState,
  reducers: {
    selectPlan: (
      state,
      action: PayloadAction<{
        planId: string;
        planName: string;
        planType: PlanType;
        priceEgp: number;
        benefits: string[];
        colorGradient: string;
        icon: string;
      }>,
    ) => {
      Object.assign(state, initialState, {
        planId: action.payload.planId,
        planName: action.payload.planName,
        planType: action.payload.planType,
        priceEgp: action.payload.priceEgp,
        benefits: action.payload.benefits,
        colorGradient: action.payload.colorGradient,
        icon: action.payload.icon,
        step: 1,
      });
    },
    setStep: (state, action: PayloadAction<1 | 2 | 3>) => {
      state.step = action.payload;
    },
    patchCheckout: (state, action: PayloadAction<Partial<CheckoutState>>) => {
      Object.assign(state, action.payload);
    },
    addFamilyMember: (state) => {
      if (state.familyMembers.length < 4) {
        state.familyMembers.push({ fullName: '', age: undefined, relationship: '', nationalIdOrPassport: '' });
      }
    },
    updateFamilyMember: (
      state,
      action: PayloadAction<{ index: number; patch: Partial<FamilyMemberForm> }>,
    ) => {
      const m = state.familyMembers[action.payload.index];
      if (m) Object.assign(m, action.payload.patch);
    },
    removeFamilyMember: (state, action: PayloadAction<number>) => {
      state.familyMembers.splice(action.payload, 1);
    },
    toggleDisease: (state, action: PayloadAction<string>) => {
      const id = action.payload;
      if (state.chronicDiseaseIds.includes(id)) {
        state.chronicDiseaseIds = state.chronicDiseaseIds.filter((x) => x !== id);
      } else {
        state.chronicDiseaseIds.push(id);
      }
    },
    resetCheckout: () => initialState,
  },
});

export const {
  selectPlan,
  setStep,
  patchCheckout,
  addFamilyMember,
  updateFamilyMember,
  removeFamilyMember,
  toggleDisease,
  resetCheckout,
} = checkoutSlice.actions;

export default checkoutSlice.reducer;
